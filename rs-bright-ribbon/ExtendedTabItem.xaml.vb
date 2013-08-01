''' <summary>
''' 閉じるボタンにより閉じることが可能なTabItem
''' </summary>
Public Class ExtendedTabItem
    Inherits TabItem
    Shared Sub New()
        DefaultStyleKeyProperty.OverrideMetadata(GetType(ExtendedTabItem), New FrameworkPropertyMetadata(GetType(ExtendedTabItem)))
    End Sub

    ''' <summary>
    ''' 閉じるボタンがクリックされた場合に発生させるルーティングイベントの識別子
    ''' </summary>
    Public Shared ReadOnly CloseButtonClickEvent As RoutedEvent = EventManager.RegisterRoutedEvent("CloseButtonClick", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(ExtendedTabItem))

    ''' <summary>
    ''' イベント
    ''' </summary>
    Public Custom Event CloseButtonClick As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            [AddHandler](CloseButtonClickEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](CloseButtonClickEvent, value)
        End RemoveHandler
        RaiseEvent()

        End RaiseEvent
    End Event

    ''' <summary>
    ''' テンプレートが摘要された際に発生するイベントのハンドラ
    ''' 閉じるボタンがクリックされた際のイベントハンドラを登録する
    ''' </summary>
    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()

        Dim closeButton As Button = TryCast(MyBase.GetTemplateChild("CloseButton"), Button)
        AddHandler closeButton.Click, New RoutedEventHandler(AddressOf closeButton_Click)
    End Sub

    ''' <summary>
    ''' 閉じるボタンがクリックされた際のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs)
        'CloseButtonClickEventを発生させる
        [RaiseEvent](New RoutedEventArgs(CloseButtonClickEvent, Me))

        '処理済にする
        e.Handled = True
    End Sub

    Private Sub Zero_core_TitleChanged(sender As Object, e As RoutedEventArgs)
        Me.Header = DirectCast(sender, Zero_core).Title
    End Sub
End Class
