Imports Newtonsoft.Json
Module youku
    Sub test(url As String, Optional pass As String = "")
        Dim u As String = String.Format("http://v.youku.com/player/getPlayList/VideoIDS/{0}/timezone/+09/version/5/source/video?n=3&ran=3915&password={1}", url, pass)
        Dim w As New Net.WebClient
        Dim j As Linq.JObject = Linq.JObject.Parse(w.DownloadString(u))
        'TODO
    End Sub
End Module
