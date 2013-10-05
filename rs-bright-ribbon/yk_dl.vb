Imports libSpirit.html
Module yk_dl
    Function getDownloadParam(url As String) As UriCookiePair
        Dim res As Hashtable = youku.getVideoUrlAndInfo(url)
        Dim uc As New UriCookiePair
        uc.sourceext = res("type")
        uc.VideoInfo = res("title")
        uc.Uris = New SortedDictionary(Of Integer, String) From {{0, res("url")}}
        Return uc
    End Function
End Module
