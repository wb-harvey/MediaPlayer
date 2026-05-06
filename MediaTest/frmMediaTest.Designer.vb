<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMediaTest
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.mnuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFileExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.txtTarget = New System.Windows.Forms.TextBox()
        Me.cmdBrowse = New System.Windows.Forms.Button()
        Me.cmdOpenAbx = New System.Windows.Forms.Button()
        Me.cmdOpenVlc = New System.Windows.Forms.Button()
        Me.txtError = New System.Windows.Forms.TextBox()
        Me.cmdAbxClip = New System.Windows.Forms.Button()
        Me.cmdVlcClip = New System.Windows.Forms.Button()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFile})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(864, 24)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'mnuFile
        '
        Me.mnuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFileExit})
        Me.mnuFile.Name = "mnuFile"
        Me.mnuFile.Size = New System.Drawing.Size(37, 20)
        Me.mnuFile.Text = "File"
        '
        'mnuFileExit
        '
        Me.mnuFileExit.Name = "mnuFileExit"
        Me.mnuFileExit.Size = New System.Drawing.Size(92, 22)
        Me.mnuFileExit.Text = "Exit"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 497)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(864, 22)
        Me.StatusStrip1.TabIndex = 1
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'txtTarget
        '
        Me.txtTarget.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.txtTarget.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem
        Me.txtTarget.Location = New System.Drawing.Point(118, 47)
        Me.txtTarget.Name = "txtTarget"
        Me.txtTarget.Size = New System.Drawing.Size(417, 20)
        Me.txtTarget.TabIndex = 2
        '
        'cmdBrowse
        '
        Me.cmdBrowse.Location = New System.Drawing.Point(37, 44)
        Me.cmdBrowse.Name = "cmdBrowse"
        Me.cmdBrowse.Size = New System.Drawing.Size(75, 23)
        Me.cmdBrowse.TabIndex = 3
        Me.cmdBrowse.Text = "Browse"
        Me.cmdBrowse.UseVisualStyleBackColor = True
        '
        'cmdOpenAbx
        '
        Me.cmdOpenAbx.Location = New System.Drawing.Point(74, 154)
        Me.cmdOpenAbx.Name = "cmdOpenAbx"
        Me.cmdOpenAbx.Size = New System.Drawing.Size(121, 66)
        Me.cmdOpenAbx.TabIndex = 4
        Me.cmdOpenAbx.Text = "Open ABX"
        Me.cmdOpenAbx.UseVisualStyleBackColor = True
        '
        'cmdOpenVlc
        '
        Me.cmdOpenVlc.Location = New System.Drawing.Point(239, 154)
        Me.cmdOpenVlc.Name = "cmdOpenVlc"
        Me.cmdOpenVlc.Size = New System.Drawing.Size(121, 66)
        Me.cmdOpenVlc.TabIndex = 5
        Me.cmdOpenVlc.Text = "Open VLC"
        Me.cmdOpenVlc.UseVisualStyleBackColor = True
        '
        'txtError
        '
        Me.txtError.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.txtError.Location = New System.Drawing.Point(0, 361)
        Me.txtError.Multiline = True
        Me.txtError.Name = "txtError"
        Me.txtError.Size = New System.Drawing.Size(864, 136)
        Me.txtError.TabIndex = 6
        '
        'cmdAbxClip
        '
        Me.cmdAbxClip.Location = New System.Drawing.Point(74, 226)
        Me.cmdAbxClip.Name = "cmdAbxClip"
        Me.cmdAbxClip.Size = New System.Drawing.Size(121, 66)
        Me.cmdAbxClip.TabIndex = 7
        Me.cmdAbxClip.Text = "ABX Clip"
        Me.cmdAbxClip.UseVisualStyleBackColor = True
        '
        'cmdVlcClip
        '
        Me.cmdVlcClip.Location = New System.Drawing.Point(239, 226)
        Me.cmdVlcClip.Name = "cmdVlcClip"
        Me.cmdVlcClip.Size = New System.Drawing.Size(121, 66)
        Me.cmdVlcClip.TabIndex = 8
        Me.cmdVlcClip.Text = "VLC Clip"
        Me.cmdVlcClip.UseVisualStyleBackColor = True
        '
        'frmMediaTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(864, 519)
        Me.Controls.Add(Me.cmdVlcClip)
        Me.Controls.Add(Me.cmdAbxClip)
        Me.Controls.Add(Me.txtError)
        Me.Controls.Add(Me.cmdOpenVlc)
        Me.Controls.Add(Me.cmdOpenAbx)
        Me.Controls.Add(Me.cmdBrowse)
        Me.Controls.Add(Me.txtTarget)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmMediaTest"
        Me.Text = "Media Test"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents mnuFile As ToolStripMenuItem
    Friend WithEvents mnuFileExit As ToolStripMenuItem
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents txtTarget As TextBox
    Friend WithEvents cmdBrowse As Button
    Friend WithEvents cmdOpenAbx As Button
    Friend WithEvents cmdOpenVlc As Button
    Friend WithEvents txtError As TextBox
    Friend WithEvents cmdAbxClip As Button
    Friend WithEvents cmdVlcClip As Button
End Class
