Imports libSpirit.html
Imports System.Xml

Module nc_dl
    Function getDownloadParam(url As String, ByRef videoinfo As Dictionary(Of String, String)) As UriCookiePair
        Dim u As New Uri(url)
        Dim nchs As Net.Cookie
        Dim player_raw As String = ""
        Dim watchId As String = u.AbsolutePath.Replace("/watch/", "")
        Dim getflv_param As String = n_getflv.getflv(watchId, nchs, player_raw)
        videoinfo = n_getflv.getflvParse(getflv_param)
        Dim downloadUri As String = videoinfo("url")
        Dim r As New System.Text.RegularExpressions.Regex("(?<=movieType\: )\'.+?\'")
        Dim ext As String = r.Matches(player_raw)(0).Value.Replace("'", "")
        Dim returnvalue As New UriCookiePair With {.cookie = nchs.ToString, .sourceext = ext}
        returnvalue.Uris = New SortedDictionary(Of Integer, String) From {{0, downloadUri}}
        Dim x As XmlDocument = n_getflv.getthumbinfo(watchId)
        returnvalue.thumbUrl = x.DocumentElement.SelectSingleNode("/nicovideo_thumb_response/thumb/thumbnail_url").FirstChild.Value
        getflv_param = Nothing
        Return returnvalue
    End Function
End Module
