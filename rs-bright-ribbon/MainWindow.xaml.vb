Imports Microsoft.Windows.Controls.Ribbon
Imports System.Text.RegularExpressions

Class MainWindow
    Inherits RibbonWindow
    Enum vServiceKind
        Niconico
        Niconama
        Youtube
    End Enum

#Region "イベントハンドラ"
    Private Sub dlbutton_Click(sender As Object, e As RoutedEventArgs) Handles dlbutton.Click
        Select Case DirectCast(DirectCast(e.Source, RibbonButton).Tag, vServiceKind)
            Case vServiceKind.Youtube
                ytdl(Urlbox.Text)
            Case vServiceKind.Niconico
                ncdl(Urlbox.Text)
        End Select
    End Sub
    Private Sub RibbonTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Regex.IsMatch(Urlbox.Text, "http://(www\.)?youtube\.com/watch\?.*") Then
            dlbutton.Tag = vServiceKind.Youtube
        ElseIf Regex.IsMatch(Urlbox.Text, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+") Then
            dlbutton.Tag = vServiceKind.Niconico
        Else
            dlbutton.Tag = Nothing
            dlbutton.IsEnabled = False
            convertgroup.Visibility = Windows.Visibility.Collapsed
            Return
        End If
        convertgroup.Visibility = Windows.Visibility.Visible
        dlbutton.IsEnabled = True
    End Sub
    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Urlbox.Paste()
    End Sub

    Private Sub Click(sender As Object, e As RoutedEventArgs)
        Dim ext As Object = DirectCast(e.OriginalSource, RibbonButton).Tag
        Select Case DirectCast(DirectCast(dlbutton, RibbonButton).Tag, vServiceKind)
            Case vServiceKind.Youtube
                ytdl(Urlbox.Text, CStr(ext))
            Case vServiceKind.Niconico
                ncdl(Urlbox.Text, CStr(ext))
        End Select
    End Sub
    Private Sub RibbonSplitButton_Click(sender As Object, e As RoutedEventArgs)
        If sender.IsChecked Then
            Rapid.IsChecked = False
            Medium.IsChecked = False
            Fine.IsChecked = False
        Else
            sender.IsChecked = True
        End If
    End Sub
    Private Sub DefaultResolutionClicked() Handles Rapid.Checked, Medium.Checked, Fine.Checked
        CustomRes.IsChecked = False
    End Sub
    Private Sub Ribbon_Loaded(sender As Object, e As RoutedEventArgs) Handles CustomRes.Click
        If Not _loaded Then setCustomRes()
    End Sub
#End Region
#Region "ロジック"
    Private Sub ytdl(url As String)
        Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
        Dim ctrl_Inst As New dlqueue
        Dim saveto As String = getStartupPath() + "\Download\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + param.sourceext
        ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, "")
        ctrl_Inst.start()
    End Sub
    Private Sub ytdl(url As String, ext As String)
        Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
        Dim ctrl_Inst As New dlqueue
        Dim saveto As String = getStartupPath() + "\temp\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + param.sourceext
        Dim output As String = IO.Path.ChangeExtension(getStartupPath() + "\Download\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + param.sourceext, ext)
        'TODO fmt値 Uris(18)ってとこ
        ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, "", getlinestr(ext, saveto, output))
        ctrl_Inst.start()
    End Sub
    Private Sub ncdl(url As String)
        Dim res As New Dictionary(Of String, String)
        Dim param As UriCookiePair = nc_dl.getDownloadParam(url, res)
        Dim saveto As String = getStartupPath() + "\Download\" + res("thread_id") + "." + param.sourceext
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(0)), saveto, "", param.cookie, "")
        ctrl_Inst.start()
    End Sub
    Private Sub ncdl(url As String, ext As String)
        Dim res As New Dictionary(Of String, String)
        Dim param As UriCookiePair = nc_dl.getDownloadParam(url, res)
        Dim saveto As String = getStartupPath() + "\temp\" + res("thread_id") + "." + param.sourceext
        Dim output As String = IO.Path.ChangeExtension(getStartupPath() + "\Download\" + res("thread_id") + "." + param.sourceext, ext)
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(0)), saveto, "", param.cookie, "", getlinestr(ext, saveto, output))
        ctrl_Inst.start()
    End Sub
    Function getlinestr(ext As String, input As String, Optional out As String = "") As String
        Dim linestr As String = rs_loadconvertCfg.getConvertablefileKinds(False)(ext)
        linestr = linestr.Replace("<input>", input)
        If out = "" Then
            linestr = linestr.Replace("<output>", IO.Path.ChangeExtension(input, ext))
        Else
            linestr = linestr.Replace("<output>", out)
        End If
        Return linestr
    End Function

    ' TODO: config.xmlの自動読み込み
    Private Sub loadffmcfg()

    End Sub
    ' yt画質の設定の読込
    Private _loaded As Boolean = False
    Private Sub setCustomRes()
        _loaded = True
        Dim t As Hashtable = getResolution()
        For Each v As Newtonsoft.Json.Linq.JObject In t.Values
            Dim s As String = String.Format("{0}: {1}, {2} ({3}, {4})", v("itag"), v("description"), v("format"), v("acodec"), v("vcodec"))
            Debug.WriteLine(s)
            'Dim c As RibbonGalleryItem = (New RibbonGalleryItem With {.Content = s, .Tag = v.DeepClone, .IsSelected = My.Settings.yt_qTarget = CInt(v("itag"))})
            'AddHandler c.Ch, New RoutedEventHandler(Sub(sender As Object, e As RoutedEventArgs) My.Settings.yt_qTarget = v("itag"))
        Next
    End Sub
#End Region
End Class