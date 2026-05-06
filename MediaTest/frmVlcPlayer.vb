Imports LibVLCSharp.Shared
Imports LibVLCSharp.WinForms

Public Class frmVlcPlayer

    Private _filePath As String
    Private _mediaPlayer As LibVLCSharp.Shared.MediaPlayer
    Private _videoView As VideoView
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
    Private _isFullScreen As Boolean = False
    Private _savedBorder As FormBorderStyle
    Private _savedState As FormWindowState

    Public Sub New(filePath As String)
        InitializeComponent()
        _filePath = filePath
        Me.Text = "LibVLC Player — " & IO.Path.GetFileName(filePath)
    End Sub

    ''' <summary>Opens a clip window that plays only startMs–stopMs of the file.</summary>
    Public Sub New(filePath As String, startMs As Long, stopMs As Long)
        InitializeComponent()
        _filePath = filePath
        _startMs = startMs
        _stopMs = stopMs
        Me.Text = String.Format("VLC Clip — {0}  [{1}s–{2}s]",
                                IO.Path.GetFileName(filePath),
                                startMs \ 1000, stopMs \ 1000)
    End Sub

    Private Sub frmVlcPlayer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            Dim libVlc As LibVLC = VlcEngine.GetLibVlc()

            If libVlc Is Nothing Then
                Dim reason As String = If(VlcEngine.LastError,
                    "Unknown error — check that libvlc.dll is present in the output folder.")
                Throw New Exception("LibVLC engine failed to initialize:" & vbCrLf & reason)
            End If

            _mediaPlayer = New LibVLCSharp.Shared.MediaPlayer(libVlc)

            _videoView = New VideoView()
            _videoView.Dock = DockStyle.Fill
            _videoView.MediaPlayer = _mediaPlayer
            pnlDisplay.Controls.Add(_videoView)

            AddHandler _mediaPlayer.Playing, AddressOf OnPlaying
            AddHandler _mediaPlayer.TimeChanged, AddressOf OnTimeChanged
            AddHandler _mediaPlayer.Paused, AddressOf OnPaused
            AddHandler _mediaPlayer.Stopped, AddressOf OnStopped
            AddHandler _mediaPlayer.EndReached, AddressOf OnEndReached
            AddHandler _bench.StatsUpdated, AddressOf OnStatsUpdated

            ' ── Start benchmark then immediately play ─────────────────────────
            _firstFrameSeen = False
            _bench.StartLoad()
            Dim media As New LibVLCSharp.Shared.Media(libVlc, _filePath, FromType.FromPath)
            _mediaPlayer.Play(media)
            media.Dispose()

            _mediaPlayer.Volume = trkVolume.Value
            lblStatus.Text = "Playing: " & IO.Path.GetFileName(_filePath)

        Catch ex As Exception
            lblStatus.Text = "Error: " & ex.Message
            MessageBox.Show("Failed to start LibVLC player:" & vbCrLf & ex.Message,
                            "LibVLC Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    ' ── Media Events (LibVLC fires these on a background thread) ─────────────

    Private Sub OnPlaying(sender As Object, e As EventArgs)
        If IsClip Then
            ' AccurateSeek uses libvlc_media_player_set_time(..., b_fast:=False)
            ' which decodes silently from the keyframe to the exact ms target.
            ' Must be in Task.Run — calling any LibVLC API from inside a LibVLC
            ' event callback deadlocks the event thread.
            Task.Run(Sub() VlcEngine.AccurateSeek(_mediaPlayer, _startMs))
        End If

        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
        Me.BeginInvoke(Sub()
                           lblStatus.Text = "Playing: " & IO.Path.GetFileName(_filePath)
                           UpdateButtons(True, False)
                       End Sub)
    End Sub

    Private _firstFrameSeen As Boolean = False

    ''' <summary>
    ''' Creates a Media object and starts playback.
    ''' Clip seeking is done in OnPlaying (more reliable than :start-time option).
    ''' </summary>
    Private Sub PlayMedia(libVlc As LibVLC)
        Dim media As New LibVLCSharp.Shared.Media(libVlc, _filePath, FromType.FromPath)
        _mediaPlayer.Play(media)
        media.Dispose()
    End Sub

    Private Sub OnTimeChanged(sender As Object, e As LibVLCSharp.Shared.MediaPlayerTimeChangedEventArgs)
        ' Hard-stop at clip end. Stop() from inside a LibVLC event callback also
        ' deadlocks (same lock re-entrancy issue) — dispatch to thread pool.
        If _stopMs > 0 AndAlso e.Time >= _stopMs Then
            Task.Run(Sub() _mediaPlayer.Stop())
            Return
        End If

        ' First-frame detection for load-time benchmarking.
        If e.Time > 0 AndAlso Not _firstFrameSeen Then
            _firstFrameSeen = True
            If Not Me.IsDisposed AndAlso Me.IsHandleCreated Then
                Me.BeginInvoke(Sub() _bench.MarkLoaded())
            End If
        End If
    End Sub

    Private Sub OnPaused(sender As Object, e As EventArgs)
        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
        Me.BeginInvoke(Sub()
                           lblStatus.Text = "Paused: " & IO.Path.GetFileName(_filePath)
                           UpdateButtons(True, True)
                       End Sub)
    End Sub

    Private Sub OnStopped(sender As Object, e As EventArgs)
        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
        Me.BeginInvoke(Sub()
                           lblStatus.Text = "Stopped"
                           UpdateButtons(False, False)
                       End Sub)
    End Sub

    Private Sub OnEndReached(sender As Object, e As EventArgs)
        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
        Me.BeginInvoke(Sub()
                           lblStatus.Text = "Finished: " & IO.Path.GetFileName(_filePath)
                           UpdateButtons(False, False)
                       End Sub)
    End Sub

    ' ── Benchmark stats update (fires on UI thread via WinForms Timer) ────────

    Private Sub OnStatsUpdated(sender As Object, e As EventArgs)
        lblStats.Text = "VLC  " & _bench.SummaryLine
    End Sub

    Private Sub UpdateButtons(isActive As Boolean, isPaused As Boolean)
        Select Case _mediaPlayer?.State
            Case VLCState.Playing
                btnPlayPause.Text = "⏸ Pause"
                btnStop.Enabled = True
            Case VLCState.Paused
                btnPlayPause.Text = "▶ Resume"
                btnStop.Enabled = True
            Case Else
                btnPlayPause.Text = "▶ Play"
                btnStop.Enabled = False
        End Select
    End Sub

    ' ── Transport Controls ────────────────────────────────────────────────────

    Private Sub btnPlayPause_Click(sender As Object, e As EventArgs) Handles btnPlayPause.Click
        If _mediaPlayer Is Nothing Then Return

        Select Case _mediaPlayer.State
            Case VLCState.Playing
                ' Currently playing → pause
                _mediaPlayer.SetPause(True)

            Case VLCState.Paused
                ' Currently paused → resume from current position
                _mediaPlayer.SetPause(False)

            Case Else
                ' Stopped, Ended, NothingSpecial, Error → start fresh
                Dim libVlc As LibVLC = VlcEngine.GetLibVlc()
                If libVlc IsNot Nothing Then
                    _firstFrameSeen = False
                    _bench.StartLoad()
                    PlayMedia(libVlc)
                End If
        End Select
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        If _mediaPlayer Is Nothing Then Return
        _mediaPlayer.Stop()
    End Sub

    Private Sub btnRestart_Click(sender As Object, e As EventArgs) Handles btnRestart.Click
        If _mediaPlayer Is Nothing Then Return
        Dim libVlc As LibVLC = VlcEngine.GetLibVlc()
        If libVlc IsNot Nothing Then
            ' Reset first-frame flag then start benchmark + play
            _firstFrameSeen = False
            _bench.StartLoad()
            PlayMedia(libVlc)
        End If
    End Sub

    Private Sub btnFullScreen_Click(sender As Object, e As EventArgs) Handles btnFullScreen.Click
        ToggleFullScreen()
    End Sub

    Private Sub ToggleFullScreen()
        If _isFullScreen Then
            FormBorderStyle = _savedBorder
            WindowState = _savedState
            pnlControls.Visible = True
            pnlStats.Visible = True
            btnFullScreen.Text = "⛶ Full Screen"
            _isFullScreen = False
        Else
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

    Private Sub frmVlcPlayer_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape AndAlso _isFullScreen Then
            ToggleFullScreen()
        End If
    End Sub

    Private Sub trkVolume_Scroll(sender As Object, e As EventArgs) Handles trkVolume.Scroll
        If _mediaPlayer Is Nothing Then Return
        _mediaPlayer.Volume = trkVolume.Value
        lblVolume.Text = "Vol: " & trkVolume.Value & "%"
    End Sub

    ' ── Close ─────────────────────────────────────────────────────────────────

    Private Sub frmVlcPlayer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CleanUp()
    End Sub

    Private Sub CleanUp()
        If _disposed Then Return
        _disposed = True

        If _isFullScreen Then ToggleFullScreen()
        _bench.StopSampling()

        Try
            If _mediaPlayer IsNot Nothing Then
                RemoveHandler _mediaPlayer.Playing, AddressOf OnPlaying
                RemoveHandler _mediaPlayer.TimeChanged, AddressOf OnTimeChanged
                RemoveHandler _mediaPlayer.Paused, AddressOf OnPaused
                RemoveHandler _mediaPlayer.Stopped, AddressOf OnStopped
                RemoveHandler _mediaPlayer.EndReached, AddressOf OnEndReached
                _mediaPlayer.Stop()
                _mediaPlayer.Dispose()
                _mediaPlayer = Nothing
            End If
        Catch
        End Try
    End Sub

End Class
