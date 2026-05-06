<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmVlcPlayer
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlDisplay = New System.Windows.Forms.Panel()
        Me.pnlControls = New System.Windows.Forms.Panel()
        Me.pnlStats = New System.Windows.Forms.Panel()
        Me.lblStats = New System.Windows.Forms.Label()
        Me.btnPlayPause = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnRestart = New System.Windows.Forms.Button()
        Me.btnFullScreen = New System.Windows.Forms.Button()
        Me.trkVolume = New System.Windows.Forms.TrackBar()
        Me.lblVolume = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.pnlControls.SuspendLayout()
        Me.pnlStats.SuspendLayout()
        CType(Me.trkVolume, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlDisplay
        '
        Me.pnlDisplay.BackColor = System.Drawing.Color.Black
        Me.pnlDisplay.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlDisplay.Location = New System.Drawing.Point(0, 0)
        Me.pnlDisplay.Name = "pnlDisplay"
        Me.pnlDisplay.Size = New System.Drawing.Size(900, 540)
        Me.pnlDisplay.TabIndex = 0
        '
        'pnlControls
        '
        Me.pnlControls.BackColor = System.Drawing.Color.FromArgb(20, 20, 40)
        Me.pnlControls.Controls.Add(Me.lblStatus)
        Me.pnlControls.Controls.Add(Me.lblVolume)
        Me.pnlControls.Controls.Add(Me.trkVolume)
        Me.pnlControls.Controls.Add(Me.btnFullScreen)
        Me.pnlControls.Controls.Add(Me.btnRestart)
        Me.pnlControls.Controls.Add(Me.btnStop)
        Me.pnlControls.Controls.Add(Me.btnPlayPause)
        Me.pnlControls.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlControls.Location = New System.Drawing.Point(0, 540)
        Me.pnlControls.Name = "pnlControls"
        Me.pnlControls.Size = New System.Drawing.Size(900, 72)
        Me.pnlControls.TabIndex = 1
        '
        'btnPlayPause
        '
        Me.btnPlayPause.BackColor = System.Drawing.Color.FromArgb(255, 102, 0)
        Me.btnPlayPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPlayPause.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnPlayPause.ForeColor = System.Drawing.Color.White
        Me.btnPlayPause.Location = New System.Drawing.Point(12, 18)
        Me.btnPlayPause.Name = "btnPlayPause"
        Me.btnPlayPause.Size = New System.Drawing.Size(100, 36)
        Me.btnPlayPause.TabIndex = 0
        Me.btnPlayPause.Text = "⏸ Pause"
        Me.btnPlayPause.UseVisualStyleBackColor = False
        '
        'btnStop
        '
        Me.btnStop.BackColor = System.Drawing.Color.FromArgb(60, 60, 80)
        Me.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnStop.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnStop.ForeColor = System.Drawing.Color.White
        Me.btnStop.Location = New System.Drawing.Point(120, 18)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(90, 36)
        Me.btnStop.TabIndex = 1
        Me.btnStop.Text = "⏹ Stop"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnRestart
        '
        Me.btnRestart.BackColor = System.Drawing.Color.FromArgb(60, 60, 80)
        Me.btnRestart.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRestart.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnRestart.ForeColor = System.Drawing.Color.White
        Me.btnRestart.Location = New System.Drawing.Point(218, 18)
        Me.btnRestart.Name = "btnRestart"
        Me.btnRestart.Size = New System.Drawing.Size(90, 36)
        Me.btnRestart.TabIndex = 2
        Me.btnRestart.Text = "↩ Restart"
        Me.btnRestart.UseVisualStyleBackColor = False
        '
        'btnFullScreen
        '
        Me.btnFullScreen.BackColor = System.Drawing.Color.FromArgb(60, 60, 80)
        Me.btnFullScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnFullScreen.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnFullScreen.ForeColor = System.Drawing.Color.White
        Me.btnFullScreen.Location = New System.Drawing.Point(316, 18)
        Me.btnFullScreen.Name = "btnFullScreen"
        Me.btnFullScreen.Size = New System.Drawing.Size(110, 36)
        Me.btnFullScreen.TabIndex = 3
        Me.btnFullScreen.Text = "⛶ Full Screen"
        Me.btnFullScreen.UseVisualStyleBackColor = False
        '
        'trkVolume
        '
        Me.trkVolume.BackColor = System.Drawing.Color.FromArgb(20, 20, 40)
        Me.trkVolume.Location = New System.Drawing.Point(460, 14)
        Me.trkVolume.Maximum = 100
        Me.trkVolume.Name = "trkVolume"
        Me.trkVolume.Size = New System.Drawing.Size(180, 45)
        Me.trkVolume.TabIndex = 4
        Me.trkVolume.TickFrequency = 10
        Me.trkVolume.Value = 100
        '
        'lblVolume
        '
        Me.lblVolume.AutoSize = True
        Me.lblVolume.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblVolume.ForeColor = System.Drawing.Color.Silver
        Me.lblVolume.Location = New System.Drawing.Point(646, 25)
        Me.lblVolume.Name = "lblVolume"
        Me.lblVolume.Size = New System.Drawing.Size(52, 15)
        Me.lblVolume.TabIndex = 5
        Me.lblVolume.Text = "Vol: 100%"
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.5!)
        Me.lblStatus.ForeColor = System.Drawing.Color.LightGray
        Me.lblStatus.Location = New System.Drawing.Point(710, 14)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(178, 44)
        Me.lblStatus.TabIndex = 6
        Me.lblStatus.Text = "Initializing..."
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pnlStats
        '
        Me.pnlStats.BackColor = System.Drawing.Color.FromArgb(10, 10, 25)
        Me.pnlStats.Controls.Add(Me.lblStats)
        Me.pnlStats.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlStats.Location = New System.Drawing.Point(0, 0)
        Me.pnlStats.Name = "pnlStats"
        Me.pnlStats.Size = New System.Drawing.Size(900, 26)
        Me.pnlStats.TabIndex = 7
        '
        'lblStats
        '
        Me.lblStats.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStats.Font = New System.Drawing.Font("Consolas", 8.5!)
        Me.lblStats.ForeColor = System.Drawing.Color.FromArgb(255, 180, 50)
        Me.lblStats.Location = New System.Drawing.Point(8, 0)
        Me.lblStats.Name = "lblStats"
        Me.lblStats.Size = New System.Drawing.Size(884, 26)
        Me.lblStats.TabIndex = 0
        Me.lblStats.Text = "Benchmark: waiting for playback to start…"
        Me.lblStats.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'frmVlcPlayer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(900, 612)
        Me.KeyPreview = True
        Me.MinimumSize = New System.Drawing.Size(640, 400)
        Me.Controls.Add(Me.pnlDisplay)
        Me.Controls.Add(Me.pnlControls)
        Me.Controls.Add(Me.pnlStats)
        Me.Name = "frmVlcPlayer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LibVLC Player"
        Me.pnlControls.ResumeLayout(False)
        Me.pnlControls.PerformLayout()
        Me.pnlStats.ResumeLayout(False)
        CType(Me.trkVolume, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlDisplay As Panel
    Friend WithEvents pnlControls As Panel
    Friend WithEvents btnPlayPause As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents btnRestart As Button
    Friend WithEvents btnFullScreen As Button
    Friend WithEvents trkVolume As TrackBar
    Friend WithEvents lblVolume As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents pnlStats As Panel
    Friend WithEvents lblStats As Label

End Class
