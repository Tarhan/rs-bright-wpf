Public Class Zero_core

    Public Property Title As String
    Private Sub Frame_Navigated(sender As Object, e As NavigationEventArgs)
        url.Text = e.Uri.AbsoluteUri
    End Sub
    Event TitleChanged(sender As Object, e As RoutedEventArgs)
    Private Sub WebBrowser_LoadCompleted(sender As Object, e As NavigationEventArgs)
        Dim myDocument As mshtml.HTMLDocument = DirectCast(sender.Document, mshtml.HTMLDocument)

        If Title <> myDocument.title Then
            Title = myDocument.title
            RaiseEvent TitleChanged(Me, Nothing)
        End If
    End Sub
End Class
