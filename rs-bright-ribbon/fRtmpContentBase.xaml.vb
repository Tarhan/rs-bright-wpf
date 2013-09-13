Public Class fRtmpContentBase
    Inherits UserControl
    Sub SetThumbnail(thumb As ImageSource)
        thumbnail.Source = thumb
    End Sub

    Public Property Time As TimeSpan
        Get
            Return GetValue(TimeProperty)
        End Get

        Set(ByVal value As TimeSpan)
            SetValue(TimeProperty, value)
            Dim ts As String = value.ToString("hh:mm:ss")
            Description.Text = ts
        End Set
    End Property

    Public TimeProperty As DependencyProperty = _
                           DependencyProperty.Register("Time", _
                           GetType(TimeSpan), GetType(fRtmpContentBase), _
                           New PropertyMetadata(TimeSpan.Zero))

    Public Property title As String
        Get
            Return GetValue(titleProperty)
        End Get

        Set(ByVal value As String)
            SetValue(titleProperty, value)
            Live_name.Text = value
        End Set
    End Property

    Public titleProperty As DependencyProperty = _
                           DependencyProperty.Register("title", _
                           GetType(String), GetType(fRtmpContentBase), _
                           New PropertyMetadata("None"))



End Class