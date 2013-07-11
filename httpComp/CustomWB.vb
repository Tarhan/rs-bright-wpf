Public Class CustomWB
    Inherits WebBrowser
    Property busy As bstate
    Shadows Property ScriptErrorsSuppressed As Boolean = True
    Public Event WWWLoaded_withoutIframe(sender As Object, e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs)
    Private Sub docComplete_hook(sender As Object, e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles Me.DocumentCompleted
        If e.Url.AbsoluteUri = Me.Url.AbsoluteUri Then
            RaiseEvent WWWLoaded_withoutIframe(sender, e)
        End If
    End Sub
    Private Sub navigates() Handles Me.Navigating
        busy = New bstate With {.flg = True}
    End Sub
    Private Sub loaded(sender As Object, e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles Me.WWWLoaded_withoutIframe
        busy = New bstate With {.flg = False, .url = e.Url.AbsoluteUri, .timing = DateTime.Now}
    End Sub
    Public Structure bstate
        Dim flg As Boolean
        Dim url As String
        Dim timing As DateTime
    End Structure
End Class
