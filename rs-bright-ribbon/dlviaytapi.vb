Imports Google.YouTube
Imports Google.GData
Imports Google.GData.YouTube
Imports Google.GData.Extensions
Imports Google.GData.Extensions.MediaRss
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

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
        Dim thumbUrl As String
        Public Function getFmtIdWhichContains() As List(Of Integer)
            Dim l As New List(Of Integer)
            For Each key As Integer In Me.Uris.Keys
                If key > 0 Then l.Add(key)
            Next
            Return l
        End Function
        Public Shared Function getExtention(Fmt As Integer) As String
            Return CType(getResolution().Item(CStr(Fmt)), JObject).Item("format")
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
    Dim yt_resolution As String = "{~	5:  { itag: 5, quality: 5, description: ""Low Quality, 240p"", format: ""FLV"", fres: ""240p"", 	mres: { width:  400, height:  240 }, acodec: ""MP3"", vcodec: ""SVQ""},~	17: { itag: 17, quality: 4, description: ""Low Quality, 144p"", format: ""3GP"", fres: ""144p"", mres: { width:  0, height: 0  }, acodec: ""AAC"", vcodec: """"},~	18: { itag: 18, quality: 15, description: ""Low Definition, 360p"", format: ""MP4"", fres: ""360p"", mres: { width:  480, height:  360 }, acodec: ""AAC"", vcodec: ""H.264""},~	22: { itag: 22, quality: 35, description: ""High Definition, 720p"", format: ""MP4"", fres: ""720p"",	mres: { width: 1280, height:  720 }, acodec: ""AAC"", vcodec: ""H.264""},~	34: { itag: 34, quality: 10, description: ""Low Definition, 360p"", format: ""FLV"", fres: ""360p"", 	mres: { width:  640, height:  360 }, acodec: ""AAC"", vcodec: ""H.264""},~	35: { itag: 35, quality: 25, description: ""Standard Definition, 480p"", format: ""FLV"" , fres: ""480p"", mres: { width:  854, height:  480 }, acodec: ""AAC"", vcodec: ""H.264""},~	36: { itag: 36, quality: 6, description: ""Low Quality, 240p"", format: ""3GP"", fres: ""240p"", 	mres: { width:  0, height:  0 }, acodec: ""AAC"", vcodec: """"},~	37: { itag: 37, quality: 45, description: ""Full High Definition, 1080p"", format: ""MP4"", fres: ""1080p"", mres: {width: 1920, height: 1080}, acodec: ""AAC"", vcodec: ""H.264""},~	38: { itag: 38, quality: 55, description: ""Original Definition"", format: ""MP4"" , fres: ""Orig"",	mres: { width: 4096, height: 3072 }, acodec: ""AAC"", vcodec: ""H.264""},~	43: { itag: 43, quality: 20, description: ""Low Definition, 360p"", format: ""WebM"", fres: ""360p"",	mres: { width:  640, height:  360 }, acodec: ""Vorbis"", vcodec: ""VP8""},~	44: { itag: 44, quality: 30, description: ""Standard Definition, 480p"", format: ""WebM"", fres: ""480p"", mres: { width:  854, height:  480 }, acodec: ""Vorbis"", vcodec: ""VP8""},~	45: { itag: 45, quality: 40, description: ""High Definition, 720p"", format: ""WebM"", fres: ""720p"", mres: { width: 1280, height:  720 }, acodec: ""Vorbis"", vcodec: ""VP8""},~	46: { itag: 46, quality: 50, description: ""Full High Definition, 1080p"", format: ""WebM"", fres: ""1080p"",	mres: {width: 1280, height: 720}, acodec: ""Vorbis"", vcodec: ""VP8""},~	82: { itag: 82, quality: 16, description: ""Low Definition 3D, 360p"", format: ""MP4"",  fres: ""360p"", mres: { width: 640,  height:  360 }, acodec: ""AAC"", vcodec: ""H.264""},~	84: { itag: 84, quality: 41, description: ""High Definition 3D, 720p"", format: ""MP4"",  fres: ""720p"",	mres: { width: 1280, height:  720 }, acodec: ""AAC"", vcodec: ""H.264""},~	100: { itag: 100, quality: 17, description: ""Low Definition 3D, 360p"", format: ""WebM"", fres: ""360p"", mres: { width: 640,  height:  360 }, acodec: ""Vorbis"", vcodec: ""VP8""},~	102: { itag: 102, quality: 42, description: ""High Definition 3D, 720p"", format: ""WebM"", fres: ""720p"", mres: {width: 1280, height: 720}, acodec: ""Vorbis"", vcodec: ""VP8""},~	}".Replace("~"c, vbNewLine)
    Function getResolution() As Hashtable
        Dim ht As Hashtable = JsonConvert.DeserializeObject(Of Hashtable)(yt_resolution)
        Return ht
    End Function
End Module
