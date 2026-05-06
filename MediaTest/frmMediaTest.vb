Public Class frmMediaTest

    Public sTarget As String = ""

    Private Sub mnuFileExit_Click(sender As Object, e As EventArgs) Handles mnuFileExit.Click
        DoShutDown()
    End Sub

    Private Sub DoShutDown()
        Application.Exit()
    End Sub

    Private Sub cmdBrowse_Click(sender As Object, e As EventArgs) Handles cmdBrowse.Click
        DoBrowse()
    End Sub

    Private Sub DoBrowse()

        Using ofd As New OpenFileDialog()
            ofd.Title = "Select a Media File"
            ofd.Filter = "Media Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mp3;*.wav;*.aac;*.m4a;*.ogg;*.flac;*.webm;*.ts;*.m2ts;*.mts|" &
                         "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.ts;*.m2ts;*.mts|" &
                         "Audio Files|*.mp3;*.wav;*.aac;*.m4a;*.ogg;*.flac|" &
                         "All Files|*.*"
            ofd.FilterIndex = 1
            ofd.RestoreDirectory = True

            If Not String.IsNullOrWhiteSpace(sTarget) AndAlso IO.File.Exists(sTarget) Then
                ofd.InitialDirectory = IO.Path.GetDirectoryName(sTarget)
                ofd.FileName = IO.Path.GetFileName(sTarget)
            End If

            If ofd.ShowDialog() = DialogResult.OK Then
                sTarget = ofd.FileName
                txtTarget.Text = sTarget
            End If
        End Using

    End Sub

    Private Sub cmdOpenAbx_Click(sender As Object, e As EventArgs) Handles cmdOpenAbx.Click

        ' Validate target file
        sTarget = txtTarget.Text.Trim()

        If String.IsNullOrWhiteSpace(sTarget) Then
            MessageBox.Show("Please select or enter a media file path first.", "No File Selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not IO.File.Exists(sTarget) Then
            MessageBox.Show("The specified file was not found:" & vbCrLf & sTarget,
                            "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' Launch ABX player window
        Dim frm As New frmAbxPlayer(sTarget)
        frm.Show()

    End Sub

    Private Sub cmdOpenVlc_Click(sender As Object, e As EventArgs) Handles cmdOpenVlc.Click
        If Not ValidateTarget() Then Return
        Dim frm As New frmVlcPlayer(sTarget)
        frm.Show()
        If Not VlcEngine.IsReady Then
            AppendError("VLC player failed to open." & vbCrLf &
                        "Engine last error: " & If(VlcEngine.LastError, "(none)"))
        End If
    End Sub

    Private Sub cmdAbxClip_Click(sender As Object, e As EventArgs) Handles cmdAbxClip.Click
        If Not ValidateTarget() Then Return
        Dim frm As New frmAbxPlayer(sTarget, startMs:=10000, stopMs:=20000)
        frm.Show()
    End Sub

    Private Sub cmdVlcClip_Click(sender As Object, e As EventArgs) Handles cmdVlcClip.Click
        If Not ValidateTarget() Then Return
        Dim frm As New frmVlcPlayer(sTarget, startMs:=10000, stopMs:=20000)
        frm.Show()
        If Not VlcEngine.IsReady Then
            AppendError("VLC player failed to open." & vbCrLf &
                        "Engine last error: " & If(VlcEngine.LastError, "(none)"))
        End If
    End Sub

    ''' <summary>Validates sTarget and shows appropriate error. Returns True if OK to proceed.</summary>
    Private Function ValidateTarget() As Boolean
        sTarget = txtTarget.Text.Trim()
        If String.IsNullOrWhiteSpace(sTarget) Then
            MessageBox.Show("Please select or enter a media file path first.", "No File Selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If Not IO.File.Exists(sTarget) Then
            MessageBox.Show("The specified file was not found:" & vbCrLf & sTarget,
                            "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If
        Return True
    End Function

    Private Sub frmMediaTest_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        sTarget = "C:\My Music\My Test Media\sample.mp4"

        If IO.File.Exists(sTarget) Then
            txtTarget.Text = sTarget
        Else
            txtTarget.Text = ""
        End If

        txtError.Text = "LibVLC: Starting background init..." & vbCrLf &
                        "Process: " & If(IntPtr.Size = 8, "64-bit", "32-bit") & vbCrLf &
                        "BaseDir: " & AppDomain.CurrentDomain.BaseDirectory

        ' Pre-init VLC in background; WhenReady fires when done and updates txtError
        VlcEngine.WhenReady(Sub(ok As Boolean, diag As String)
            If Me.IsHandleCreated AndAlso Not Me.IsDisposed Then
                Me.BeginInvoke(Sub() txtError.Text = diag)
            End If
        End Sub)

    End Sub

    Private Sub frmMediaTest_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        VlcEngine.Shutdown()
    End Sub

    Private Sub txtTarget_TextChanged(sender As Object, e As EventArgs) Handles txtTarget.TextChanged
        sTarget = txtTarget.Text.Trim()
    End Sub

    ' Helper to append a line to txtError without clearing existing content
    Private Sub AppendError(msg As String)
        If txtError.TextLength > 0 Then
            txtError.AppendText(vbCrLf & msg)
        Else
            txtError.Text = msg
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles cmdAbxClip.Click

    End Sub

    Private Sub txtError_TextChanged(sender As Object, e As EventArgs) Handles txtError.TextChanged

    End Sub
End Class

