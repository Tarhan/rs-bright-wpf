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
    Public Shared ReadOnly CurrentUriStateChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("CurrentUriStateChanged", RoutingStrategy.Bubble, GetType(TabUriChangedRoutedEventArgs), GetType(Zero_core))
    Public Custom Event CurrentUriStateChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            Me.AddHandler(CurrentUriStateChangedEvent, value)
        End AddHandler

        RemoveHandler(value As RoutedEventHandler)
            Me.RemoveHandler(CurrentUriStateChangedEvent, value)
        End RemoveHandler

        RaiseEvent(sender As Object, e As TabUriChangedRoutedEventArgs)
            Me.RaiseEvent(e)
        End RaiseEvent
    End Event

    Private Sub Zero_core_TitleChanged(sender As Object, e As RoutedEventArgs)
        Me.Header = DirectCast(sender, Zero_core).Title
    End Sub

    Private Sub Zero_core_UriChanged(sender As Object, e As RoutedEventArgs)
        Dim eventarg As New TabUriChangedRoutedEventArgs(CurrentUriStateChangedEvent)
        eventarg.Uri = sender.Uri
        eventarg.Index = DirectCast(Me.Parent, Zero_UI).SelectedIndex
        RaiseEvent CurrentUriStateChanged(Me, eventarg)
    End Sub
End Class

Public Class TabUriChangedRoutedEventArgs
    Inherits RoutedEventArgs
    Sub New(routedEvent As RoutedEvent)
        MyBase.New(routedEvent)
    End Sub
    Public Uri As String
    Public Index As Integer
End Class
