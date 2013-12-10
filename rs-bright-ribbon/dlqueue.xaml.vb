Imports System.Net
Imports System.Text.RegularExpressions
Imports AsynchronousExtensions
Imports System.Reactive.Linq

Public Class dlqueue

    Dim _Dulation As TimeSpan
    Friend Structure dlInfo
        Dim url As Uri      'uri.
        Dim ck As String    'cookie.
        Dim rf As String    'Referrer.
        Dim dst As String   'Destination.
    End Structure
#Region "Property section"
    Private info As dlInfo
    Private _stopflag As Boolean
    Private resume_req As HttpWebRequest
    Private ReqDisposeProvider As IDisposable
    Private time As New Stopwatch
    Public Property IsStopped() As Boolean
        Get
            Return _stopflag
        End Get
        Set(ByVal value As Boolean)
            If value Then
                pause()
                Debug.WriteIf(value, String.Format("I/Oは{0}バイトで正常に中断", data.Count))
                time.Stop()
                Debug.Print("Stopwatch successfully stopped.")
            Else
                restart_connection()
                Debug.Print("Conection successfully restarted, and stopwatch")
            End If
            _stopflag = value
        End Set
    End Property
    Public _linestr As String   'ffmpeg
    Dim data As New List(Of Byte)   'downloaded
#End Region


#Region "値チェック/開始"
    Public Sub SetInfo(ByVal Url As Uri, ByVal dst As String, ByVal contentTitle As String, Optional ByVal ck As String = "", Optional ByVal img As String = "", Optional linestr As String = "")
        Me.speed.Text = "Waiting..."
        Me.title.Text = contentTitle
        If String.IsNullOrWhiteSpace(contentTitle) Then Me.title.Text = IO.Path.GetFileName(dst)
        If Not String.IsNullOrWhiteSpace(img) Then thumb.Source = New BitmapImage(New Uri(img))
        info = New dlInfo With {.url = Url, .ck = ck, .dst = dst}
        _linestr = linestr
        DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Add(Me)
    End Sub
    Public Sub start()
        Me.speed.Text = "Connecting..."
        With info
            Proc(.url, .dst, .ck, .rf)
        End With
        time.Start()
    End Sub
#End Region
#Region "メイン"
    Private Sub Proc(ByVal Url As Uri, ByVal dst As String, Optional ByVal ck As String = "", Optional ByVal rf As String = "")
        resume_req = CType(Net.WebRequest.Create(info.url), Net.HttpWebRequest)
        If Not String.IsNullOrEmpty(info.ck) Then
            Dim cc As New Net.CookieContainer
            resume_req.CookieContainer = cc
            cc.SetCookies(info.url, info.ck)
        End If
        ReqDisposeProvider = setOperation()
    End Sub
    Sub Oncompleted()
        IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(info.dst))
        My.Computer.FileSystem.WriteAllBytes(info.dst, data.ToArray, False)
        Dispatcher.Invoke(Sub()
                              Me.PauseButton.IsEnabled = False
                              Me.speed.Text = Me.speed.Text + vbCrLf + "変換中…"
                              StartConvert()
                          End Sub)
        data.Clear()
    End Sub
    Dim FFMProcess As Process
    Private Sub StartConvert()
        If Not String.IsNullOrWhiteSpace(_linestr) Then
            FFMProcess = New Process
            With FFMProcess
                With .StartInfo
                    .FileName = getStartupPath() + "\ffmpeg.exe"
                    .Arguments = Me._linestr.Replace(vbCrLf, "")
                    .ErrorDialog = True
                    .WindowStyle = ProcessWindowStyle.Minimized
                    .UseShellExecute = False
                    .CreateNoWindow = True
                    .RedirectStandardError = True
                End With
                .EnableRaisingEvents = True
            End With
            AddHandler FFMProcess.Exited, Sub()
                                              If FFMProcess.ExitCode <> 0 Then
                                                  Debug.Print("""" + getStartupPath() + "\ffmpeg.exe""" & " " & Me._linestr.Replace(vbCrLf, ""))
                                                  MsgBox("変換エラー Exitcode:" + FFMProcess.ExitCode)
                                              End If
                                              FFMProcess.Dispose()
                                              IO.File.Delete(info.dst)
                                              Release()
                                          End Sub
            AddHandler FFMProcess.ErrorDataReceived, AddressOf logging
            FFMProcess.Start()
            FFMProcess.BeginErrorReadLine()
        Else
                Release()
        End If
    End Sub
