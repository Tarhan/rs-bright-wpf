<<<<<<< HEAD
﻿Imports System.Net
Imports System.Text.RegularExpressions
Imports AsynchronousExtensions
Imports System.Reactive.Linq

Public Class dlqueue

    Dim _Dulation As TimeSpan

    Sub New()

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。
        monitor.IsIndeterminate = False
    End Sub
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
    Public Property IsStopped() As Boolean
        Get
            Return _stopflag
        End Get
        Set(ByVal value As Boolean)
            If value Then
                Debug.WriteIf(value, String.Format("I/Oは{0}バイトで正常に中断", data.Count))
            Else
            End If
            _stopflag = value
        End Set
    End Property
    Public _linestr As String   'ffmpeg
    Dim data As New List(Of Byte)   'downloaded
    Dim thumbUri As Uri 'Thumbnail URI
#End Region

#Region "中断用オブジェクトの付与"
    Private Function setOperation() As IDisposable
        Return resume_req.DownloadDataAsyncWithProgress().Do(Sub(p)
                                                                 Dim s As String = String.Format("{0}/{1} - {2}%", p.BytesReceived, p.TotalBytesToReceive, p.ProgressPercentage)
                                                                 Dispatcher.Invoke(Sub() speed.Text = s)
                                                             End Sub).
                                  Aggregate(data,
                                            Function(list, p)
                                                Debug.WriteLine(String.Format("AddRange {0} +{1}", list.Count, p.Value.Length))
                                                list.AddRange(p.Value)
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

#Region "値チェック/開始"
    Public Sub SetInfo(ByVal Url As Uri, ByVal dst As String, ByVal contentTitle As String, Optional ByVal ck As String = "", Optional ByVal rf As String = "", Optional linestr As String = "", Optional ByVal img As Uri = Nothing)
        Me.speed.Text = "Waiting..."
        Me.title.Text = contentTitle
        info = New dlInfo With {.url = Url, .ck = ck, .rf = rf, .dst = dst}
        _linestr = linestr
        DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Add(Me)
    End Sub
    Public Sub start()
        Me.speed.Text = "Connecting..."
        With info
            Proc(.url, .dst, .ck, .rf)
        End With
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
        IO.File.WriteAllBytes(info.dst, data.ToArray)
        Dispatcher.Invoke(Sub()
                              Me.Pause.IsEnabled = False
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
                AddHandler .ErrorDataReceived, AddressOf logging
                .EnableRaisingEvents = True
                AddHandler .Exited, Sub()
                                        If .ExitCode <> 0 Then
                                            Debug.Print("""" + getStartupPath() + "\ffmpeg.exe""" & " " & Me._linestr.Replace(vbCrLf, ""))
                                            MsgBox(.ExitCode)
                                        End If
                                        .Dispose()
                                        IO.File.Delete(info.dst)
                                        Release()
                                    End Sub
                .Start()
                .BeginErrorReadLine()
            End With
        Else
            Release()
        End If
    End Sub
#End Region
#Region "演算系"
    ' download



    ' ffmpeg

    Private Sub logging(sender As Object, e As System.Diagnostics.DataReceivedEventArgs)
        '正規表現でffmpegの進捗状況をとる
        Dim d As Net.WebRequest
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
        On Error Resume Next
Getstate: r1 = Regex.Matches(t, "(?<=time\=)[0123456789\.]+")
        If r1.Count > 0 Then
            Dim current As Double = CInt(TimeSpan.FromSeconds(CDbl(r1.Item(0).Value)).TotalMilliseconds)
            Dim total As Double = CInt(_Dulation.TotalMilliseconds)
            Dispatcher.Invoke(Sub()
                                  monitor.Maximum = total
                                  monitor.Value = current
                              End Sub)
        End If
    End Sub

#End Region
#Region "レジューム制御"
    Private Sub restart_connection()
        IsStopped = False
        resume_req = CType(Net.WebRequest.Create(info.url), Net.HttpWebRequest)
        resume_req.AddRange(data.Count)
        Dim cc As New Net.CookieContainer
        resume_req.CookieContainer = cc
        cc.SetCookies(info.url, info.ck)
        ReqDisposeProvider = setOperation()
    End Sub

#End Region

    Private Sub Release()
        Dispatcher.Invoke(Sub()
                              'TODO コントロールの削除
                              DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Remove(Me)
                          End Sub)
    End Sub

End Class
=======
﻿Imports System.Net
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
    Public Property IsStopped() As Boolean
        Get
            Return _stopflag
        End Get
        Set(ByVal value As Boolean)
            If value Then
                Debug.WriteIf(value, String.Format("I/Oは{0}バイトで正常に中断", data.Count))
            Else
            End If
            _stopflag = value
        End Set
    End Property
    Public _linestr As String   'ffmpeg
    Dim data As New List(Of Byte)   'downloaded
    Dim thumbUri As Uri 'Thumbnail URI
#End Region

#Region "中断用オブジェクトの付与"
    Private Function setOperation() As IDisposable
        Return resume_req.DownloadDataAsyncWithProgress().Do(Sub(p)
                                                                 Dim s As String = String.Format("{0}/{1} - {2}%", p.BytesReceived, p.TotalBytesToReceive, p.ProgressPercentage)
                                                                 Dispatcher.Invoke(Sub()
                                                                                       speed.Text = s
                                                                                       monitor.Value = p.ProgressPercentage
                                                                                   End Sub)
                                                             End Sub).
                                  Aggregate(data,
                                            Function(list, p)
                                                Debug.WriteLine(String.Format("AddRange {0} +{1}", list.Count, p.Value.Length))
                                                list.AddRange(p.Value)
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

#Region "値チェック/開始"
    Public Sub SetInfo(ByVal Url As Uri, ByVal dst As String, ByVal contentTitle As String, Optional ByVal ck As String = "", Optional ByVal rf As String = "", Optional linestr As String = "", Optional ByVal img As Uri = Nothing)
        Me.speed.Text = "Waiting..."
        Me.title.Text = contentTitle
        info = New dlInfo With {.url = Url, .ck = ck, .rf = rf, .dst = dst}
        _linestr = linestr
        DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Add(Me)
    End Sub
    Public Sub start()
        Me.speed.Text = "Connecting..."
        With info
            Proc(.url, .dst, .ck, .rf)
        End With
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
        IO.File.WriteAllBytes(info.dst, data.ToArray)
        Dispatcher.Invoke(Sub()
                              Me.Pause.IsEnabled = False
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
                AddHandler .ErrorDataReceived, AddressOf logging
                .EnableRaisingEvents = True
            End With
            AddHandler FFMProcess.Exited, Sub()
                                              If FFMProcess.ExitCode <> 0 Then
                                                  Debug.Print("""" + getStartupPath() + "\ffmpeg.exe""" & " " & Me._linestr.Replace(vbCrLf, ""))
                                                  MsgBox(FFMProcess.ExitCode)
                                              End If
                                              FFMProcess.Dispose()
                                              IO.File.Delete(info.dst)
                                              Release()
                                          End Sub
            FFMProcess.Start()
            FFMProcess.BeginErrorReadLine()
        Else
                Release()
        End If
    End Sub
#End Region
#Region "演算系"
    ' download



    ' ffmpeg

    Private Sub logging(sender As Object, e As System.Diagnostics.DataReceivedEventArgs)
        '正規表現でffmpegの進捗状況をとる
        Dim d As Net.WebRequest
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
        On Error Resume Next
Getstate: r1 = Regex.Matches(t, "(?<=time\=)[0123456789\.]+")
        If r1.Count > 0 Then
            Dim current As Double = CInt(TimeSpan.FromSeconds(CDbl(r1.Item(0).Value)).TotalMilliseconds)
            Dim total As Double = CInt(_Dulation.TotalMilliseconds)
            Dispatcher.Invoke(Sub()
                                  monitor.Maximum = total
                                  monitor.Value = current
                              End Sub)
        End If
    End Sub

#End Region
#Region "レジューム制御"
    Private Sub restart_connection()
        IsStopped = False
        resume_req = CType(Net.WebRequest.Create(info.url), Net.HttpWebRequest)
        resume_req.AddRange(data.Count)
        Dim cc As New Net.CookieContainer
        resume_req.CookieContainer = cc
        cc.SetCookies(info.url, info.ck)
        ReqDisposeProvider = setOperation()
    End Sub

#End Region

    Private Sub Release()
        Dispatcher.Invoke(Sub()
                              'TODO コントロールの削除
                              DirectCast(Application.Current.MainWindow, MainWindow).Queueboard.Children.Remove(Me)
                          End Sub)
    End Sub

End Class
>>>>>>> develop
