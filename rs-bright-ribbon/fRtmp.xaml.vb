Public Class fRtmp
    Sub New()

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        Me.DataContext = m_rtmpctrl._l
    End Sub
End Class