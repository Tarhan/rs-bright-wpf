Imports Microsoft.Windows.Controls.Ribbon
Imports System.Text.RegularExpressions

Class MainWindow
    Inherits RibbonWindow
    Enum vServiceKind
        No
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
        If Regex.IsMatch(Urlbox.Text, "http://(www\.)?youtube\.com/watch\?.*") OrElse Regex.IsMatch(Urlbox.Text, "http://youtu\.be/\w+") Then
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
    Private Sub CustomResItemClicked(sender As Object, e As RoutedEventArgs)
        ytconfigure(sender.Tag)
        CustomRes.IsChecked = True
    End Sub
    Private Sub ytconfigure(itag As Integer)
        My.Settings.yt_qTarget = itag
    End Sub
    Private Sub DefaultResolutionClicked(sender As Object, e As RoutedEventArgs) Handles Rapid.Checked, Medium.Checked, Fine.Checked
        CustomRes.IsChecked = False
        ytconfigure(CInt(sender.Tag))
        For Each i As RibbonGalleryItem In Res_Cat.Items
            i.IsSelected = CInt(i.Tag) = CInt(sender.tag)
        Next
    End Sub
    Private Sub Ribbon_Loaded(sender As Object, e As RoutedEventArgs) Handles CustomRes.Click
        If Not _loaded Then setCustomRes()
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
#End Region
#Region "ロジック"
    Private Sub ytdl(url As String)
        Try
            Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
            Dim ctrl_Inst As New dlqueue
            Dim fmt As Integer
            If param.getFmtIdWhichContains(My.Settings.yt_qTarget) Then
                fmt = My.Settings.yt_qTarget
            Else
                fmt = 18
            End If
            Dim saveto As String = getStartupPath() + "\Download\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + UriCookiePair.getExtention(fmt)
            ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, "")
            ctrl_Inst.start()
        Catch ex As Net.WebException

        End Try
    End Sub
    Private Sub ytdl(url As String, ext As String)
        Try
            Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
            Dim ctrl_Inst As New dlqueue
            Dim fmt As Integer
            If param.getFmtIdWhichContains(My.Settings.yt_qTarget) Then
                fmt = My.Settings.yt_qTarget
            Else
                fmt = 18
            End If
            Dim saveto As String = getStartupPath() + "\temp\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + UriCookiePair.getExtention(fmt)
            Dim output As String = getStartupPath() + "\Download\" + IO.Path.GetFileNameWithoutExtension(saveto) + "." + ext
            'TODO fmt値 Uris(18)ってとこ
            ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, "", getlinestr(ext, saveto, output))
            ctrl_Inst.start()
        Catch ex As Net.WebException

        End Try
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
        Dim output As String = IO.Path.GetFileNameWithoutExtension(saveto) + "." + ext
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
        ' TODO: 画質設定の自動読み込み
    Private Sub loadytcfg() Handles MyBase.Initialized
        Select Case My.Settings.yt_qTarget
            Case 18
                Rapid.IsChecked = True
            Case 22
                Fine.IsChecked = True
            Case 35
                Medium.IsChecked = True
            Case Else
                For Each i As RibbonGalleryItem In Res_Cat.Items
                    i.IsSelected = CInt(i.Tag) = CInt(My.Settings.yt_qTarget)
                Next
        End Select

        AddHandler My.Settings.PropertyChanged, Sub() My.Settings.Save()
    End Sub
    ' yt画質の設定の読込
    Private _loaded As Boolean = False
    Private Sub setCustomRes()
        _loaded = True
        Dim t As Hashtable = getResolution()
        For Each v As Newtonsoft.Json.Linq.JObject In t.Values
            Dim s As String = String.Format("{0}: {1}, {2} ({3}, {4})", v("itag"), v("description"), v("format"), v("acodec"), v("vcodec"))
            Debug.WriteLine(s)
        Next
    End Sub
#Region "contextualtabctl"
    Private Sub ActivateContextTab(uri As String, attr As vServiceKind)
        Select Case attr
            Case vServiceKind.No
                context.Visibility = Windows.Visibility.Hidden
                context.Tag = Nothing
            Case Else
                context.Visibility = Windows.Visibility.Visible
                context.Tag = New contextattributecollection With {.uri = uri, .attribute = attr}
        End Select
    End Sub
    Private Structure contextattributecollection
        Dim attribute As vServiceKind
        Dim uri As String
    End Structure
    Private Sub ContextClick(sender As Object, e As RoutedEventArgs)
        'TODO: コンテキストタブのボタンの命令
    End Sub
#End Region
#End Region

    Private Sub Expander_Changed(sender As Object, e As RoutedEventArgs)
        sender.Height = sender.ActualHeight
    End Sub

    Private Sub RibbonButton_Click(sender As Object, e As RoutedEventArgs)
        'TODO
    End Sub
End Class