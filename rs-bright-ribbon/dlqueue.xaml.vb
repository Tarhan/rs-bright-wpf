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
            Else
                restart_connection()
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
        Dim cc As New Net.CookieContainer
        resume_req.CookieContainer = cc
        cc.SetCookies(info.url, info.ck)
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
                                                                 Dim s As String = String.Format("{0}MB / {1}MB", Format(p.BytesReceived / 1000000, "0.00"), Format(p.TotalBytesToReceive / 1000000, "0.00"))
                                                                 Dispatcher.Invoke(Sub()
                                                                                       speed.Text = s
                                                                                       monitor.Value = p.ProgressPercentage
                                                                                   End Sub)
                                                             End Sub).
                                  Aggregate(data,
                                            Function(list, p)
                                                list.AddRange(p.Value)
                                                state = p
                                                add_downloaded()
                                                Dispatcher.Invoke(Sub() speed.Text = String.Format("{0} kB/s", getCurrentSpeedkb()))
                                                Return list
                                            End Function).Subscribe(Sub(s) Return, Sub(ex As Exception)
                                                                                       If TryCast(ex, Net.WebException) Is Nothing Then
                                                                                       ElseIf DirectCast(ex, Net.WebException).Status = Net.WebExceptionStatus.RequestCanceled Then
                                                                                           'TODO
                                                                                       Else
                                                                                           Dispatcher.Invoke(Sub() speed.Text = DirectCast(ex, Net.WebException).Message)
                                                                                       End If
                                                                                   End Sub, AddressOf Oncompleted)
    End Function
#End Region
#Region "演算系"
    ' download
    Structure downloadspeed
        Dim progress As AsynchronousExtensions.Progress(Of Byte())
        Dim time As TimeSpan
        Sub New(p As AsynchronousExtensions.Progress(Of Byte()), timer As Stopwatch)
            progress = p
            If Not timer.IsRunning Then Throw New Exception
            time = timer.Elapsed
        End Sub
    End Structure
    Public state As AsynchronousExtensions.Progress(Of Byte()) 'これのTotalBytesToReceiveで全体が求まる
    Public speedlist As New List(Of downloadspeed)
    Public speedvalList As New List(Of Integer)
    Private Sub add_downloaded()
        If speedlist.Count > 0 AndAlso (time.Elapsed - speedlist.Last.time).Seconds < 1 Then Return
        If Not state Is Nothing Then speedlist.Add(New downloadspeed(state, time))
    End Sub

    Public Function getCurrentSpeedkb() As Integer
        If speedlist.Count < 2 Then Return 0
        Dim lastitem As downloadspeed = speedlist.Last
        Dim prev As downloadspeed = speedlist(speedlist.Count - 2)
        On Error GoTo err
        speedvalList.Add((lastitem.progress.BytesReceived - prev.progress.BytesReceived) / (lastitem.time - prev.time).Milliseconds)
        Return speedvalList.Last
err:    Return 0
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
                              End Sub)
        End If
    End Sub
#End Region
#Region "レジューム制御"
    Private Sub restart_connection()
        resume_req = CType(Net.WebRequest.Create(info.url), Net.HttpWebRequest)
        resume_req.AddRange(data.Count)
        Dim cc As New Net.CookieContainer
        resume_req.CookieContainer = cc
        cc.SetCookies(info.url, info.ck)
        ReqDisposeProvider = setOperation()
        time.Restart()
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