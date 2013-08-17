Public Class MyWrapPanel
    Inherits WrapPanel
    Protected Overrides Sub OnVisualChildrenChanged(visualAdded As DependencyObject, visualRemoved As DependencyObject)
        If Not visualAdded Is Nothing Then
            RaiseEvent vAdded(visualAdded)
        ElseIf Not visualRemoved Is Nothing Then
            RaiseEvent vRemoved(visualRemoved)
        End If
        MyBase.OnVisualChildrenChanged(visualAdded, visualRemoved)
    End Sub
    Event vAdded(targ As DependencyObject)
    Event vRemoved(targ As DependencyObject)
End Class
