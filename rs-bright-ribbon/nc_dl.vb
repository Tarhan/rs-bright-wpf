﻿Imports libSpirit.html
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module nc_dl
    Function getDownloadParam(url As String, ByRef videoinfo As Dictionary(Of String, String)) As UriCookiePair
        Dim u As New Uri(url)
        Dim nchs As Net.Cookie
        Dim player_raw As String = ""
        Dim getflv_param As String = n_getflv.getflv(u.AbsolutePath.Replace("/watch/", ""), nchs, player_raw)
        videoinfo = n_getflv.getflvParse(getflv_param)
        Dim downloadUri As String = videoinfo("url")
        Dim r As New System.Text.RegularExpressions.Regex("(?<=movieType\: )\'.+?\'")
        Dim ext As String = r.Matches(player_raw)(0).Value.Replace("'", "")
        Dim returnvalue As New UriCookiePair With {.cookie = nchs.ToString, .sourceext = ext}
        returnvalue.Uris = New SortedDictionary(Of Integer, String) From {{0, downloadUri}}

        getflv_param = Nothing
        Return returnvalue
    End Function
    Structure ncEntry
        Sub New(url As String)

        End Sub
        Dim title As String

    End Structure
End Module
