Class Application

    ' Startup、Exit、DispatcherUnhandledException などのアプリケーション レベルのイベントは、
    ' このファイルで処理できます。

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs)
        On Error Resume Next
        My.Computer.FileSystem.DeleteDirectory(getStartupPath() + "\Temp", FileIO.DeleteDirectoryOption.DeleteAllContents)
    End Sub
End Class
