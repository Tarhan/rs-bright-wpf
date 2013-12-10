Imports System.Text.RegularExpressions
Imports System.Xml
Module veoh
    Public Function RetrieveVideoUrl(pageUrl As String) As String
        Dim u As String = pageUrl.Replace("/watch/", "/api/findByPermalink?permalink=")
        Dim x As New XmlDocument
        x.Load(My.Computer.FileSystem.ReadAllText(u))

        '注：ipodurl
        Return x.SelectSingleNode("/rsp/videoList/video").Attributes("ipodUrl").Value
    End Function
End Module
