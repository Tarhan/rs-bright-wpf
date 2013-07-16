Module Startuppath
    Function getStartupPath() As String
        Dim exePath As String = Environment.GetCommandLineArgs()(0)
        Dim exeFullPath As String = System.IO.Path.GetFullPath(exePath)
        Dim startupPath As String = System.IO.Path.GetDirectoryName(exeFullPath)
        Return startupPath
    End Function
End Module
