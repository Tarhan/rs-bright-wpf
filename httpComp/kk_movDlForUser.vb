Imports System.Net
Imports System.Text.RegularExpressions
Public Module kk_movDlForUser
    Public Function GetWebRequest(param As KK_HTTP_REQ) As HttpWebRequest
        Dim reqpacketinfo As KK_HTTP_REQ = param
        If (Not param.KK_HTTP_TARGET_URI_FMTDICTIONARY Is Nothing) AndAlso param.KK_HTTP_TARGET_URI Is Nothing Then
            'method of fmt selector
            reqpacketinfo = fmt_selector(param)
        End If

        Dim req As HttpWebRequest = CType(WebRequest.Create(param.KK_HTTP_TARGET_URI), HttpWebRequest)
        req.Headers = reqpacketinfo.KK_HTTP_HEADER
        Return req
    End Function

    Public Function fmt_selector(ByVal p As KK_HTTP_REQ) As KK_HTTP_REQ
        '続き
    End Function

    Public Structure KK_HTTP_REQ
        Dim KK_HTTP_TARGET_URI As Uri
        Dim KK_HTTP_TARGET_URI_FMTDICTIONARY As SortedDictionary(Of Integer, String)
        Dim KK_HTTP_HEADER As Net.WebHeaderCollection
        Dim KK_HTTP_PRELOADED_FILENAME As String
    End Structure

    Function yt(ByVal targetUri As String) As SortedDictionary(Of Integer, String)
        Dim cc As New CookieContainer
        Dim req As HttpWebRequest = CType(WebRequest.Create(targetUri), HttpWebRequest)
        req.CookieContainer = cc : req.Timeout = 5000
        req.GetResponse().Close()
        req = DirectCast(WebRequest.Create("http://www.youtube.com/get_video_info?video_id=" & Regex.Match(targetUri, "(?<=v=)[\w-]+").Value), HttpWebRequest)
        req.CookieContainer = cc
        Dim _info As String
        Dim res As Net.WebResponse = req.GetResponse()
        Dim sr As New IO.StreamReader(res.GetResponseStream())
        _info = sr.ReadToEnd
        sr.Close()
        res.Close()
        Dim info As New Hashtable
        Dim _tmp As New Dictionary(Of String, String)
        Dim fmtmap As New SortedDictionary(Of Integer, String)

        For Each item As String In _info.Split("&"c)
            info.Add(item.Split("="c)(0), Uri.UnescapeDataString(item.Split("="c)(1)))
        Next
        If CStr(info("status")) = "fail" Then
            Throw New UnauthorizedAccessException
        End If
        For Each item As String In CStr(info("url_encoded_fmt_stream_map")).Split(","c)
            For Each a As String In item.Split("&"c)
                _tmp.Add(a.Split("="c)(0), Uri.UnescapeDataString(a.Split("="c)(1)))
            Next
            fmtmap.Add(CInt(_tmp("itag")), (_tmp("url")) + "&signature=" + _tmp("sig"))
            _tmp.Clear()
        Next

        req = DirectCast(WebRequest.Create("http://www.youtube.com/get_video_info?video_id=" & Regex.Match(targetUri, "(?<=v=)\w+").Value & "&t=" & CStr(info("token"))), HttpWebRequest)
        req.CookieContainer = cc
        req.Timeout = 1500
        req.GetResponse().Close()

        fmtmap(-2) = CStr(info("title"))
        fmtmap(-1) = cc.GetCookieHeader(New Uri("http://www.youtube.com"))
        info.Clear()
        Return fmtmap
    End Function

    Function nc(ByVal targetUri As String) As KK_HTTP_REQ
        Dim nchs As Net.Cookie
        Dim player_raw As String = ""
        Dim u As New Uri(targetUri)
        Dim watchId As String = u.AbsolutePath.Replace("/watch/", "")
        Dim getflv_param As String = n_getflv.getflv(watchId, nchs, player_raw)
        Dim param_sanitized As Dictionary(Of String, String) = n_getflv.getflvParse(getflv_param)
        Dim downloadUri As String = param_sanitized("url")
        Dim ck As String = nchs.ToString
        Dim fn As String = ""
        Select Case True
            Case watchId.StartsWith("sm")
                Dim r As New System.Text.RegularExpressions.Regex("(?<=movieType\: )\'.+?\'")
                fn = "video." & r.Matches(player_raw)(0).Value.Replace("'", "")
        End Select
        param_sanitized = Nothing
        getflv_param = Nothing
        Dim req_head As New WebHeaderCollection
        req_head.Add(Net.HttpRequestHeader.Cookie, ck)
        Return New KK_HTTP_REQ With {.KK_HTTP_HEADER = req_head, .KK_HTTP_PRELOADED_FILENAME = fn, .KK_HTTP_TARGET_URI = New Uri(downloadUri)}
    End Function
End Module