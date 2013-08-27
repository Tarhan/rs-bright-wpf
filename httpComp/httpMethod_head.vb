Imports System.Net

Module httpMethod_head
    Public Function HEAD(uri As String) As HttpWebResponse
        Dim u As New Uri(uri)
        Dim req As HttpWebRequest = DirectCast(WebRequest.CreateDefault(u), HttpWebRequest)
        req.Method = "head"
        req.AllowAutoRedirect = True
        Dim res As HttpWebResponse = DirectCast(req.GetResponse, HttpWebResponse)
        Return res
    End Function
End Module
