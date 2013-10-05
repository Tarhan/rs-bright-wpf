'Based on: http://code.google.com/p/savemedia/source/browse/SaveMedia/Sites/Youku.cs?r=144cdf9258962cbbe04075a25f8b96bb1e9b4424
Imports Newtonsoft.Json

Public Module youku
    Const crypt As Integer = &HA55AA5A5
    Const rndsource As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ/\:._-1234567890"
    Const type As String = "flv"
    Function getVideoUrlAndInfo(url As String, Optional pass As String = "") As Hashtable
        Dim u As String = String.Format("http://v.youku.com/player/getPlayList/VideoIDS/{0}/timezone/+09/version/5/source/video?n=3&ran=3915&password={1}", Text.RegularExpressions.Regex.Match(url, "(?<=http://v.youku.com/v_show/id_).+(?=\.html)").Value, pass)
        Dim w As New Net.WebClient
        Dim j As Linq.JObject = Linq.JObject.Parse(w.DownloadString(u))
        'TODO
        Dim key1 As Int64
        Dim key2 As String = ""
        Dim final As String
        Dim time, K, seed As String

        Dim title As String = ""

        Dim hash = j.SelectToken("data[0].streamfileids").Item(type).ToString
        With j.SelectToken("data[0]")
            title = .Item("title").ToString
            With j.SelectToken("data[0].segs." + type + "[0]")
                K = .Item("k").ToString
            End With
            key1 = (crypt Xor Convert.ToInt32(.Item("key1").ToString, 16))
            key2 = .Item("key2").ToString()
            final = key2 & Convert.ToString(key1, 16)

            seed = .Value(Of String)("seed")
            Dim stlen As Integer = rndsource.Length
            Dim dc As List(Of String) = Shuffle(seed)
            hash = DecryptId(hash, dc)
            time = SessionId()
        End With
        u = " http://f.youku.com/player/getFlvPath/sid/" & time & "/st/" & 0 & "/fileid/" & hash & "?K=" & K & "&hd=1&myp=0&ts=" & j.SelectToken("data[0].segs.flv[0]").Item("seconds").ToString
        Dim r As New Hashtable From {{"url", u}, {"title", title}, {"type", type}}
        Return r
    End Function
    Function Shuffle(ByRef aSeed As [String]) As List(Of String)
        ' Modified Fisher-Yates shuffle
        ' http://en.wikipedia.org/wiki/Fisher-Yates_shuffle

        Dim theShuffle As New List(Of String)()

        For theIndex As Integer = 0 To rndsource.Length - 1
            theShuffle.Add(rndsource.Substring(theIndex, 1))
        Next

        Dim theSeed As Double
        Double.TryParse(aSeed, theSeed)
        For theIndex As Integer = 0 To rndsource.Length - 1
            ' PRNG is a standard linear congruential generator
            ' with a = 211, c = 30031, and m = 2^16
            theSeed = (211 * theSeed + 30031) Mod Math.Pow(2, 16)

            Dim x As Double = theSeed / Math.Pow(2, 16) * (rndsource.Length - theIndex)
            theShuffle.Add(theShuffle(CInt(Math.Truncate(x))))
            theShuffle.RemoveAt(CInt(Math.Truncate(x)))
        Next

        Return theShuffle
    End Function
    Function DecryptId(ByRef aEncryptedId As [String], ByRef aDecryptor As List(Of String)) As [String]
        Dim theId As String = ""
        Dim theChars As String() = aEncryptedId.Split("*"c)
        For Each theChar As String In theChars
            If String.IsNullOrEmpty(theChar) Then
                Continue For
            End If
            Dim theIndex As Integer
            Integer.TryParse(theChar, theIndex)

            If theIndex < aDecryptor.Count Then
                theId += aDecryptor(theIndex)
            End If
        Next

        Return theId
    End Function
    Function SessionId() As [String]
        Dim t As DateTime = DateTime.Now
        Dim theTimeSpan As TimeSpan = (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0))
        Dim theUnixTime As Double = theTimeSpan.TotalSeconds
        Dim theUnixTimeStr As [String] = theUnixTime.ToString().Replace(".", "")
        Dim r As New Random()

        Return [String].Format("{0}1{1:00000}_00", theUnixTimeStr, r.[Next](10000))
    End Function
End Module