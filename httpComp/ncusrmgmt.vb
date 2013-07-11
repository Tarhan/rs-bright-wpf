Module ncusrmgmt
    Dim cfgpath As String = Application.StartupPath + "user.cfg"
    Structure n_usrprof
        Dim user As String
        Dim pass As String
        Dim IsNull As Boolean
    End Structure
    Property conf As n_usrprof
        Get
            Dim r As New n_usrprof
            If IO.File.Exists(cfgpath) Then
                Dim lst As IEnumerable(Of String) = IO.File.ReadLines(cfgpath)
                r.user = lst(0)
                r.pass = lst(1)
            Else
                r.IsNull = True
            End If
            Return r
        End Get
        Set(value As n_usrprof)
            IO.File.WriteAllText(cfgpath, value.user & vbCrLf & value.pass)
        End Set
    End Property
End Module
