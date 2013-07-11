Imports System.Net

Public Class ncLoginForm

    ' TODO: 指定されたユーザー名およびパスワードを使用して、カスタム認証を実行するコードを挿入します 
    ' ( http://go.microsoft.com/fwlink/?LinkId=35339 を参照してください)。  
    ' カスタム プリンシパルは、以下のように現在のスレッドのプリンシパルにアタッチできます: 
    '     My.User.CurrentPrincipal = CustomPrincipal
    ' この場合 CustomPrincipal は、認証を実行するのに使われる IPrincipal 実装です。 
    ' これにより My.User は、ユーザー名および表示名などの CustomPrincipal オブジェクトに要約された ID 情報を
    ' 返します。

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        If ncLogin() Then Me.Close()
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
    End Sub

    Public Property callbackReturn As New Net.CookieContainer

    Private Function ncLogin() As Boolean
        Dim user, pass As String
        user = UsernameTextBox.Text
        pass = PasswordTextBox.Text
        Dim req As HttpWebRequest = CType(WebRequest.Create("https://secure.nicovideo.jp/secure/login?site=niconico"), HttpWebRequest) : req.CookieContainer = New CookieContainer
        Dim res As HttpWebResponse = Nothing
        req.AllowAutoRedirect = True
        req.Method = "POST"
        req.ContentType = "application/x-www-form-urlencoded"
        req.Timeout = 5000
        req.ReadWriteTimeout = 5000
        req.Referer = "https://secure.nicovideo.jp/secure/login_form"
        Using st As New IO.StreamWriter(req.GetRequestStream())
            st.Write("mail=" & user & "&password=" & pass)
            st.Flush()
        End Using
        res = CType(req.GetResponse(), HttpWebResponse)
        Dim authflag As String = res.GetResponseHeader("x-niconico-authflag")
        Dim setcookie As String = res.GetResponseHeader("Set-Cookie")
        Dim responsedData As String
        Using st As New IO.StreamReader(res.GetResponseStream)
            responsedData = st.ReadToEnd
        End Using
        If authflag = 1 Or 3 Then
            Dim x As UInt16

            For Each c As Cookie In req.CookieContainer.GetCookies(New Uri("http://nicovideo.jp/"))
                callbackReturn.Add(c)
                x += 1
                MsgBox(c.ToString)
            Next
            Stop
        Else
            MsgBox("ログイン失敗")
            Throw New NotSupportedException("ログインに失敗しましたorz")
            Return False
        End If
        Dim cf As New n_usrprof With {.user = user, .pass = pass}
        ncusrmgmt.conf = cf
        Return True
    End Function
End Class
