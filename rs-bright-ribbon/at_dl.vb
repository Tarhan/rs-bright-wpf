Imports libSpirit.html
Module at_dl
    Dim cc As New Net.CookieContainer
    Dim priority = New String() {"filehd", "html5", "file"}
    Public Function GetSessionAndUri(url As String) As UriCookiePair
        Dim playlist As Xml.XmlDocument = Anitube.getPlaylistInfo(url, cc)
        Dim returnValue As New UriCookiePair
        Dim e As Xml.XmlElement = playlist.DocumentElement
        With returnValue
            .Uris(0) = e.SelectSingleNode("/playlist/trackList/track/" & priority(0)).Value & "?start=0"
            .thumbUrl = e.SelectSingleNode("/playlist/trackList/track/image").Value
            .sourceext = "flv"
        End With
        Return returnValue
    End Function

End Module
