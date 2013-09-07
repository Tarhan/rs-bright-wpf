Public Class fRtmpContentBase
    Sub New(thumb As ImageSource)

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        thumbnail.Source = thumb
    End Sub
    Sub SetInfo(title As String, timelapse As TimeSpan)
        Dim ts As String = timelapse.ToString("hh:mm:ss")
        Description.Text = ts
    End Sub
End Class