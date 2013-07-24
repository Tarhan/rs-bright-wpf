Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports rs_bright_ribbon.ExtendedTabItem

''' <summary>
''' ExtendedTabControlをホストするユーザコントロール
''' </summary>
Partial Public Class tabbrowser
    Inherits UserControl
    Private _isDivided As Boolean = False
    Private _isDividable As Boolean = False

    ''' <summary>
    ''' ドラッグ操作により左右のタブのサイズが変更されている
    ''' 最中かどうかを示す
    ''' </summary>
    Private _isTabSizeChanging As Boolean = False

    ''' <summary>
    ''' Panelの左側に表示するTabControl
    ''' </summary>
    Private _leftTab As ExtendedTabControl = Nothing

    ''' <summary>
    ''' Panelの右側に表示するTabControl
    ''' </summary>
    Private _rightTab As ExtendedTabControl = Nothing

    '現在開いているページのコントロールのリスト
    Private _openedPages As List(Of UserControl) = Nothing

    ''' <summary>
    ''' IsDividableの値が変更された場合の発生するイベントハンドラ
    ''' </summary>
    Public Event DividableValueChange As EventHandler

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        'TabControlを初期化する
        _leftTab = New ExtendedTabControl()
        _rightTab = New ExtendedTabControl()

        _openedPages = New List(Of UserControl)()

        'ページが閉じた際のイベントにハンドラを登録する

        [AddHandler](ExtendedTabControl.PageCloseEvent, New PageCloseEventHandler(AddressOf TabPageClosed))
    End Sub

    ''' <summary>
    ''' ページが閉じた際のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TabPageClosed(sender As [Object], e As PageCloseEventArgs)
        Dim tab As TabControl = TryCast(e.OriginalSource, ExtendedTabControl)
        Dim page As ExtendedTabItem = e.ClosedItem

        '開いているページのリストから削除する
        _openedPages.Remove(TryCast(page.Content, UserControl))

        'Tabが左右に分割されている場合
        If _isDivided Then
            'Tabにページが存在しない場合
            If tab.Items.Count = 0 Then
                OpenSingle()
            End If
        Else
            '分割されていない場合
            If _leftTab.Items.Count >= 2 Then
                IsDividable = True
            Else
                IsDividable = False
            End If
        End If
    End Sub

    ''' <summary>
    ''' 2つのTabControlが左右に分割されて表示されている
    ''' 状態かどうかを示す
    ''' trueの場合、分割表示されている場合、falseの場合、単一の
    ''' TabControlが表示されている
    ''' </summary>
    Public Property IsDivided() As Boolean
        Get
            Return _isDivided
        End Get
        Set(value As Boolean)
            _isDivided = value
        End Set
    End Property

    ''' <summary>
    ''' 2つのTabControlをPanelの左右に分割して表示する形に
    ''' 変更することが出来るかどうかを示す。
    ''' 現在、1つTabControlが表示されている状態で、かつTabItemが2つ以上、
    ''' 登録されている場合、分割できる状態とし、trueを返す。それ以外の場合は
    ''' false。TabHostPanelの表示切替を行うためのメニューItemなどを制御するために
    ''' 使用される
    ''' </summary>
    Public Property IsDividable() As Boolean
        Get
            Return _isDividable
        End Get
        Set(value As Boolean)
            _isDividable = value

            RaiseEvent DividableValueChange(Me, EventArgs.Empty)
        End Set
    End Property

    ''' <summary>
    ''' 指定されたページが現在開かれているかどうかを返す
    ''' </summary>
    ''' <param name="control"></param>
    ''' <returns></returns>
    Private Function ContainsPage(controlType As Type) As Boolean
        For Each control As UserControl In _openedPages
            If controlType.Equals(control.[GetType]()) Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' 左側のタブにTabItemを追加する
    ''' </summary>
    Public Sub OpenPage(tabPage As Type, header As [String])
        'ページが開かれていない場合
        If Not ContainsPage(tabPage) Then
            Dim item As New ExtendedTabItem()
            item.Header = header

            Dim pageControl As UserControl = TryCast(Activator.CreateInstance(tabPage), UserControl)
            item.Content = pageControl

            _leftTab.Items.Add(item)
            _leftTab.SelectedItem = item

            '開いているページのリストに追加する
            _openedPages.Add(pageControl)

            '追加されたページが最初のページの場合、
            '何故か、そのコンテンツが表示されない現象が
            '発生する。それを回避するため、2つ目のページを追加し
            '即座に削除している
            If _leftTab.Items.Count = 1 Then
                item = New ExtendedTabItem()
                item.Header = "ダミー"
                item.Content = "ダミー"
                _leftTab.Items.Add(item)

                _leftTab.Items.Remove(item)
            End If
        Else
            For Each tabItem As ExtendedTabItem In _leftTab.Items
                If tabItem.Content.[GetType]().Equals(tabPage) Then
                    _leftTab.SelectedItem = tabItem
                    Exit For
                End If
            Next
        End If

        'ページを追加した結果、タブ内に2ページ以上ある場合
        If _leftTab.Items.Count >= 2 Then
            IsDividable = True
        Else
            IsDividable = False
        End If
    End Sub

    ''' <summary>
    ''' 単一のTabControlをPanel内に表示する
    ''' </summary>
    Public Sub OpenSingle()
        'Gridを初期化する
        _tabPanel.Children.Clear()
        _tabPanel.ColumnDefinitions.Clear()
        _tabPanel.RowDefinitions.Clear()

        '列を定義する
        Dim col As New ColumnDefinition()
        _tabPanel.ColumnDefinitions.Add(col)

        '行を定義する
        Dim row As New RowDefinition()
        _tabPanel.RowDefinitions.Add(row)

        '右側のTabControlに追加されているTabItemを左側の
        'TabControlに移す
        Dim rightPages As New List(Of ExtendedTabItem)()
        For Each page As ExtendedTabItem In _rightTab.Items
            rightPages.Add(page)
        Next

        _rightTab.Items.Clear()

        For Each page As ExtendedTabItem In rightPages
            _leftTab.Items.Add(page)
        Next

        '左側のTabControlをPabelに追加する
        _tabPanel.Children.Add(_leftTab)
        Grid.SetColumn(_leftTab, 0)
        Grid.SetRow(_leftTab, 0)

        _isDivided = False

        '左側のTabに2つ以上のページが有る場合
        '分割可能フラグをたてる
        If _leftTab.Items.Count >= 2 Then
            IsDividable = True
        Else
            IsDividable = False
        End If
    End Sub

    ''' <summary>
    ''' Panelの左右に2つのTabControlを開く
    ''' </summary>
    Public Sub OpenDouble()
        '一旦クリアする
        _tabPanel.Children.Clear()
        _tabPanel.ColumnDefinitions.Clear()
        _tabPanel.RowDefinitions.Clear()

        '列を定義する
        Dim col1 As New ColumnDefinition()
        _tabPanel.ColumnDefinitions.Add(col1)

        Dim col2 As New ColumnDefinition()
        _tabPanel.ColumnDefinitions.Add(col2)

        '行を定義する
        Dim row As New RowDefinition()
        _tabPanel.RowDefinitions.Add(row)

        '現在選択されているTabのページを
        '右側に開くTabControlに移動する
        Dim selectedItem As ExtendedTabItem = TryCast(_leftTab.SelectedItem, ExtendedTabItem)
        _leftTab.Items.Remove(selectedItem)

        _rightTab.Items.Add(selectedItem)

        Dim item As New ExtendedTabItem()
        item.Header = "ダミー"
        item.Content = "ダミー"
        _rightTab.Items.Add(item)
        _rightTab.Items.Remove(item)

        '追加しなおす
        _tabPanel.Children.Add(_leftTab)
        Grid.SetColumn(_leftTab, 0)
        Grid.SetRow(_leftTab, 0)

        _tabPanel.Children.Add(_rightTab)
        Grid.SetColumn(_rightTab, 1)
        Grid.SetRow(_rightTab, 0)

        _isDivided = True
        IsDividable = False
    End Sub

    ''' <summary>
    ''' マウスカーソルが左右のタブの境界エリアにあるか
    ''' どうかを判定する
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    Private Function IsInBorder(e As MouseEventArgs) As Boolean
        Dim leftX As Double = e.GetPosition(_leftTab).X
        Dim rightX As Double = e.GetPosition(_rightTab).X

        If (leftX > _leftTab.RenderSize.Width - 5 AndAlso leftX <= _leftTab.RenderSize.Width) OrElse (_rightTab.RenderSize.Width <> 0 AndAlso rightX >= 0 AndAlso rightX < 5) Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' TabControlをホストするGrid内をマウスが移動した場合のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub _tabPanel_MouseMove(sender As Object, e As MouseEventArgs)
        'Tabが分割されている場合
        If _isDivided Then
            'サイズ変更中の場合
            If _isTabSizeChanging Then
                Dim mouseX As Double = e.GetPosition(_tabPanel).X

                If mouseX > 40 AndAlso mouseX < _tabPanel.RenderSize.Width - 40 Then
                    '左側の列サイズを変更する
                    _tabPanel.ColumnDefinitions(0).Width = New GridLength(mouseX)
                End If
            Else
                '左右のタブの境界エリアにいる場合
                If IsInBorder(e) Then
                    _tabPanel.Cursor = Cursors.SizeWE
                Else
                    _tabPanel.Cursor = Cursors.Arrow
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' TabControlをホストするGrid内でマウスの左ボタンが押された場合のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub _tabPanel_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        '左右に分割されている場合
        If _isDivided Then
            '左右のタブの境界エリアにいる場合
            If IsInBorder(e) Then
                _isTabSizeChanging = True

                'マウスをキャプチャする
                _tabPanel.CaptureMouse()
            End If
        End If
    End Sub


    ''' <summary>
    ''' マウスの左ボタンが離された場合のイベントハンドラ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub _tabPanel_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        '左右に分割されている場合
        If _isDivided Then
            'サイズ変更中の場合
            If _isTabSizeChanging Then
                _isTabSizeChanging = False
            End If
        End If

        If _tabPanel.IsMouseCaptured Then
            'マウスのキャプチャを開放する
            _tabPanel.ReleaseMouseCapture()
        End If
    End Sub
End Class
''' <summary>
''' ページが閉じられた際に発生するイベントを処理するハンドラのデリゲート
''' </summary>
''' <param name="sender"></param>
''' <param name="e"></param>
Public Delegate Sub PageCloseEventHandler(sender As [Object], e As PageCloseEventArgs)