#End Region
#Region "中断用オブジェクトの付与"
    Private Function setOperation() As IDisposable
        Return resume_req.DownloadDataAsyncWithProgress().Do(Sub(p)
                                                                 Dim s As String = String.Format("{0}MB / {1}MB", Format(data.Count / 1000000, "0.0"), Format((latestrange + p.TotalBytesToReceive) / 1000000, "0.0"))
                                                                 Dispatcher.Invoke(Sub()
                                                                                       speed.Text = s
                                                                                       monitor.Value = (data.Count / Convert.ToInt32(latestrange + p.TotalBytesToReceive)) * 100
                                                                                   End Sub)
                                                             End Sub).
                                  Aggregate(data,
                                            Function(list, p)
                                                list.AddRange(p.Value)
                                                Dispatcher.Invoke(Sub()
                                                                      kbpers.Text = String.Format("{0} kB/s", getCurrentSpeedkb(p.BytesReceived))
                                                                      Dim t As String = CStr(Format(getLeftTime(data.Count, latestrange + p.TotalBytesToReceive), "0.0"))
                                                                      'If 3.5 < t And t <= 6 Then t = "数"
                                                                      Nokori.Text = String.Format("{0}秒後...", t)
                                                                  End Sub)
                                                Return list
                                            End Function).Subscribe(Sub(s) Return, AddressOf Oncompleted)
    End Function
#End Region
#Region "演算系"
    ' download
    Private Function getCurrentSpeedkb(bytesReceived As Integer) As Integer
        Return bytesReceived / time.ElapsedMilliseconds
    End Function
    Private Function getLeftTime(bytesReceived As Integer, total As Integer)
        Return (total - bytesReceived) / (bytesReceived / time.Elapsed.Seconds)
    End Function

    ' ffmpeg

    Private Sub logging(sender As Object, e As System.Diagnostics.DataReceivedEventArgs)
        '正規表現でffmpegの進捗状況をとる
        Dim t As String = CStr(e.Data)
        On Error Resume Next
        Debug.WriteLine(t)
        Dim r1 As MatchCollection
        If Not _Dulation = TimeSpan.Zero Then GoTo Getstate
        r1 = Regex.Matches(t, "(?<=Duration:\s)[0123456789:\.]+")
        If r1.Count > 0 Then
            Debug.Assert(TimeSpan.TryParse(r1.Item(0).Value, _Dulation))
        Else
            Exit Sub
        End If
Getstate: r1 = Regex.Matches(t, "(?<=time\=)[0123456789\:\.]+")
        If r1.Count > 0 Then
            Dim current As Double = CInt(TimeSpan.Parse(r1.Item(0).Value).TotalMilliseconds)
            Dim total As Double = CInt(_Dulation.TotalMilliseconds)
            Dispatcher.Invoke(Sub()
                                  monitor.Maximum = total
                                  monitor.Value = current
                                  speed.Text = "変換中... " + Format(current / total * 100, "0.0") + "%"
                                  Nokori.Text = "しばらくお待ちください..."
                              End Sub)
        End If
    End Sub
#End Region
#Region "レジューム制御"
    Dim latestrange As Integer = 0
    Private Sub restart_connection()
        resume_req = CType(Net.WebRequest.Create(info.url), Net.HttpWebRequest)
        resume_req.AddRange(data.Count)
        latestrange = data.Count
        Dim cc As New Net.CookieContainer
        resume_req.CookieContainer = cc
        cc.SetCookies(info.url, info.ck)
        ReqDisposeProvider = setOperation()
    End Sub
    Public Sub pause()
        ReqDisposeProvider.Dispose()
        speed.Text += " (中断中)"
    End Sub
#End Region

    Private Sub Release()
        Dispatcher.Invoke(Sub()
                              DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Remove(Me)
                          End Sub)
    End Sub

    Private Sub PauseButton_Checked(sender As System.Object, e As System.Windows.RoutedEventArgs)
        IsStopped = True
        sender.Header = "再開"
    End Sub

    Private Sub PauseButton_Unchecked(sender As System.Object, e As System.Windows.RoutedEventArgs)
        IsStopped = False
        sender.Header = "中断"
    End Sub
End Class