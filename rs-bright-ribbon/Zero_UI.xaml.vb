﻿''' <summary>
''' ページが閉じられた際に発生するイベントを処理するハンドラのデリゲート
''' </summary>
''' <param name="sender"></param>
''' <param name="e"></param>
Public Delegate Sub PageCloseEventHandler(sender As [Object], e As PageCloseEventArgs)


''' <summary>
''' TabItemを閉じることが出来るTabControl
''' </summary>
Public Class Zero_UI
    Inherits TabControl
    Shared Sub New()
        '外観はカスタマイズしないので、TabControlのスタイルを適用する
        'DefaultStyleKeyProperty.OverrideMetadata(GetType(TabControl), New FrameworkPropertyMetadata(GetType(TabControl)))
    End Sub

    ''' <summary>
    ''' TagPageのCloseButtonClickEventのハンドラを登録する
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        [AddHandler](ExtendedTabItem.CloseButtonClickEvent, New RoutedEventHandler(AddressOf PageCloseButtonClick))
    End Sub

    ''' <summary>
    ''' Pageが閉じられた際に発生するルーティングイベントを識別するための識別子
    ''' </summary>
    Public Shared ReadOnly PageCloseEvent As RoutedEvent = EventManager.RegisterRoutedEvent("PageClose", RoutingStrategy.Bubble, GetType(PageCloseEventHandler), GetType(Zero_UI))

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

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        root.Items.Insert(root.Items.IndexOf(addtab), New ExtendedTabItem)
    End Sub
End Class


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