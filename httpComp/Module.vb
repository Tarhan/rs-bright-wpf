Imports System.Net
Imports System.Runtime.InteropServices

Public Module f_ncLogin
    Public Property Enable__ncLogin As Boolean
    Dim loginCookies As New CookieCollection
    Event CookieInserted(ByVal ck As String)
    Sub LoginFromEvent()
        Dim ck As String = ""
        Login(ck)
        RaiseEvent CookieInserted(ck)

    End Sub
    <DllImport("wininet.dll")> _
    Private Function InternetSetCookie(ByVal lpszUrl As String, ByVal lpszCookieName As String, ByVal lpszCookieData As String) As Boolean
    End Function

    Public Function Login(ByRef ck As String, Optional ByVal forcely As Boolean = False) As Boolean
        Dim z As Cookie
        If Enable__ncLogin Or forcely Then
            If String.IsNullOrWhiteSpace(ck) Then
                Dim r As CookieContainer = nc_getAccount()
                If r.Count < 1 Then Return False
                ck = r.GetCookieHeader(New Uri("http://nicovideo.jp"))
            End If
            'user_session=usersesion_XXXXX_XXXX_XXXX の「user_session=」の除去
            ck = ck.Replace("user_session=", "")
            '念のためCOOKIEを削除
            InternetSetCookie("http://nicovideo.jp/", "user_session", "x; expires=Fri, 31-Dec-1999 23:59:59 GMT;")


            Dim timedate As String
            '48時間後のタイム(まあ、これくらいでいいか）
            Dim expires As DateTime = DateTime.Now + New TimeSpan(48, 0, 0)
            'Debug.Print(expires.ToString)
            'Fri, 31-Dec-1999 23:59:59 GMTのように時間フォーマットを指定
            timedate = "expires=" & expires.ToString("r", Globalization.DateTimeFormatInfo.InvariantInfo)

            'ＩＥへCOOKIEセット
            InternetSetCookie("http://nicovideo.jp", "user_session", ck & ";" & timedate)
            ck = "user_session=" & ck & ";" & timedate

            Return True
        Else
            Debug.Assert(loginCookies.Count < 1)
            For Each x In loginCookies
                'z.Tostring が末尾にセミコロンを含まないなら、要修正
                ck += z.ToString + " "
            Next
        End If
    End Function

    Friend Function nc_getAccount() As CookieContainer
        Dim LgOnFrm As New ncLoginForm

        If LgOnFrm.ShowDialog() = DialogResult.No Then
            Return Nothing
        End If
        Return LgOnFrm.callbackReturn
    End Function
End Module