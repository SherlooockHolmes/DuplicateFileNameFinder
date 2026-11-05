Imports System.Runtime.InteropServices
Imports System.Text
Public Class FolderPickerHelper
    <DllImport("shell32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SHBrowseForFolder(ByRef bi As BROWSEINFO) As IntPtr
    End Function

    <DllImport("shell32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SHGetPathFromIDList(ByVal pidl As IntPtr, ByVal pszPath As StringBuilder) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Private Structure BROWSEINFO
        Public hwndOwner As IntPtr
        Public pidlRoot As IntPtr
        <MarshalAs(UnmanagedType.LPTStr)>
        Public pszDisplayName As String
        <MarshalAs(UnmanagedType.LPTStr)>
        Public lpszTitle As String
        Public ulFlags As UInteger
        Public lpfn As IntPtr
        Public lParam As IntPtr
        Public iImage As Integer
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, Msg As UInteger, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Private Const BIF_RETURNONLYFSDIRS As UInteger = &H1
    Private Const BIF_NEWDIALOGSTYLE As UInteger = &H40
    Private Const BFFM_INITIALIZED As UInteger = &H1
    Private Const BFFM_SETSELECTIONW As UInteger = &H467

    Private Delegate Function BrowseCallbackProc(hwnd As IntPtr, msg As UInteger, lParam As IntPtr, lpData As IntPtr) As Integer

    Public Shared Function SelectFolder(owner As IntPtr,
                                        Optional title As String = "Select a folder...",
                                        Optional allowNewFolderButton As Boolean = True,
                                        Optional initialFolder As String = Nothing) As String

        Dim buffer As New StringBuilder(260)

        Dim flags As UInteger = BIF_RETURNONLYFSDIRS
        If allowNewFolderButton Then
            flags = flags Or BIF_NEWDIALOGSTYLE
        End If

        Dim callbackDelegate As BrowseCallbackProc = Nothing
        Dim lParam As IntPtr = IntPtr.Zero

        If Not String.IsNullOrEmpty(initialFolder) Then
            ' Define the callback to set initial folder on dialog initialization
            callbackDelegate = Function(hwnd, msg, lParamMsg, lpData)
                                   If msg = BFFM_INITIALIZED Then
                                       ' Send message to set selected folder
                                       SendMessage(hwnd, BFFM_SETSELECTIONW, CType(1, IntPtr), Marshal.StringToHGlobalUni(initialFolder))
                                   End If
                                   Return 0
                               End Function

            ' Allocate callback pointer
            Dim ptrCallback = Marshal.GetFunctionPointerForDelegate(callbackDelegate)
            lParam = IntPtr.Zero ' Not used here because string is passed in SendMessage
        End If

        Dim bi As New BROWSEINFO With {
            .hwndOwner = owner,
            .pszDisplayName = New String(" "c, 260),
            .lpszTitle = title,
            .ulFlags = flags,
            .lpfn = If(callbackDelegate IsNot Nothing, Marshal.GetFunctionPointerForDelegate(callbackDelegate), IntPtr.Zero),
            .lParam = lParam
        }

        Dim pidl As IntPtr = SHBrowseForFolder(bi)

        If pidl <> IntPtr.Zero Then
            If SHGetPathFromIDList(pidl, buffer) Then
                Return buffer.ToString()
            End If
        End If

        Return Nothing
    End Function
End Class
