Imports Microsoft.Windows.Controls.Ribbon
Imports System.Text.RegularExpressions

Class MainWindow
    Inherits RibbonWindow
    Enum vServiceKind
        Niconico
        Niconama
        Youtube
    End Enum

    Sub New()

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        'For Each entry As DictionaryEntry In getResolution()
        '    ' TODO
        'Next
    End Sub

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
                'TODO
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
        Dim saveto As String = getStartupPath() + "\Download\" + (System.Text.RegularExpressions.Regex.Match(url, "(?<=v=)[\w-]+").Value) + "." + param.sourceext
        'TODO fmt値 Uris(18)ってとこ
        ctrl_Inst.SetInfo(New Uri(param.Uris(18)), saveto, param.VideoInfo.Title, param.cookie, "", getlinestr(ext, saveto))
        ctrl_Inst.start()
    End Sub
    Private Sub ncdl(url As String)
        Dim res As New Dictionary(Of String, String)
        Dim param As UriCookiePair = nc_dl.getDownloadParam(url, res)
        Dim saveto As String = getStartupPath() + "\Download\" + res("thread_id") + "." + param.sourceext
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(0)), saveto, "", param.cookie, "")
        ' ↑拡張子の判定も

        ctrl_Inst.start()
    End Sub

    ' TODO: config.xmlの自動読み込み
    Private Sub loadffmcfg()

    End Sub
    ' TODO: outは未実装
    Function getlinestr(ext As String, input As String, Optional out As String = "") As String
        Dim linestr As String = rs_loadconvertCfg.getConvertablefileKinds(False)(ext)
        linestr = linestr.Replace("<input>", input)
        If out = "" Then
            linestr = linestr.Replace("<output>", IO.Path.ChangeExtension(input, ext))
        Else
            linestr = linestr.Replace("<output>", IO.Path.ChangeExtension(out, ext))
        End If
        Return linestr
    End Function
#End Region
End Class