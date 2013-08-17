Public Class AccordionToggle
    Dim _c As Boolean = False
    Public Property IsCollapsed As Boolean
        Get
            Return _c
        End Get
        Set(value As Boolean)
            If value Then
                Dim newEventArgs As New RoutedEventArgs(AccordionToggle.ce)
                MyBase.RaiseEvent(newEventArgs)
            Else
                Dim newEventArgs As New RoutedEventArgs(AccordionToggle.ee)
                MyBase.RaiseEvent(newEventArgs)
            End If
            _c = value
        End Set
    End Property
    Public Shared ReadOnly ce As RoutedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(AccordionToggle))
    Public Shared ReadOnly ee As RoutedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(AccordionToggle))
    Public Custom Event Collapsed As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Me.AddHandler(ce, value)
        End AddHandler

        RemoveHandler(ByVal value As RoutedEventHandler)
            Me.RemoveHandler(ce, value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Me.RaiseEvent(e)
        End RaiseEvent
    End Event
    Public Custom Event Expanded As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Me.AddHandler(ee, value)
        End AddHandler

        RemoveHandler(ByVal value As RoutedEventHandler)
            Me.RemoveHandler(ee, value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Me.RaiseEvent(e)
        End RaiseEvent
    End Event

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        IsCollapsed = Not IsCollapsed
    End Sub
End Class
