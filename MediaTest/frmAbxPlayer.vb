Imports ABX.MediaPlayer

Public Class frmAbxPlayer

    Private _filePath As String
    Private _player As Player
    Private _bench As New MediaBenchmark()
    Private _disposed As Boolean = False

    ' ── Clip bounds (0 = full file) ───────────────────────────────────────────
    Private _startMs As Long = 0
    Private _stopMs As Long = 0
    Private ReadOnly Property IsClip As Boolean
        Get
            Return _startMs > 0 OrElse _stopMs > 0
        End Get
    End Property

    ' ── Fullscreen state ──────────────────────────────────────────────────────
    ' We do NOT use _player.FullScreen — it can deadlock the message pump when
    ' called from a button click. Instead we manually toggle form state.
    Private _isFullScreen As Boolean = False
    Private _savedBorder As FormBorderStyle
    Private _savedState As FormWindowState

    Public Sub New(filePath As String)
        InitializeComponent()
        _filePath = filePath
        Me.Text = "ABX MediaPlayer — " & IO.Path.GetFileName(filePath)
    End Sub

    ''' <summary>Opens a clip window that plays only startMs–stopMs of the file.</summary>
    Public Sub New(filePath As String, startMs As Long, stopMs As Long)
        InitializeComponent()
        _filePath = filePath
        _startMs = startMs
        _stopMs = stopMs
        Me.Text = String.Format("ABX Clip — {0}  [{1}s–{2}s]",
                                IO.Path.GetFileName(filePath),
                                startMs \ 1000, stopMs \ 1000)
    End Sub

    Private Sub frmAbxPlayer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            _player = New Player(pnlDisplay)

            AddHandler _player.Events.MediaStarted, AddressOf OnMediaStarted
            AddHandler _player.Events.MediaEnded, AddressOf OnMediaEnded
            AddHandler _player.Events.MediaPausedChanged, AddressOf OnMediaPausedChanged
            AddHandler _bench.StatsUpdated, AddressOf OnStatsUpdated

            _bench.StartLoad()
            Dim result As Integer
            If IsClip Then
                result = _player.Play(_filePath,
                                      TimeSpan.FromMilliseconds(_startMs),
                                      TimeSpan.FromMilliseconds(_stopMs))
            Else
                result = _player.Play(_filePath)
            End If

            If result <> 0 Then
                lblStatus.Text = "Error loading media. Code: " & result.ToString("X8")
                lblStats.Text = "Benchmark: play failed"
            Else
                lblStatus.Text = "Playing: " & IO.Path.GetFileName(_filePath)
            End If

        Catch ex As Exception
            lblStatus.Text = "Error: " & ex.Message
            MessageBox.Show("Failed to initialize ABX.MediaPlayer:" & vbCrLf & ex.Message,
                            "ABX Player Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    ' ── Media Events ──────────────────────────────────────────────────────────

    Private Sub OnMediaStarted(sender As Object, e As EventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() OnMediaStarted(sender, e))
            Return
        End If
        _bench.MarkLoaded()
        lblStatus.Text = "Playing: " & IO.Path.GetFileName(_filePath)
        UpdateButtons()
    End Sub

    Private Sub OnMediaEnded(sender As Object, e As EndedEventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() OnMediaEnded(sender, e))
            Return
        End If
        lblStatus.Text = "Finished: " & IO.Path.GetFileName(_filePath)
        UpdateButtons()
    End Sub

    Private Sub OnMediaPausedChanged(sender As Object, e As EventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(Sub() OnMediaPausedChanged(sender, e))
            Return
        End If
        UpdateButtons()
    End Sub

    Private Sub OnStatsUpdated(sender As Object, e As EventArgs)
        lblStats.Text = "ABX  " & _bench.SummaryLine
    End Sub

    Private Sub UpdateButtons()
        If _player Is Nothing Then Return
        If _player.Playing Then
            btnPlayPause.Text = If(_player.Paused, "▶ Resume", "⏸ Pause")
            btnStop.Enabled = True
        Else
            btnPlayPause.Text = "▶ Play"
            btnStop.Enabled = False
        End If
    End Sub

    ' ── Transport Controls ────────────────────────────────────────────────────

    Private Sub btnPlayPause_Click(sender As Object, e As EventArgs) Handles btnPlayPause.Click
        If _player Is Nothing Then Return
        If _player.Playing Then
            If _player.Paused Then _player.Resume() Else _player.Pause()
        Else
            _bench.StartLoad()
            If IsClip Then
                _player.Play(_filePath,
                             TimeSpan.FromMilliseconds(_startMs),
                             TimeSpan.FromMilliseconds(_stopMs))
            Else
                _player.Play(_filePath)
            End If
        End If
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        If _player Is Nothing Then Return
        _player.Stop()
        lblStatus.Text = "Stopped"
        UpdateButtons()
    End Sub

    Private Sub btnRestart_Click(sender As Object, e As EventArgs) Handles btnRestart.Click
        If _player Is Nothing Then Return
        _bench.StartLoad()
        If IsClip Then
            _player.Play(_filePath,
                         TimeSpan.FromMilliseconds(_startMs),
                         TimeSpan.FromMilliseconds(_stopMs))
        Else
            _player.Play(_filePath)
        End If
    End Sub

    Private Sub btnFullScreen_Click(sender As Object, e As EventArgs) Handles btnFullScreen.Click
        ToggleFullScreen()
    End Sub

    Private Sub trkVolume_Scroll(sender As Object, e As EventArgs) Handles trkVolume.Scroll
        If _player Is Nothing Then Return
        _player.Audio.Volume = CSng(trkVolume.Value) / 100.0F
        lblVolume.Text = "Vol: " & trkVolume.Value & "%"
    End Sub

    ' ── Fullscreen (form-level — avoids ABX message-pump deadlock) ───────────

    Private Sub ToggleFullScreen()
        If _isFullScreen Then
            ' Restore
            FormBorderStyle = _savedBorder
            WindowState = _savedState
            pnlControls.Visible = True
            pnlStats.Visible = True
            btnFullScreen.Text = "⛶ Full Screen"
            _isFullScreen = False
        Else
            ' Save state then go fullscreen
            _savedBorder = FormBorderStyle
            _savedState = WindowState
            pnlControls.Visible = False
            pnlStats.Visible = False
            FormBorderStyle = FormBorderStyle.None
            WindowState = FormWindowState.Maximized
            btnFullScreen.Text = "⊠ Restore"
            _isFullScreen = True
        End If
    End Sub

    Private Sub frmAbxPlayer_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape AndAlso _isFullScreen Then
            ToggleFullScreen()
        End If
    End Sub

    ' ── Close ─────────────────────────────────────────────────────────────────

    Private Sub frmAbxPlayer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CleanUp()
    End Sub

    Private Sub CleanUp()
        If _disposed Then Return
        _disposed = True

        ' Exit fullscreen first so the player releases any window hooks cleanly
        If _isFullScreen Then ToggleFullScreen()

        _bench.StopSampling()

        Try
            If _player IsNot Nothing Then
                RemoveHandler _player.Events.MediaStarted, AddressOf OnMediaStarted
                RemoveHandler _player.Events.MediaEnded, AddressOf OnMediaEnded
                RemoveHandler _player.Events.MediaPausedChanged, AddressOf OnMediaPausedChanged
                _player.Dispose()
                _player = Nothing
            End If
        Catch
        End Try
    End Sub

End Class