''' <summary>
''' TabItemを閉じることが出来るTabControl
''' </summary>
Public Class ExtendedTabControl
    Inherits TabControl
    Shared Sub New()
        '外観はカスタマイズしないので、TabControlのスタイルを適用する
        DefaultStyleKeyProperty.OverrideMetadata(GetType(ExtendedTabControl), New FrameworkPropertyMetadata(GetType(TabControl)))
    End Sub

    ''' <summary>
    ''' TagPageのCloseButtonClickEventのハンドラを登録する
    ''' </summary>
    Public Sub New()
        [AddHandler](ExtendedTabItem.CloseButtonClickEvent, New RoutedEventHandler(AddressOf PageCloseButtonClick))
    End Sub

    ''' <summary>
    ''' Pageが閉じられた際に発生するルーティングイベントを識別するための識別子
    ''' </summary>
    Public Shared ReadOnly PageCloseEvent As RoutedEvent = EventManager.RegisterRoutedEvent("PaeClose", RoutingStrategy.Bubble, GetType(PageCloseEventHandler), GetType(TabControl))

    ''' <summary>
    ''' TabPageの閉じるボタンのクリックイベントを処理して
    ''' PageCloseEventを発生させる 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PageCloseButtonClick(sender As [Object], e As RoutedEventArgs)
        Dim page As ExtendedTabItem = TryCast(e.OriginalSource, ExtendedTabItem)

        '自身からページを削除する
        Items.Remove(page)

        'ルーティングイベントを発生させる
        [RaiseEvent](New PageCloseEventArgs(PageCloseEvent, Me, page))
    End Sub
End Class

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
    ''' <summary>
    ''' TabPageが閉じられた際に発生するPageCloseEventの情報を管理するクラス
    ''' </summary>
    Public Class PageCloseEventArgs
        Inherits RoutedEventArgs
        Private _closedItem As ExtendedTabItem = Nothing

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="routedEvent"></param>
        ''' <param name="originalSource"></param>
        ''' <param name="closedPage"></param>
        Public Sub New(routedEvent As RoutedEvent, originalSource As [Object], closedItem As ExtendedTabItem)
            MyBase.New(routedEvent, originalSource)
            _closedItem = closedItem
        End Sub

        ''' <summary>
        ''' 閉じられたページ
        ''' </summary>
        Public Property ClosedItem() As ExtendedTabItem
            Get
                Return _closedItem
            End Get
            Set(value As ExtendedTabItem)
                _closedItem = value
            End Set
        End Property

    End Class
End Class