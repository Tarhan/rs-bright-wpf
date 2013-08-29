Imports Microsoft.Windows.Controls.Ribbon
Imports System.Text.RegularExpressions
Imports libSpirit.html

Class MainWindow
    Inherits RibbonWindow
    Enum vServiceKind
        No
        Niconico
        Niconama
        Youtube
        Ustream
    End Enum
    Dim rtmpController As New RecCtrl

#Region "イベントハンドラ"
    Private Sub dlbutton_Click(sender As Object, e As RoutedEventArgs) Handles dlbutton.Click
        Select Case DirectCast(DirectCast(e.Source, RibbonButton).Tag, vServiceKind)
            Case vServiceKind.Youtube
                ytdl(Urlbox.Text)
            Case vServiceKind.Niconico
                ncdl(Urlbox.Text)
            Case vServiceKind.Ustream
                ustdl(Urlbox.Text)
        End Select
    End Sub
    Private Sub RibbonTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Regex.IsMatch(Urlbox.Text, "http://(www\.)?youtube\.com/watch\?.*") OrElse Regex.IsMatch(Urlbox.Text, "http://youtu\.be/\w+") Then
            dlbutton.Tag = vServiceKind.Youtube
        ElseIf Regex.IsMatch(Urlbox.Text, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+") Then
            dlbutton.Tag = vServiceKind.Niconico
        ElseIf Regex.IsMatch(Urlbox.Text, "http://(www\.)?ustream.tv/recorded/.*") Then
            dlbutton.Tag = vServiceKind.Ustream
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
        e.Handled = True
    End Sub
    Private Sub CustomResButtonClick(sender As Object, e As RoutedEventArgs) Handles CustomRes.Click
        With CustomRes
            My.Settings.settingstate = .IsChecked
            If .IsChecked Then
                Medium.IsChecked = False
                Rapid.IsChecked = False
                Fine.IsChecked = False
            Else
                Select Case getCurrentfmt()
                    Case 18
                        Rapid.IsChecked = True
                    Case 22
                        Fine.IsChecked = True
                    Case 35
                        Medium.IsChecked = True
                End Select
            End If
        End With
        Debug.Print(getCurrentfmt)
    End Sub
    Private Sub CustomResItemClicked(sender As Object, e As RoutedEventArgs)
        ytconfigure(DirectCast(sender, RibbonGallery).SelectedItem.Tag)
    End Sub
    Private Sub ytconfigure(itag As Integer)
        If itag = 0 Then Return
        My.Settings.ytTarget_custom = itag
        Debug.WriteLine(itag)
        Select Case itag
            Case 18
                CustomRes.IsChecked = False
                Medium.IsChecked = False
                Rapid.IsChecked = True
                Fine.IsChecked = False
            Case 22
                CustomRes.IsChecked = False
                Medium.IsChecked = False
                Rapid.IsChecked = False
                Fine.IsChecked = True
            Case 35
                CustomRes.IsChecked = False
                Medium.IsChecked = True
                Rapid.IsChecked = False
                Fine.IsChecked = False
            Case Else
                CustomRes.IsChecked = True
                Medium.IsChecked = False
                Rapid.IsChecked = False
                Fine.IsChecked = False
        End Select
        My.Settings.settingstate = CustomRes.IsChecked
        For Each i As RibbonGalleryItem In Res_Cat.Items
            i.IsSelected = CInt(i.Tag) = CInt(itag)
        Next
    End Sub
    Private Function getCurrentfmt() As Integer
        If My.Settings.settingstate Then
            Return My.Settings.ytTarget_custom
        Else
            Return My.Settings.ytTarget_ClickedState
        End If
    End Function
    Private Sub DefaultResolutionClicked(sender As Object, e As RoutedEventArgs) Handles Rapid.Checked, Medium.Checked, Fine.Checked
        My.Settings.ytTarget_ClickedState = sender.tag
        My.Settings.settingstate = False
        CustomRes.IsChecked = False
        Debug.WriteLine(getCurrentfmt)
    End Sub
    Private Sub currrenturichanged() Handles uiCtl.UrlOfCurrentTabChanged
        Dim attribute As vServiceKind
        If Regex.IsMatch(uiCtl.UrlOfCurrentTab, "http://(www\.)?youtube\.com/watch\?.*") OrElse Regex.IsMatch(Urlbox.Text, "http://youtu\.be/\w+") Then
            attribute = vServiceKind.Youtube
        ElseIf Regex.IsMatch(uiCtl.UrlOfCurrentTab, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+") Then
            attribute = vServiceKind.Niconico
        Else
            attribute = vServiceKind.No
        End If
        ActivateContextTab(uiCtl.UrlOfCurrentTab, attribute)
    End Sub
    Private Sub DlQueueAdded(addedObj As DependencyObject) Handles Queueboard.vadded
        expclbtn.IsCollapsed = False
    End Sub
    Private Sub RibbonWindow_Loaded(sender As Object, e As RoutedEventArgs)
        Me.expclbtn.IsCollapsed = True
    End Sub
    Private Sub TabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Me.expclbtn.IsCollapsed = False
    End Sub
#End Region
#Region "ロジック"
    Private Sub ytdl(url As String)
        Try
            Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
            Dim ctrl_Inst As New dlqueue
            Dim fmt As Integer
            If param.getFmtIdWhichContains.Contains(getCurrentfmt) Then
                fmt = getCurrentfmt()
            Else
                fmt = 18
            End If
            Dim saveto As String = my.settings.savepath + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + UriCookiePair.getExtention(fmt)
            ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, param.VideoInfo.Thumbnails.Item(0).Url)
            ctrl_Inst.start()
        Catch ex As Net.WebException

        End Try
    End Sub
    Private Sub ytdl(url As String, ext As String)
        If String.IsNullOrEmpty(ext) Then ytdl(url) : Return
        Try
            Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
            Dim ctrl_Inst As New dlqueue
            Dim fmt As Integer
            If param.getFmtIdWhichContains.Contains(getCurrentfmt) Then
                fmt = getCurrentfmt()
            Else
                fmt = 18
            End If
            Dim saveto As String = getStartupPath() + "\temp\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + UriCookiePair.getExtention(fmt)
            Dim output As String = my.settings.savepath + IO.Path.GetFileNameWithoutExtension(saveto) + "." + ext
            'TODO fmt値 Uris(18)ってとこ
            ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, param.VideoInfo.Thumbnails.Item(0).Url, getlinestr(ext, saveto, output))
            ctrl_Inst.start()
        Catch ex As Net.WebException

        End Try
    End Sub
    Private Sub ncdl(url As String)
        Dim res As New Dictionary(Of String, String)
        Dim param As UriCookiePair = nc_dl.getDownloadParam(url, res)
        Dim saveto As String = my.settings.savepath + res("thread_id") + "." + param.sourceext
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(0)), saveto, "", param.cookie, param.thumbUrl)
        ctrl_Inst.start()
    End Sub
    Private Sub ncdl(url As String, ext As String)
        If String.IsNullOrEmpty(ext) Then ncdl(url) : Return
        Dim res As New Dictionary(Of String, String)
        Dim param As UriCookiePair = nc_dl.getDownloadParam(url, res)
        Dim saveto As String = getStartupPath() + "\temp\" + res("thread_id") + "." + param.sourceext
        Dim output As String = my.settings.savepath + IO.Path.GetFileNameWithoutExtension(saveto) + "." + ext
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(0)), saveto, "", param.cookie, param.thumbUrl, getlinestr(ext, saveto, output))
        ctrl_Inst.start()
    End Sub
    Private Sub ustdl(url As String)
        Dim info As KK_HTTP_REQ = ust(url)
        Dim ctrl_Inst As New dlqueue
        Dim x As New Xml.XmlDocument
        x.Load(New Xml.XmlTextReader(url.Replace("www.ustream.tv/recorded", "api.ustream.tv/xml/video") + "/getinfo"))
        Dim title As String = x.SelectSingleNode("/xml/results/title").FirstChild.Value
        ctrl_Inst.SetInfo(info.KK_HTTP_TARGET_URI, My.Settings.Savepath + ustRec.getCID(url) + "." + info.KK_HTTP_PRELOADED_FILENAME, title)
        ctrl_Inst.start()
    End Sub

