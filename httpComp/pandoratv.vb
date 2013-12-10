Option Strict Off
Imports Newtonsoft.Json
Imports System.Net
Imports System.Text.RegularExpressions

Public Module pandoratv
    Public Function mkKey(firstkey As String) As String
        Dim key1 As String = firstkey
        Dim key2 As String = ""

        Dim RMA, TA As New List(Of List(Of String))

        Dim RA, RAO, VA As New List(Of String)

        Dim ranKey As Integer

        Dim retsu As Integer = 4

        'packKey1
        Dim key As String = key1
        Dim gyo As Integer = CInt(key.Length / 2 / retsu)

        For i As Integer = 0 To (gyo - 1)
            TA.Insert(i, New List(Of String))
            For j As Integer = 0 To (retsu - 1)
                TA(i).Insert(j, "0x" + key.Substring(0, 2))
                key = key.Substring(2)
            Next
        Next

        Randomize()

        'randomKey
        Dim Rd As Integer = CInt(Rnd() * 255)
        If (Rd Mod 2) = 0 Then Rd += 1
        Dim Rh As String = Convert.ToString(Rd, 16)
        Rh = keta(Rh, 2)
        RA.Insert(0, "0x" + Rh)
        RAO.Insert(0, Rh)

        'timeKey
        Dim duration As Integer = mkDuration(0, 7)

        Dim Th As String = Convert.ToString(duration, 16)
        Th = keta(Th, 4)
        Dim Thup As String = Th.Substring(0, 2)
        Dim Thdn As String = Th.Substring(2)
        Dim ThupEnc As Integer = Convert.ToInt32(Thup, 16) Xor &HFA
        Dim ThdnEnc As Integer = (Convert.ToInt32(Thdn, 16) Xor &HCE)
        Dim ThEnc As String = keta(Hex(ThupEnc), 2) & keta(Hex(ThdnEnc), 2)
        For i As Byte = 0 To 3
            RMA.Insert(i, New List(Of String))
            RMA(i).insert(0, ThEnc.Chars(i))
            RMA(i).insert(1, keta(Convert.ToString(4 - 1 - i, 2), 2))
        Next

        'mixSort
        Dim length As Int32 = RMA.Count
        For i As Int32 = 0 To (length - 1)
            Randomize()
            Dim rndNum As Integer = CInt(Math.Truncate(Rnd() * length))
            Dim taihi As List(Of String) = RMA(i).ToArray.ToList
            RMA(i) = RMA(rndNum)
            RMA(rndNum) = taihi
        Next
        Dim order As String = ""
        For i As Integer = 0 To (length - 1)
            order += RMA(i)(1)
        Next

        ranKey = (Convert.ToInt32(order, 2) Xor &HA7)
        Dim ranHex As String = Convert.ToString(ranKey, 16)
        RA.Insert(1, "0x" & ranHex)
        RAO.Insert(1, ranHex)
        RAO.Insert(2, RMA(0)(0) & RMA(1)(0))
        RAO.Insert(3, RMA(2)(0) & RMA(3)(0))
        RA.Insert(2, "0x" & RAO(2))
        RA.Insert(3, "0x" & RAO(3))

        'setVertical
        For j As Integer = 0 To (retsu - 1)
            Dim preV As Int32 = Convert.ToInt32(TA(0)(j), 16) Xor Convert.ToInt32(TA(1)(j), 16)
            For i As Integer = 2 To (TA.Count - 1)
                preV = preV Xor Convert.ToInt32(TA(i)(j), 16)
            Next
            Dim V As Int32 = preV Xor Convert.ToInt32("0x" + RAO(j), 16)
            VA.Add(keta(Hex(V), 2))
        Next

        'setHorizontal
        For j As Integer = 0 To (TA.Count - 1)
            Dim preH As Int32 = Convert.ToInt32(TA(j)(0), 16) Xor Convert.ToInt32(TA(j)(1), 16)
            For i As Integer = 2 To (retsu - 1)
                preH = preH Xor Convert.ToInt32(TA(j)(i), 16)
            Next
            Dim H As Int32 = preH Xor Convert.ToInt32("0x" + RAO(j Mod 4), 16)
            VA.Add(keta(Hex(H), 2))
        Next

        'makeKey
        Dim keyheader As String = ""
        For i As Integer = 0 To retsu - 1
            keyheader += RAO(i)
        Next
        key2 = keyheader &
            VA(10) & VA(0) &
            VA(9) & VA(1) &
            VA(8) & VA(2) &
            VA(2) & VA(7) &
            VA(3) & VA(6) &
            VA(5) & VA(4)
        key2 = key2.Replace("0"c, "")
        Return key2

    End Function

    Private Function mkDuration(min As Integer, max As Integer) As Int32
        Randomize()
        Dim r As Int32 = CInt(Math.Truncate(Rnd() * (max - min)) + min)
        Return r
    End Function

    Private Function keta(str As String, v_keta As Integer) As String
        Dim r As String = str
        For i As Integer = r.Length To v_keta
            r = "0" & r
        Next
        Return r
    End Function

    Public Function getFlvUrl(pageUrl As String, ByRef cc As CookieContainer) As String
        If cc Is Nothing Then cc = New CookieContainer
        Dim base As New Uri(pageUrl)
        Dim QueryString As String = "?prgid=" & Regex.Match(base.Query, "(?<=prgid=)\w+").Value & "&ch_userid=" & Regex.Match(base.Query, "(?<=ch_userid=)\w+").Value & "&HIT=off&player=channel&count=on&dummy=" & CStr(Math.Floor(Rnd() * 100000))
        Dim req As HttpWebRequest = WebRequest.CreateDefault(base)
        req.CookieContainer = cc
        req.GetResponse.Close()
        req = WebRequest.Create("http://channel.pandora.tv/video/php/php.player.query.php" + QueryString)
        req.CookieContainer = cc
        Dim str As String
        Using s As New IO.StreamReader(req.GetResponse.GetResponseStream)
            str = s.ReadToEnd
        End Using
        Dim first As Linq.JObject = Linq.JObject.Parse(str)
        Dim hug1 As String
        If first("countryCodeIP") = "KR" Then
            hug1 = "http://imgcdnprt.pandora.tv:6359/nocache/serviceinfo_neo.xml"
        Else
            hug1 = "http://imgcdn.pandora.tv/nocache/serviceinfo_neo.xml"
        End If
        Dim x As New Xml.XmlDocument()
        req = WebRequest.Create(hug1)
        str = [String].Empty
        Dim a As New System.Net.Configuration.HttpWebRequestElement
        a.UseUnsafeHeaderParsing = True
        Using s As New IO.StreamReader(req.GetResponse.GetResponseStream)
            str = s.ReadToEnd
        End Using
        x.LoadXml(str)
        Dim hug4 As String = x.SelectSingleNode("/vod/lsuppip").FirstChild.Value
        Dim defaultmultibit, multibitType As String
        If first("multibit").ToString.ToUpper().IndexOf("H") > (-1) Then
            defaultmultibit = "hd"
        ElseIf first("multibit").ToString.ToUpper().IndexOf("S") > (-1) Then
            defaultmultibit = "sd"
        ElseIf first("multibit").ToString.ToUpper().IndexOf("L") > (-1) Then
            defaultmultibit = "flv"
        ElseIf first("multibit").ToString.ToUpper().IndexOf("V") > (-1) Then
            defaultmultibit = "vld"
        ElseIf first("multibit").ToString = "" Then
            defaultmultibit = "vld"
        Else
            Throw New NotImplementedException
        End If

        multibitType = "vld" ' todo

        Dim flv_url, key1, key2 As String
        flv_url = first("vodUrl").ToString.Replace("http://", "")

        Dim hug6 As String = flv_url.Split(".pandora.tv")(0)
        Dim hug7 As String
        If hug6 = "flvcdn" Then
            If defaultmultibit = multibitType Then
                hug7 = ("http://" & hug4 & "/flvorgx.pandora.tv/")
            Else
                hug7 = ("http://" & hug4 & "/flvchmt.pandora.tv/")
            End If
        ElseIf hug6 = "flvcdn2" Then
            If defaultmultibit = multibitType Then
                hug7 = ("http://" & hug4 & "/flvorgx2.pandora.tv/")
            Else
                hug7 = ("http://" & hug4 & "/flvchmt2.pandora.tv/")
            End If
        End If

        hug7 += multibitType
        flv_url = "/" & flv_url.Split("_user")(1)

        req = WebRequest.Create("http://channel.pandora.tv/channel/cryptKey.ptv?dummy=" & CStr(Rnd()))
        req.CookieContainer = cc
        Using s As New IO.StreamReader(req.GetResponse.GetResponseStream)
            str = s.ReadToEnd
        End Using
        Dim second As Linq.JObject = Linq.JObject.Parse(str)
        key1 = second("encrypt_key")
        key2 = pandoratv.mkKey(key1)

        Return String.Format("{3}{0}@key1={1}&key2={2}" + "&ft=FC&class=normal&country=JP&method=differ", flv_url, key1, key2, hug7)
    End Function

End Module