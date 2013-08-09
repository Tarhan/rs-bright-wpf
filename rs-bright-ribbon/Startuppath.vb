Imports System.Runtime.InteropServices
Imports System.Drawing
Module Utils
    Public selecteduri As String
    Function getDownloadPath() As String
        'TODO:
    End Function
    Function getStartupPath() As String
        Dim exePath As String = Environment.GetCommandLineArgs()(0)
        Dim exeFullPath As String = System.IO.Path.GetFullPath(exePath)
        Dim startupPath As String = System.IO.Path.GetDirectoryName(exeFullPath)
        Return startupPath
    End Function
#Region "API"

    Public Structure SHFILEINFO
        Public hIcon As IntPtr 'icon
        Public iIcon As Integer 'icondex
        Public dwAttributes As Integer 'SFGAO_ flags
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)> _
        Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)> _
        Public szTypeName As String
    End Structure

    <DllImport("shell32.dll", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)> _
    Public Function SHGetFileInfo(ByVal pszPath As String, ByVal dwFileAttributes As Integer, _
                    ByRef psfi As SHFILEINFO, ByVal cbFileInfo As Integer, ByVal uFlags As Integer) _
                    As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Const SHGFI_ICON As Integer = &H100 ' アイコン・リソースの取得
    Private Const SHGFI_SMALLICON As Integer = &H1 ' 小さいアイコン (16x16)
    Private Const SHGFI_LARGEICON As Integer = &H0 ' 大きいアイコン (32x32)
    Private Const SHGFI_USEFILEATTRIBUTES As Integer = &H10 ' 拡張子のみでも取得できるようにする為
    Dim IconIndex As Integer = 0

#End Region
    Function GetIconImage(ByVal FilePath As String, Optional ByVal LargeSize As Boolean = True) As Icon
        Dim shinfo As New SHFILEINFO()
        Dim _SHGFI_ICON As Integer

        '大きいアイコン用のアイコンを取得するかどうか指定
        If LargeSize Then
            _SHGFI_ICON = SHGFI_LARGEICON
        Else
            _SHGFI_ICON = SHGFI_SMALLICON
        End If

        ' ファイルに関する情報を取得
        Dim hImg As IntPtr = CType(SHGetFileInfo(FilePath, IconIndex, shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON Or _SHGFI_ICON Or SHGFI_USEFILEATTRIBUTES), IntPtr)
        If hImg.Equals(IntPtr.Zero) = False Then
            Dim Icon As Icon = CType(Icon.FromHandle(shinfo.hIcon).Clone, Icon)
            Return Icon
        Else
            Return Nothing
        End If
    End Function
End Module
