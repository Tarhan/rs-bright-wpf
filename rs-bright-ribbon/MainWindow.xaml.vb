Imports Microsoft.Windows.Controls.Ribbon
Imports System.Text.RegularExpressions

Class MainWindow
    Inherits RibbonWindow
#Region "イベントハンドラ"
    Enum vServiceKind
        Niconico
        Niconama
        Youtube
    End Enum
    Private Sub dlbutton_Click(sender As Object, e As RoutedEventArgs) Handles dlbutton.Click
        Select Case DirectCast(DirectCast(e.Source, RibbonButton).Tag, vServiceKind)
            Case vServiceKind.Youtube
                ytdl(Urlbox.Text)
            Case vServiceKind.Niconico
                'TODO
        End Select

    End Sub

    Private Sub RibbonTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Regex.IsMatch(Urlbox.Text, "http://(www\.)?youtube\.com/watch?.*") Then
            dlbutton.IsEnabled = True
            dlbutton.Tag = vServiceKind.Youtube

        ElseIf Regex.IsMatch(Urlbox.Text, "http://(www\.)?nicovideo\.jp/watch/[sn][mo]\d+") Then
            dlbutton.IsEnabled = True
            dlbutton.Tag = vServiceKind.Niconico
        Else
            dlbutton.Tag = Nothing
            dlbutton.IsEnabled = False
        End If
    End Sub
    Private Sub PublishReq(attribute As vServiceKind, url As String)    '情報取得要求
        Select Case attribute
            Case vServiceKind.Youtube

            Case vServiceKind.Niconico

        End Select
    End Sub
#End Region
#Region "ロジック"
    Private Sub ytdl(url As String)
        Dim param As UriCookiePair = downloadViaGDataapi.getDownloadParam(url)
        Dim ctrl_Inst As New dlqueue
        ctrl_Inst.SetInfo(New Uri(param.Uris(18)), getStartupPath() + "\" + Uri.EscapeDataString(param.VideoInfo.Title), "", param.cookie, "")
        'TODO ダウンロード用のディレクトリを作成↑
        ctrl_Inst.start()
    End Sub
#End Region
End Class