Imports System.Net
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports libSpirit.html
Module ptv
    Public Function getFlvUrlSet(pageUrl As String) As UriCookiePair
        Dim cc As New CookieContainer
        Return UriCookiePair.createSingleTarget(pandoratv.getFlvUrl(pageUrl, cc), cc)

    End Function
End Module