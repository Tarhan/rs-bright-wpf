Module m_rtmpctrl
    Friend _l As New List(Of Process)
    Friend Function NewProcess(cmdoption As String, vtitle As String)
        Dim p As New Process
        With p.StartInfo
            .FileName = getFFMpath()
            .Arguments = cmdoption
            .CreateNoWindow = True
            .RedirectStandardError = True
            .RedirectStandardInput = True
            .UseShellExecute = False
        End With
        p.EnableRaisingEvents = True
        _l.Add(p)
    End Function
End Module