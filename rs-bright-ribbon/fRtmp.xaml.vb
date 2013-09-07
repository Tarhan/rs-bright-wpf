Public Class fRtmp
    WithEvents t As New Timers.Timer With {.Interval = 500, .Enabled = True}
    Sub RefreshStatus() Handles t.Elapsed

    End Sub
End Class