#Region "録画"
    Public Sub ustRecord(url As String)
        Dim fixed As String = ustRec.getCID(url)
        Dim command As String = String.Format("-re -y -i {0} out.mov", ustRec.getLiveUrl(fixed))
        Dim FFMProcess As New Process
        With FFMProcess.StartInfo
            .FileName = getStartupPath() + "\ffmpeg.exe"
            .Arguments = command
            '.ErrorDialog = True
            '.WindowStyle = ProcessWindowStyle.Minimized
            '.UseShellExecute = False
            '.CreateNoWindow = True
            '.RedirectStandardError = True
        End With
        FFMProcess.EnableRaisingEvents = True
        'AddHandler FFMProcess.ErrorDataReceived, Sub(sender, e) Debug.WriteLine(e.Data)
        FFMProcess.Start()
        'FFMProcess.BeginErrorReadLine()
    End Sub


    Public Class RecCtrl
        Sub Add(dstDir As String, uri As String)
            If Not IO.Directory.Exists(dstDir) Then Throw New Exception

        End Sub
    End Class

#End Region

    '動画サイト用
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
    Private Sub load() Handles Me.Loaded
        ytconfigure(My.Settings.ytTarget_custom)
        AddHandler My.Settings.PropertyChanged, Sub() My.Settings.Save()
        If Not IO.Directory.Exists(My.Settings.Savepath) Then My.Settings.Savepath = Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos)
    End Sub
    ' yt画質の設定の読込
    'Private _loaded As Boolean = False
    'Private Sub setCustomRes()
    '    _loaded = True
    '    Dim t As Hashtable = getResolution()
    '    For Each v As Newtonsoft.Json.Linq.JObject In t.Values
    '        Dim s As String = String.Format("{0}: {1}, {2} ({3}, {4})", v("itag"), v("description"), v("format"), v("acodec"), v("vcodec"))
    '        Debug.WriteLine(s)
    '    Next
    'End Sub
