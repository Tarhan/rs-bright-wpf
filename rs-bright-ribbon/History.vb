Module History
    Sub Init()
        Dim i As New IO.DirectoryInfo(My.Settings.Savepath)
        For Each f As IO.FileInfo In i.EnumerateFiles()
            rs_loadconvertCfg.getConvertablefileKinds(False)

        Next
    End Sub

End Module
