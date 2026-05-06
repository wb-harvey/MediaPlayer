Imports System.Diagnostics

''' <summary>
''' Measures media engine load time, memory footprint, and CPU usage.
''' Uses System.Windows.Forms.Timer so all events fire on the UI thread —
''' no BeginInvoke needed in the player form's StatsUpdated handler.
''' </summary>
Public Class MediaBenchmark

    ' ── Private state ─────────────────────────────────────────────────────────

    Private _loadWatch As Stopwatch
    Private _sampleTimer As System.Windows.Forms.Timer
    Private _proc As Process

    Private _baselineMemory As Long     ' WorkingSet64 before Play() was called
    Private _currentMemory As Long
    Private _peakMemory As Long

    Private _prevCpuTime As TimeSpan
    Private _prevSampleTime As DateTime
    Private _currentCpu As Double
    Private _peakCpu As Double

    Private _loadTimeMs As Long = -1    ' -1 = not yet loaded

    ' ── Event ─────────────────────────────────────────────────────────────────

    ''' <summary>Fires on the UI thread every 500 ms once playback has started.</summary>
    Public Event StatsUpdated As EventHandler

    ' ── Read-only result properties ───────────────────────────────────────────

    ''' <summary>Milliseconds from Play() call to MediaStarted/Playing event. -1 if not yet loaded.</summary>
    Public ReadOnly Property LoadTimeMs As Long
        Get
            Return _loadTimeMs
        End Get
    End Property

    Public ReadOnly Property LoadTimeText As String
        Get
            If _loadTimeMs < 0 Then Return "loading…"
            Return _loadTimeMs.ToString("N0") & " ms"
        End Get
    End Property

    ''' <summary>Process working-set at the moment Play() was called (MB).</summary>
    Public ReadOnly Property BaselineMemoryMB As Double
        Get
            Return _baselineMemory / 1048576.0
        End Get
    End Property

    ''' <summary>Current process working-set (MB).</summary>
    Public ReadOnly Property CurrentMemoryMB As Double
        Get
            Return _currentMemory / 1048576.0
        End Get
    End Property

    ''' <summary>Change in working-set since Play() was called (MB). Positive = grew.</summary>
    Public ReadOnly Property DeltaMemoryMB As Double
        Get
            Return (_currentMemory - _baselineMemory) / 1048576.0
        End Get
    End Property

    ''' <summary>Peak working-set observed since sampling started (MB).</summary>
    Public ReadOnly Property PeakMemoryMB As Double
        Get
            Return _peakMemory / 1048576.0
        End Get
    End Property

    ''' <summary>Current process CPU utilisation (0-100 %).</summary>
    Public ReadOnly Property CurrentCpuPct As Double
        Get
            Return _currentCpu
        End Get
    End Property

    ''' <summary>Peak CPU utilisation observed since sampling started.</summary>
    Public ReadOnly Property PeakCpuPct As Double
        Get
            Return _peakCpu
        End Get
    End Property

    ''' <summary>One-line summary suitable for a status label.</summary>
    Public ReadOnly Property SummaryLine As String
        Get
            Return String.Format(
                "Load: {0}   |   Mem Δ: {1:+0.0;-0.0} MB  ({2:0.0} MB / peak {3:0.0} MB)   |   CPU: {4:0.0}%  (peak {5:0.0}%)",
                LoadTimeText,
                DeltaMemoryMB,
                CurrentMemoryMB,
                PeakMemoryMB,
                _currentCpu,
                _peakCpu)
        End Get
    End Property

    ''' <summary>Multi-line summary suitable for a report or txtError.</summary>
    Public ReadOnly Property ReportText As String
        Get
            Return String.Format(
                "  Load time   : {0}{1}" &
                "  Mem at open : {2:0.0} MB{1}" &
                "  Mem now     : {3:0.0} MB  (Δ {4:+0.0;-0.0} MB){1}" &
                "  Mem peak    : {5:0.0} MB{1}" &
                "  CPU now     : {6:0.0}%{1}" &
                "  CPU peak    : {7:0.0}%{1}" &
                "  Processors  : {8}",
                LoadTimeText, vbCrLf,
                BaselineMemoryMB,
                CurrentMemoryMB, DeltaMemoryMB,
                PeakMemoryMB,
                _currentCpu,
                _peakCpu,
                Environment.ProcessorCount)
        End Get
    End Property

    ' ── Control ───────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Call immediately before calling Play() on the media engine.
    ''' Takes a baseline memory snapshot and starts the load-time stopwatch.
    ''' </summary>
    Public Sub StartLoad()
        _proc = Process.GetCurrentProcess()
        _proc.Refresh()

        _baselineMemory = _proc.WorkingSet64
        _peakMemory = _baselineMemory
        _currentMemory = _baselineMemory

        _prevCpuTime = _proc.TotalProcessorTime
        _prevSampleTime = DateTime.UtcNow
        _currentCpu = 0
        _peakCpu = 0
        _loadTimeMs = -1

        _loadWatch = Stopwatch.StartNew()
    End Sub

    ''' <summary>
    ''' Call from the MediaStarted / Playing event handler to record the load time
    ''' and begin periodic resource sampling.
    ''' Must be called on the UI thread (or via Invoke/BeginInvoke).
    ''' </summary>
    Public Sub MarkLoaded()
        If _loadWatch IsNot Nothing AndAlso _loadWatch.IsRunning Then
            _loadWatch.Stop()
            _loadTimeMs = _loadWatch.ElapsedMilliseconds
        End If

        ' Start the UI-thread sampling timer (fires on UI thread — no marshalling needed)
        If _sampleTimer Is Nothing Then
            _sampleTimer = New System.Windows.Forms.Timer With {.Interval = 500}
            AddHandler _sampleTimer.Tick, AddressOf OnSampleTick
            _sampleTimer.Start()
        End If
    End Sub

    ''' <summary>Stop sampling. Call when the player form closes.</summary>
    Public Sub StopSampling()
        If _sampleTimer IsNot Nothing Then
            _sampleTimer.Stop()
            _sampleTimer.Dispose()
            _sampleTimer = Nothing
        End If
        If _loadWatch IsNot Nothing AndAlso _loadWatch.IsRunning Then
            _loadWatch.Stop()
        End If
    End Sub

    ' ── Internal sampling ─────────────────────────────────────────────────────

    Private Sub OnSampleTick(sender As Object, e As EventArgs)
        Sample()
        RaiseEvent StatsUpdated(Me, EventArgs.Empty)
    End Sub

    Private Sub Sample()
        Try
            _proc.Refresh()

            ' Memory
            _currentMemory = _proc.WorkingSet64
            If _currentMemory > _peakMemory Then _peakMemory = _currentMemory

            ' CPU — delta of TotalProcessorTime divided by wall-clock delta and core count
            Dim now As DateTime = DateTime.UtcNow
            Dim cpuNow As TimeSpan = _proc.TotalProcessorTime
            Dim cpuDeltaMs As Double = (cpuNow - _prevCpuTime).TotalMilliseconds
            Dim wallDeltaMs As Double = (now - _prevSampleTime).TotalMilliseconds

            If wallDeltaMs > 10 Then
                _currentCpu = Math.Min(100.0,
                    cpuDeltaMs / (wallDeltaMs * Environment.ProcessorCount) * 100.0)
                If _currentCpu > _peakCpu Then _peakCpu = _currentCpu
            End If

            _prevCpuTime = cpuNow
            _prevSampleTime = now

        Catch
            ' Silently ignore transient refresh errors
        End Try
    End Sub

End Class
