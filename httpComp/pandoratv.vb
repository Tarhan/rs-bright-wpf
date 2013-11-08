Option Strict Off
Option Infer On
Imports Newtonsoft.Json
Imports System.Net
Imports System.Text.RegularExpressions

Public Module pandoratv
    Public Function mkKey(firstkey As String) As String
        Dim key1 As String = firstkey
        Dim key2 As String = ""

        Dim TA, RMA, VA As New ArrayList

        Dim RA As New ArrayList
        Dim RAO As New ArrayList

        Dim ranKey As Object

        Dim retsu As Integer = 4

        'packKey1
        Dim key As String = key1
        Dim gyo = key.Length / 2 / retsu

        For i As Byte = 0 To (gyo - 1)
            TA.Insert(i, New ArrayList)
            For j As Byte = 0 To (retsu - 1)
                TA(i).Insert(j, "0x" + key.Substring(0, 2))
                key = key.Substring(2)
            Next
        Next

        Randomize()

        'randomKey
        Dim Rd = CInt(Rnd() * 255)
        If (Rd Mod 2) = 0 Then Rd += 1
        Dim Rh As String = Hex(Rd)
        Rh = keta(Rh, 2)
        RA.Insert(0, "0x" + Rh)
        RAO.Insert(0, Rh)

        'timeKey
        Dim duration = mkDuration(0, 7)

        Dim Th = Convert.ToString(duration, 16)
        Th = keta(Th, 4)
        Dim Thup = Th.Substring(0, 2)
        Dim Thdn = Th.Substring(2)
        Dim ThupEnc As Object = Convert.ToInt32(Thup, 16) Xor &HFA
        ThupEnc = keta(Hex(ThupEnc), 2)
        Dim ThdnEnc As Object = Convert.ToInt32(Thdn, 16) Xor &HCE
        ThdnEnc = keta(Hex(ThdnEnc), 2)

        Dim ThEnc = ThupEnc & ThdnEnc
        For i As Byte = 0 To 3
            RMA.Insert(i, New ArrayList)
            RMA(i).insert(0, ThEnc.Chars(i))
            RMA(i).insert(1, keta(Convert.ToString(4 - 1 - i, 2), 2))
        Next

        'mixSort
        Dim length As Int32 = RMA.Count
        For i As Byte = 0 To (length - 1)
            Randomize()
            Dim rndNum = Math.Truncate(Rnd() * length)
            Dim taihi = CType(RMA(i), ArrayList).Clone
            RMA(i) = CType(RMA(rndNum), ArrayList).Clone
            Dim dst As ArrayList = CType(RMA(rndNum), ArrayList)
            dst.Clear()
            dst.AddRange(taihi)
        Next
        Dim order As Object
        For i As Byte = 0 To (length - 1)
            order += RMA(i)(1)
        Next
        order = Convert.ToInt32(order, 2)
        ranKey = Hex(order Xor &HA7)
        RA.Insert(1, "0x" & ranKey)
        RAO.Insert(1, ranKey)
        RAO.Insert(2, RMA(0)(0) & RMA(1)(0))
        RAO.Insert(3, RMA(2)(0) & RMA(3)(0))
        RA.Insert(2, "0x" & RAO(2))
        RA.Insert(3, "0x" & RAO(3))

        'setVertical
        For j As Byte = 0 To (retsu - 1)
            Dim preV As Int32 = Convert.ToInt32(TA(0)(j), 16) Xor Convert.ToInt32(TA(1)(j), 16)
            For i As Byte = 2 To (TA.Count - 1)
                preV = preV Xor Convert.ToInt32(TA(i)(j), 16)
            Next
            Dim V As Int32 = preV Xor Convert.ToInt32("0x" + RAO(j), 16)
            VA.Add(keta(Hex(V), 2))
        Next

        'setHorizontal
        For j As Byte = 0 To (TA.Count - 1)
            Dim preH As Int32 = Convert.ToInt32(TA(j)(0), 16) Xor Convert.ToInt32(TA(j)(1), 16)
            For i As Byte = 2 To (retsu - 1)
                preH = preH Xor Convert.ToInt32(TA(j)(i), 16)
            Next
            Dim H As Int32 = preH Xor Convert.ToInt32("0x" + RAO(j Mod 4), 16)
            VA.Add(keta(Hex(H), 2))
        Next

        'makeKey
        Dim keyheader As String = ""
        For i = 1 To retsu - 1
            keyheader += RAO(i)
        Next
        key2 = keyheader &
            VA(10) & VA(0) &
            VA(9) & VA(1) &
            VA(8) & VA(2) &
            VA(2) & VA(7) &
            VA(3) & VA(6) &
            VA(5) & VA(4)
        key2 = key2.ToUpper.Replace("0"c, "")
        Return key2

    End Function

    Private Function mkDuration(min As Integer, max As Integer) As Int32
        Randomize()
        Dim r As Int32 = Math.Truncate(Rnd() * (max - min)) + min
        Return r
    End Function

    Private Function keta(str As String, v_keta As Integer) As String
        Dim r As String = str
        For i As Int16 = r.Length To v_keta
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
        Dim flv_url, key1, key2 As String
        flv_url = first("vodUrl").ToString.Replace("http://", "")
        req = WebRequest.Create("http://channel.pandora.tv/channel/cryptKey.ptv?dummy=" & CStr(Rnd()))
        req.CookieContainer = cc
        Using s As New IO.StreamReader(req.GetResponse.GetResponseStream)
            str = s.ReadToEnd
        End Using
        Dim second As Linq.JObject = Linq.JObject.Parse(str)
        key1 = second("encrypt_key")
        key2 = pandoratv.mkKey(key1)

        Return String.Format("http://trans-idx.pandora.tv/{0}?key1={1}&key2={2}" + "&ft=FC&class=normal&country=JP&method=differ", flv_url, key1, key2)
    End Function

End Module