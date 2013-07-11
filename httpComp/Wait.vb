Module Wait
    Sub Main(ByVal milliSec As Integer)
        Dim t As New Threading.Thread(New Threading.ParameterizedThreadStart(Sub()
                                                                                 System.Windows.Forms.Application.DoEvents()
                                                                                 Threading.Thread.Sleep(milliSec)
                                                                                 System.Windows.Forms.Application.DoEvents()
                                                                             End Sub))
        t.Start()
        t.Join()
    End Sub
End Module
