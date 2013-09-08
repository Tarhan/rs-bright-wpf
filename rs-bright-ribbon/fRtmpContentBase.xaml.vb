Public Class fRtmpContentBase
    Inherits UserControl
    Sub SetThumbnail(thumb As ImageSource)
        thumbnail.Source = thumb
    End Sub
    Public Property Time As TimeSpan
        Get

        End Get
        Set(value As TimeSpan)
            Dim ts As String = value.ToString("hh:mm:ss")
            Description.Text = ts
        End Set
    End Property
    Public Property title As String
        Get

        End Get
        Set(value As String)
            Live_name.Text = value
        End Set
    End Property
End Class