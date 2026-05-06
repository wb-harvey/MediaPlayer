Imports LibVLCSharp.Shared
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks

''' <summary>
''' Holds a shared, lazily pre-initialized LibVLC instance.
''' Core.Initialize() and the LibVLC engine construction are moved here
''' so they run once, on a background thread, the moment the app starts —
''' not when the user clicks "Open VLC".
''' </summary>
Public Module VlcEngine

    Private _libVlc As LibVLC = Nothing
    Private _initTask As Task = Nothing
    Private _initLock As New Object()
    Private _initError As String = Nothing

    ''' <summary>
    ''' Returns the error message from the last failed init attempt, or Nothing if succeeded.
    ''' </summary>
    Public ReadOnly Property LastError As String
        Get
            Return _initError
        End Get
    End Property

    ''' <summary>
    ''' Call this from frmMediaTest.Load to kick off background init.
    ''' </summary>
    Public Sub PreInitialize()
        SyncLock _initLock
            If _initTask Is Nothing Then
                _initTask = Task.Run(Sub() DoInit())
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Registers a callback that fires (on a background thread) once init is done.
    ''' callback(success As Boolean, diagnosticText As String)
    ''' </summary>
    Public Sub WhenReady(callback As Action(Of Boolean, String))
        SyncLock _initLock
            If _initTask Is Nothing Then
                _initTask = Task.Run(Sub() DoInit())
            End If
        End SyncLock

        _initTask.ContinueWith(Sub(t)
            Dim ok = (_libVlc IsNot Nothing)
            Dim diag As String

            If ok Then
                diag = "LibVLC: Ready ✓" & vbCrLf &
                       "BaseDir: " & AppDomain.CurrentDomain.BaseDirectory
            Else
                diag = "LibVLC: FAILED" & vbCrLf &
                       If(_initError, "(no error detail captured)") & vbCrLf &
                       "IntPtr.Size: " & IntPtr.Size.ToString() & " bytes (" &
                           If(IntPtr.Size = 8, "64-bit", "32-bit") & " process)"
            End If

            callback(ok, diag)
        End Sub)
    End Sub

    ''' <summary>
    ''' Returns the shared LibVLC instance.
    ''' Blocks (on the calling thread) until init is complete if PreInitialize
    ''' was called early enough this will already be done.
    ''' </summary>
    Public Function GetLibVlc() As LibVLC
        SyncLock _initLock
            ' If a previous attempt completed but left _libVlc as Nothing (failed),
            ' reset so we can try again (Core.Initialize is idempotent after the
            ' first successful native load).
            If _initTask IsNot Nothing AndAlso _initTask.IsCompleted AndAlso _libVlc Is Nothing Then
                _initError = Nothing
                _initTask = Task.Run(Sub() DoInit())
            ElseIf _initTask Is Nothing Then
                _initTask = Task.Run(Sub() DoInit())
            End If
        End SyncLock

        _initTask.Wait()
        Return _libVlc
    End Function

    ''' <summary>
    ''' Returns True if the background init has already completed.
    ''' </summary>
    Public ReadOnly Property IsReady As Boolean
        Get
            Return _initTask IsNot Nothing AndAlso _initTask.IsCompleted AndAlso _libVlc IsNot Nothing
        End Get
    End Property

    Private Sub DoInit()
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim libVlcDir As String = Nothing

        Try
            ' A 32-bit process MUST load 32-bit DLLs (win-x86).
            ' A 64-bit process MUST load 64-bit DLLs (win-x64).
            ' IntPtr.Size is the definitive indicator of process bitness —
            ' do NOT probe x64 first and hope; a wrong-arch DLL will always fail
            ' with 0x8007007E even though the file exists on disk.
            Dim arch As String = If(IntPtr.Size = 8, "win-x64", "win-x86")
            Dim candidate As String = IO.Path.Combine(baseDir, "libvlc", arch)

            If IO.File.Exists(IO.Path.Combine(candidate, "libvlc.dll")) Then
                libVlcDir = candidate
                Core.Initialize(libVlcDir)
            Else
                ' DLL not in the expected subfolder — let LibVLCSharp auto-detect
                ' (handles cases where libvlc.dll was copied to the root folder)
                Core.Initialize()
            End If

            _libVlc = New LibVLC()

        Catch ex As Exception
            _initError = String.Format(
                "{0}{1}BaseDir: {2}{1}libvlc folder: {3}{1}IntPtr.Size: {4} bytes ({5} process)",
                ex.Message, vbCrLf,
                baseDir,
                If(libVlcDir, "(not found)"),
                IntPtr.Size,
                If(IntPtr.Size = 8, "64-bit", "32-bit"))
            _libVlc = Nothing
        End Try
    End Sub

    ' ── Accurate seeking (P/Invoke) ──────────────────────────────────────────
    '  LibVLCSharp 3.x wraps libvlc_media_player_set_time() with b_fast=True,
    '  which snaps to the nearest keyframe. We call the native function directly
    '  with b_fast=False to decode silently from the keyframe to the exact ms.
    '  Core.Initialize() must have been called first so LoadLibrary can find
    '  libvlc.dll via the directory it added to the DLL search path.

    <DllImport("libvlc", CallingConvention:=CallingConvention.Cdecl)>
    Private Sub libvlc_media_player_set_time(
            mp As IntPtr,
            i_time As Long,
            <MarshalAs(UnmanagedType.I1)> b_fast As Boolean)
    End Sub

    ''' <summary>
    ''' Seeks to an exact millisecond position without keyframe snapping.
    ''' LibVLC will decode silently from the preceding keyframe up to the
    ''' target timestamp before presenting the first frame — this costs
    ''' extra time proportional to the keyframe interval, but is frame-accurate.
    '''
    ''' IMPORTANT: Do NOT call from a LibVLC event callback. Use Task.Run:
    '''   Task.Run(Sub() VlcEngine.AccurateSeek(player, ms))
    ''' </summary>
    Public Sub AccurateSeek(mediaPlayer As LibVLCSharp.Shared.MediaPlayer, positionMs As Long)
        libvlc_media_player_set_time(mediaPlayer.NativeReference, positionMs, False)
    End Sub

    ''' <summary>
    ''' Call this on application exit to release the native library cleanly.
    ''' </summary>
    Public Sub Shutdown()
        SyncLock _initLock
            If _libVlc IsNot Nothing Then
                _libVlc.Dispose()
                _libVlc = Nothing
            End If
        End SyncLock
    End Sub

End Module
