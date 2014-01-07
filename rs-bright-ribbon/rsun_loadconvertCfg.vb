Imports System.Xml
Module rs_loadconvertCfg
    Dim _xml As XmlDocument
    Private Sub Main()
        If chk_CfgExist() AndAlso _xml Is Nothing Then
            _xml = New XmlDocument
            _xml.Load(New IO.FileStream(getStartupPath() + "\config.xml", IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read))
        End If
    End Sub
    Private Function chk_CfgExist() As Boolean
        Return IO.File.Exists(getStartupPath() + "\config.xml")
    End Function

    Function getConvertablefileKinds(forBuildCvtList As Boolean) As Dictionary(Of String, String)
        If _xml Is Nothing Then
            Main()
        End If
        Dim l As New Dictionary(Of String, String)
        If forBuildCvtList Then
            For Each e As XmlNode In _xml.GetElementsByTagName("root")
                For Each e2 As XmlNode In e.ChildNodes
                    For Each e3 As XmlElement In e2.ChildNodes
                        l.Add(e3.InnerText + "| *." + e3.Name, e3.GetAttribute("arg"))
                        Debug.WriteLine(String.Format("Add an item {0}(Type {1})", e3.GetAttribute("arg"), e3.Name))
                    Next
                Next
            Next
        Else
            For Each e As XmlElement In _xml.GetElementsByTagName("root")
                For Each e2 As XmlNode In e.ChildNodes
                    For Each e3 As XmlElement In e2.ChildNodes
                        l.Add(e3.Name, e3.GetAttribute("arg"))
                    Next
                Next
            Next
        End If
        Return l
    End Function

    Function buildCvtListForSFD() As String()
        'この関数は、SaveFileDialog.Filter 向けのデータを返します。
        Dim returnvalue As New List(Of String)
        For Each x As String In getConvertablefileKinds(True).Keys
            returnvalue.Add(x)
        Next
        Return returnvalue.ToArray()
        returnvalue.Clear()
    End Function
End Module
