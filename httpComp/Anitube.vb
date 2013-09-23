Imports System.Net
Imports System.Text.RegularExpressions
Public Module Anitube
    Dim cc As Net.CookieContainer
    Const configphp_pattern As String = "(?<=cnf=').+?(?=';)"
    Public Function getPlaylistInfo(ByVal url As String, ByRef Cookie As CookieContainer) As Xml.XmlDocument
        Dim req As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
        cc = Cookie
        req.CookieContainer = cc
        Dim data As New IO.StreamReader(req.GetResponse.GetResponseStream())
        Dim result As Match = Regex.Match(data.ReadToEnd, configphp_pattern)
        If result.Success Then Return Extract_playlist(result.Value)
    End Function
    Function Extract_playlist(cpUrl As String) As Xml.XmlDocument
        Dim req As HttpWebRequest = CType(WebRequest.Create(cpUrl.Replace("config", "playlist")), HttpWebRequest)
        req.CookieContainer = cc
        Dim data As New IO.StreamReader(req.GetResponse.GetResponseStream())
        Dim xml As New Xml.XmlDocument
        xml.LoadXml(data.ReadToEnd)
        Return xml
    End Function
End Module
