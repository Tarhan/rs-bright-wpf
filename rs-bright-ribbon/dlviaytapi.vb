Imports Google.YouTube
Imports Google.GData
Imports Google.GData.YouTube
Imports Google.GData.Extensions
Imports Google.GData.Extensions.MediaRss

Module downloadViaGDataapi
    Private Function getRequest() As YouTubeRequest
        Dim reqset As New YouTubeRequestSettings("kazuki kuroda", "AI39si4os6YfGiIiw_ZIK4KJTqOFj8fkbiWSz36qct-kDHwkJMilJMRA4bEdcQ72Udwa5kS1YC8qqZhvBdGxHqkbR8UfJLWb0Q")
        Dim request As New YouTubeRequest(reqset)
        Return request
    End Function
    Public Function getPlaylistVideoIdList(listID As String) As List(Of String)
        Dim l As New List(Of String)
        For Each i As Client.AtomEntry In ExtractPlaylistFeed(listID)
            l.Add(i.AlternateUri.Content)
        Next
        Return l
    End Function
    Private Function ExtractPlaylistFeed(listid As String) As Client.AtomEntryCollection
        Return getPlaylistFeed(listid).Entries
    End Function
    Public Function getPlaylistFeed(listId As String) As YouTubeFeed
        Dim playlisturl As New Uri("http://gdata.youtube.com/feeds/api/playlists/" & listId & "?v=2")
        Dim q As New YouTubeQuery
        q.Uri = playlisturl
        Return getRequest.Service.Query(q)
    End Function

    Public Function getVideoIdFromUrl(url As String) As VideoId
        Return New VideoId(System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value)
    End Function
    Public Function getStringFromVideoId(vid As VideoId) As String
        Return (System.Text.RegularExpressions.Regex.Match(vid.Value, "(?<=v=)[\w-]+").Value)
    End Function
    Public Function getVideoEntry(vid_Id As VideoId) As Video
        Dim videoEntryUrl As Uri = New Uri("http://gdata.youtube.com/feeds/api/videos/" & vid_Id.Value)
        On Error Resume Next
        Return getRequest.Retrieve(Of Video)(videoEntryUrl)
    End Function


    'Downloader

    Structure UriCookiePair
        Dim Uris As SortedDictionary(Of Integer, String)
        Dim cookie As String
        Dim VideoInfo As Object
        Dim sourceext As String
        Public Function getFmtIdWhichContains() As List(Of Integer)
            Dim l As New List(Of Integer)
            For Each key As Integer In Me.Uris.Keys
                If key > 0 Then l.Add(key)
            Next
            Return l
        End Function
    End Structure
    Public Function getDownloadParam(Url As String) As UriCookiePair
        Dim dic As SortedDictionary(Of Integer, String) = libSpirit.html.yt(Url)
        Dim cookie As String = dic(-1)
        dic.Remove(-1)
        dic.Remove(-2)
        Dim entry As Video = getVideoEntry(getVideoIdFromUrl(Url))
        Return New UriCookiePair With {.cookie = cookie, .Uris = dic, .VideoInfo = entry}
    End Function
    Public Function getDownloadParam(vid_Id As VideoId) As UriCookiePair
        Return getDownloadParam("http://youtube.com/watch?v=" + getStringFromVideoId(vid_Id))
    End Function
    Public Function getPlaylistDownloadParam(listId As String) As List(Of UriCookiePair)
        Dim l As New List(Of UriCookiePair)
        For Each Id As String In getPlaylistVideoIdList(listId)
            l.Add(getDownloadParam(New VideoId(Id)))
            System.Threading.Thread.Sleep(300)
            Debug.WriteLine("Processed")
        Next
        Return l
    End Function
End Module

Module ytfmt

End Module
