Public Class dlqueue
    Public Property MaxValue As Integer
    Public Property Value As Integer
    Protected Overrides Sub OnRender(drawingContext As DrawingContext)
        MyBase.OnRender(drawingContext)
        Dim rect As New Rect(New Point(0, 0), New Size(Me.ActualWidth * Value / MaxValue, Me.ActualHeight))
        drawingContext.DrawRectangle(Windows.Media.Brushes.GreenYellow, Nothing, rect)
    End Sub


End Class