#Region "contextualtabctl"
    Private Sub ActivateContextTab(uri As String, attr As vServiceKind)
        Select Case attr
            Case vServiceKind.No
                cGroup.Visibility = Windows.Visibility.Hidden
                cGroup.Tag = Nothing
            Case Else
                cGroup.Visibility = Windows.Visibility.Visible
                cGroup.Tag = New contextattributecollection With {.uri = uri, .attribute = attr}
        End Select
    End Sub
    Private Structure contextattributecollection
        Dim attribute As vServiceKind
        Dim uri As String
    End Structure
    Private Sub ContextClick(sender As Object, e As RoutedEventArgs)
        Select Case DirectCast(cGroup.Tag, contextattributecollection).attribute
            Case vServiceKind.Niconico
                ncdl(DirectCast(cGroup.Tag, contextattributecollection).uri, e.Source.Tag)
            Case vServiceKind.Youtube
                ytdl(DirectCast(cGroup.Tag, contextattributecollection).uri, e.Source.Tag)
        End Select
        e.Handled = True
    End Sub
#End Region
#End Region

    Private Sub FolderChangeButton_Click(sender As Object, e As RoutedEventArgs)
        'TODO フォルダ指定  My.Settings.Savepath
        Using d As New Windows.Forms.FolderBrowserDialog
            d.RootFolder = Environment.SpecialFolder.MyVideos
            d.ShowNewFolderButton = True
            If d.ShowDialog = Forms.DialogResult.OK Then My.Settings.Savepath = d.SelectedPath
        End Using
    End Sub

    Private Sub dbg(sender As Object, e As RoutedEventArgs)
        If tb.Text Like "http://www.ustream.tv/channel/*" Then
            ustRecord(tb.Text)
        Else
            MsgBox("URL形式が違います")
        End If
    End Sub
End Class