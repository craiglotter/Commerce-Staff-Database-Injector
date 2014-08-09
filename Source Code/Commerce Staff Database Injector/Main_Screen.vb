Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text
Imports System.Security.Cryptography



Public Class Main_Screen

    Private busyworking As Boolean = False

    Private lastinputline As String = ""
    Private inputlines As Long = 0
    Private highestPercentageReached As Integer = 0
    Private inputlinesprecount As Long = 0
    Private pretestdone As Boolean = False
    Private primary_PercentComplete As Integer = 0
    Private percentComplete As Integer

    Private SelectedIndex As Integer = 0

    Private backupdirectory As String = ""
    Private savedirectory As String = ""

    Private AlertMessage As String = ""




    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If ex.Message.IndexOf("Thread was being aborted") < 0 Then
                Dim Display_Message1 As New Display_Message()
                Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.Message.ToString

                Display_Message1.Timer1.Interval = 1000
                Display_Message1.ShowDialog()
                Dim dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
                If dir.Exists = False Then
                    dir.Create()
                End If
                dir = Nothing
                Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
                filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ": " & ex.ToString)
                filewriter.WriteLine("")
                filewriter.Flush()
                filewriter.Close()
                filewriter = Nothing
            End If
            ex = Nothing
            identifier_msg = Nothing
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub




   



    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim result As DialogResult
        result = OpenFileDialog2.ShowDialog
        If result = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = OpenFileDialog2.FileName
        End If
    End Sub


    


    Private Sub cancelAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancelAsyncButton.Click

        ' Cancel the asynchronous operation.
        Me.BackgroundWorker1.CancelAsync()

        ' Disable the Cancel button.
        cancelAsyncButton.Enabled = False
        sender = Nothing
        e = Nothing
    End Sub 'cancelAsyncButton_Click

    Private Sub PreCount_Function(ByVal worker As BackgroundWorker)
        Try
            inputlinesprecount = 0
            inputlines = 0
            Dim reader As StreamReader = New StreamReader(TextBox1.Text)
            While reader.Peek <> -1
                If reader.ReadLine.Trim = "--" Then
                    inputlinesprecount = inputlinesprecount + 1
                    inputlines = inputlines + 1
                    worker.ReportProgress(0)
                End If
            End While
            reader.Close()
            reader = Nothing
            inputlinesprecount = inputlinesprecount - 1
            inputlines = inputlines - 1
        Catch ex As Exception
            Error_Handler(ex, "PreCount_Function")
        End Try
    End Sub

    Private Sub startAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles startAsyncButton.Click
        Try
            If busyworking = False Then
                If My.Computer.FileSystem.FileExists(TextBox1.Text) Then
                    If My.Computer.FileSystem.FileExists(TextBox2.Text) Then




                        busyworking = True


                        inputlines = 0
                        lastinputline = ""
                        highestPercentageReached = 0
                        inputlinesprecount = 0

                        backupdirectory = ""
                        savedirectory = ""
                        pretestdone = False

                        TextBox1.Enabled = False
                        Button1.Enabled = False
                        TextBox2.Enabled = False
                        Button2.Enabled = False
                        startAsyncButton.Enabled = False
                        cancelAsyncButton.Enabled = True
                        ' Start the asynchronous operation.
                        AlertMessage = ""

                        BackgroundWorker1.RunWorkerAsync(TextBox1.Text)
                    Else
                        MsgBox("Please ensure that you select an existing Commerce Staff database to process", MsgBoxStyle.Information, "Invalid Database Selected")
                    End If
                Else
                    MsgBox("Please ensure that you select an existing text input file to process", MsgBoxStyle.Information, "Invalid Input File Selected")
                End If
                End If
        Catch ex As Exception
            Error_Handler(ex, "StartWorker")
        End Try
    End Sub

    ' This event handler is where the actual work is done.
    Private Sub backgroundWorker1_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        ' Get the BackgroundWorker object that raised this event.
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)

        ' Assign the result of the computation
        ' to the Result property of the DoWorkEventArgs
        ' object. This is will be available to the 
        ' RunWorkerCompleted eventhandler.
        e.Result = MainWorkerFunction(worker, e)
        sender = Nothing
        e = Nothing
        worker.Dispose()
        worker = Nothing
    End Sub 'backgroundWorker1_DoWork

    ' This event handler deals with the results of the
    ' background operation.
    Private Sub backgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        busyworking = False


        ' First, handle the case where an exception was thrown.
        If Not (e.Error Is Nothing) Then
            Error_Handler(e.Error, "backgroundWorker1_RunWorkerCompleted")
        ElseIf e.Cancelled Then
            ' Next, handle the case where the user canceled the 
            ' operation.
            ' Note that due to a race condition in 
            ' the DoWork event handler, the Cancelled
            ' flag may not have been set, even though
            ' CancelAsync was called.
            Me.ToolStripStatusLabel1.Text = "Operation Cancelled" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 0

        Else
            ' Finally, handle the case where the operation succeeded.
            Me.ToolStripStatusLabel1.Text = "Operation Completed" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 100
            If AlertMessage.Length > 0 Then
                'MsgBox("The following alerts were raised during the operation. If you wish to save these alerts, press Ctrl+C and paste it into NotePad." & vbCrLf & vbCrLf & "********************" & vbCrLf & vbCrLf & AlertMessage, MsgBoxStyle.Information, "Raised Alerts")
                MsgBox("The following inputs appear to have been ignored: " & vbCrLf & vbCrLf & AlertMessage, MsgBoxStyle.Information, "Inputs Ignored")
            End If
        End If

        TextBox1.Enabled = True
        Button1.Enabled = True
        TextBox2.Enabled = True
        Button2.Enabled = True
        startAsyncButton.Enabled = True
        cancelAsyncButton.Enabled = False

        sender = Nothing
        e = Nothing


    End Sub 'backgroundWorker1_RunWorkerCompleted

    Private Sub backgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged


        Me.ProgressBar1.Value = e.ProgressPercentage
        'If lastinputline.StartsWith("Operation Completed") Then
        'Me.ToolStripStatusLabel1.Text = lastinputline
        'Else
        Me.ToolStripStatusLabel1.Text = lastinputline & "   (" & inputlines & " of " & inputlinesprecount & ")"
        'End If


        sender = Nothing
        e = Nothing
    End Sub

    Function MainWorkerFunction(ByVal worker As BackgroundWorker, ByVal e As DoWorkEventArgs) As Boolean
        Dim result As Boolean = False
        Try
            If Me.pretestdone = False Then
                primary_PercentComplete = 0
                worker.ReportProgress(0)
                PreCount_Function(worker)
                Me.pretestdone = True
            End If

            If worker.CancellationPending Then
                e.Cancel = True
                Return False
            End If

            primary_PercentComplete = 0
            worker.ReportProgress(0)

            inputlines = 0
            lastinputline = ""


            Dim reader As StreamReader = New StreamReader(TextBox1.Text)
            If reader.Peek <> -1 Then
                Dim lineread As String

                Dim CSFirstname As String = ""
                Dim CSLastname As String = ""
                Dim CSTitle As String = ""
                Dim CSQualifications As String = ""
                Dim CSPosition As String = ""
                Dim CSExtraText As String = ""
                Dim CSEmail As String = ""
                Dim CSPhone As String = ""
                Dim CSFax As String = ""
                Dim CSOffice As String = ""
                Dim CSPersonalPage As String = ""
                Dim CSDepartment As String = ""
                Dim CSDepartmentSection As String = ""
                Dim CSRanking As String = "5"
                Dim CSActive As String = "Yes"

                If reader.ReadLine().Trim = "--" Then
                    While reader.Peek <> -1

                        If worker.CancellationPending Then
                            e.Cancel = True
                            Exit While
                            Return False
                        End If
                        lineread = reader.ReadLine().Trim
                        Try
                            If lineread = "--" Then

                                WorkerExecuteNonQuery("Insert into Commerce_Staff (CSFirstname, CSLastname, CSTitle, CSQualifications, CSPosition, CSExtraText, CSEmail, CSPhone, CSFax, CSOffice, CSPersonalPage, CSDepartment, CSDepartmentSection, CSRanking, CSActive) values ('" & CSFirstname & "', '" & CSLastname & "', '" & CSTitle & "', '" & CSQualifications & "', '" & CSPosition & "', '" & CSExtraText & "', '" & CSEmail & "', '" & CSPhone & "', '" & CSFax & "', '" & CSOffice & "', '" & CSPersonalPage & "', '" & CSDepartment & "', '" & CSDepartmentSection & "', " & CSRanking & ", '" & CSActive & "')", 1)

                                CSFirstname = ""
                                CSLastname = ""
                                CSTitle = ""
                                CSQualifications = ""
                                CSPosition = ""
                                CSExtraText = ""
                                CSEmail = ""
                                CSPhone = ""
                                CSFax = ""
                                CSOffice = ""
                                CSPersonalPage = ""
                                CSDepartment = ""
                                CSDepartmentSection = ""
                                CSRanking = "5"
                                CSActive = "Yes"

                                inputlines = inputlines + 1
                                lastinputline = "Processed: " & CSLastname
                                ' Report progress as a percentage of the total task.
                                percentComplete = 0
                                If inputlinesprecount > 0 Then
                                    percentComplete = CSng(inputlines) / CSng(inputlinesprecount) * 100
                                Else
                                    percentComplete = 100
                                End If
                                primary_PercentComplete = percentComplete
                                If percentComplete > 100 Then
                                    percentComplete = 100
                                End If
                                If percentComplete = 100 Then
                                    lastinputline = "Operation Completed"
                                End If
                                If percentComplete > highestPercentageReached Then
                                    highestPercentageReached = percentComplete
                                    worker.ReportProgress(percentComplete)
                                End If
                                If worker.CancellationPending Then
                                    e.Cancel = True
                                    Exit While
                                    Return False
                                End If
                            Else
                                Select Case lineread.Substring(0, lineread.IndexOf("=", 0))
                                    Case "CSFirstname"
                                        CSFirstname = lineread.Replace("CSFirstname=", "").Trim
                                    Case "CSLastname"
                                        CSLastname = lineread.Replace("CSLastname=", "").Trim
                                    Case "CSTitle"
                                        CSTitle = lineread.Replace("CSTitle=", "").Trim
                                    Case "CSQualifications"
                                        CSQualifications = lineread.Replace("CSQualifications=", "").Trim
                                    Case "CSPosition"
                                        CSPosition = lineread.Replace("CSPosition=", "").Trim
                                    Case "CSExtraText"
                                        CSExtraText = lineread.Replace("CSExtraText=", "").Trim
                                    Case "CSEmail"
                                        CSEmail = lineread.Replace("CSEmail=", "").Trim
                                    Case "CSPhone"
                                        CSPhone = lineread.Replace("CSPhone=", "").Trim
                                    Case "CSFax"
                                        CSFax = lineread.Replace("CSFax=", "").Trim
                                    Case "CSOffice"
                                        CSOffice = lineread.Replace("CSOffice=", "").Trim
                                    Case "CSPersonalPage"
                                        CSPersonalPage = lineread.Replace("CSPersonalPage=", "").Trim
                                    Case "CSDepartment"
                                        CSDepartment = lineread.Replace("CSDepartment=", "").Trim
                                    Case "CSDepartmentSection"
                                        CSDepartmentSection = lineread.Replace("CSDepartmentSection=", "").Trim
                                    Case "CSRanking"
                                        CSRanking = lineread.Replace("CSRanking=", "").Trim
                                    Case "CSActive"
                                        CSActive = lineread.Replace("CSActive=", "").Trim
                                End Select
                            End If
                        Catch ex As Exception
                            Error_Handler(ex, "Parsing Input File")
                        End Try
                    End While
                Else
                    AlertMessage = AlertMessage & TextBox1.Text & vbCrLf
                End If
            End If
            reader.Close()
            reader = Nothing

        Catch ex As Exception
            Error_Handler(ex, "MainWorkerFunction")
        End Try
        worker.Dispose()
        worker = Nothing
        e = Nothing
        Return result

    End Function

    Private Sub Form1_Close(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            Me.ToolStripStatusLabel1.Text = "Application Closing"
            SaveSettings()
        Catch ex As Exception
            Error_Handler(ex, "Application Close")
        End Try
    End Sub

    Private Sub LoadSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")
            If My.Computer.FileSystem.FileExists(configfile) Then
                Dim reader As StreamReader = New StreamReader(configfile)
                Dim lineread As String
                Dim variablevalue As String
                While reader.Peek <> -1
                    lineread = reader.ReadLine
                    If lineread.IndexOf("=") <> -1 Then

                        variablevalue = lineread.Remove(0, lineread.IndexOf("=") + 1)

                        If lineread.StartsWith("SelectFile=") Then
                            Dim dinfo As FileInfo = New FileInfo(variablevalue)
                            If dinfo.Exists Then
                                OpenFileDialog2.FileName = variablevalue
                                TextBox1.Text = variablevalue
                            End If
                            dinfo = Nothing
                        End If
                        If lineread.StartsWith("SelectDatabase=") Then
                            Dim dinfo As FileInfo = New FileInfo(variablevalue)
                            If dinfo.Exists Then
                                OpenFileDialog1.FileName = variablevalue
                                TextBox2.Text = variablevalue
                            End If
                            dinfo = Nothing
                        End If

                        'If lineread.StartsWith("SetVariable=") Then
                        '    ComboBox1.SelectedIndex = variablevalue
                        'End If

                        'If lineread.StartsWith("PixelValue=") Then
                        '    NumericUpDown2.Value = variablevalue
                        'End If

                    End If
                End While
                reader.Close()
                reader = Nothing
            End If
        Catch ex As Exception
            Error_Handler(ex, "Load Settings")
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")

            Dim writer As StreamWriter = New StreamWriter(configfile, False)

            If TextBox1.Text.Length > 0 Then
                Dim dinfo As FileInfo = New FileInfo(TextBox1.Text)
                If dinfo.Exists Then
                    writer.WriteLine("SelectFile=" & TextBox1.Text)
                End If
                dinfo = Nothing
            End If
            If TextBox2.Text.Length > 0 Then
                Dim dinfo As FileInfo = New FileInfo(TextBox2.Text)
                If dinfo.Exists Then
                    writer.WriteLine("SelectDatabase=" & TextBox2.Text)
                End If
                dinfo = Nothing
            End If
            'If ComboBox1.SelectedIndex <> -1 Then
            '    writer.WriteLine("SetVariable=" & ComboBox1.SelectedIndex)
            'End If

            'writer.WriteLine("PixelValue=" & NumericUpDown2.Value)

            writer.Flush()
            writer.Close()
            writer = Nothing

        Catch ex As Exception
            Error_Handler(ex, "Save Settings")
        End Try
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Me.Text = My.Application.Info.ProductName & " " & Format(My.Application.Info.Version.Major, "0000") & Format(My.Application.Info.Version.Minor, "00") & Format(My.Application.Info.Version.Build, "00") & "." & Format(My.Application.Info.Version.Revision, "00") & ""
            LoadSettings()
            Me.ToolStripStatusLabel1.Text = "Application Loaded"
        Catch ex As Exception
            Error_Handler(ex, "Application Load")
        End Try

    End Sub



    

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "About displayed"
            AboutBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display About Screen")
        End Try
    End Sub

    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "Help displayed"
            HelpBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display Help Screen")
        End Try
    End Sub

    Private Sub TextBox1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragDrop
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim MyFiles() As String
                Dim i As Integer

                ' Assign the files to an array.
                MyFiles = e.Data.GetData(DataFormats.FileDrop)
                ' Loop through the array and add the files to the list.
                'For i = 0 To MyFiles.Length - 1
                If MyFiles.Length > 0 Then
                    Dim finfo As FileInfo = New FileInfo(MyFiles(0))
                    If finfo.Exists = True Then
                        TextBox1.Text = (MyFiles(0))
                        OpenFileDialog2.FileName = (MyFiles(0))
                    End If
                End If
                'Next
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox1_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragEnter
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox2_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragDrop
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim MyFiles() As String
                Dim i As Integer

                ' Assign the files to an array.
                MyFiles = e.Data.GetData(DataFormats.FileDrop)
                ' Loop through the array and add the files to the list.
                'For i = 0 To MyFiles.Length - 1
                If MyFiles.Length > 0 Then
                    Dim finfo As FileInfo = New FileInfo(MyFiles(0))
                    If finfo.Exists = True Then
                        TextBox2.Text = (MyFiles(0))
                        OpenFileDialog1.FileName = (MyFiles(0))
                    End If
                End If
                'Next
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox2_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragEnter
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub


    Protected Friend Function Get_Connection(ByVal dbselect As Integer) As OleDb.OleDbConnection
        'Standard(Security)
        '"Provider=sqloledb;Data Source=Aron1;Initial Catalog=pubs;User Id=sa;Password=asdasd;" 

        'Trusted(Connection)
        '"Provider=sqloledb;Data Source=Aron1;Initial Catalog=pubs;Integrated Security=SSPI;" 
        '(use serverName\instanceName as Data Source to use an specifik SQLServer instance, only SQLServer2000)

        'Prompt for username and password:
        'oConn.Provider = "sqloledb"
        'oConn.Properties("Prompt") = adPromptAlways
        'oConn.Open("Data Source=Aron1;Initial Catalog=pubs;")

        'Connect via an IP address:
        '"Provider=sqloledb;Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=pubs;User ID=sa;Password=asdasd;" 
        '(DBMSSOCN=TCP/IP instead of Named Pipes, at the end of the Data Source is the port to use (1433 is the default))

        Dim connection_string As String
        'If dbserver.IndexOf(".") = -1 Then
        'connection_string = "Provider=sqloledb;Data Source=" & dbserver & ";Initial Catalog=" & dbtable & ";User Id=" & dbuser & ";Password=" & dbpassword & ";"
        'Else
        'connection_string = "Provider=sqloledb;Data Source=" & dbserver & ",1433;Network Library=DBMSSOCN;Initial Catalog=" & dbtable & ";User Id=" & dbuser & ";Password=" & dbpassword & ";"
        'End If
        'Dim connection_string As String = "User ID=" & dbuser & ";password=" & dbpassword & ";Data Source=" & dbserver & ";Tag with column collation when possible=False;Initial Catalog=" & dbtable & ";Use Procedure for Prepare=1;Auto Translate=True;Persist Security Info=False;Provider=""SQLOLEDB.1"";Use Encryption for Data=False;Packet Size=4096"
        Select Case dbselect
            Case 1
                connection_string = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=""" & TextBox2.Text & """"
            Case 2
                connection_string = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=""" & TextBox2.Text & """"
        End Select

        Dim conn As OleDb.OleDbConnection = New OleDb.OleDbConnection(connection_string)
        Return conn
    End Function



    Public Function WorkerExecuteNonQuery(ByVal SQLstatement As String, ByVal dbselect As Integer) As String
        Dim result As String
        Try

            Dim conn As OleDb.OleDbConnection
            Try
                conn = Get_Connection(dbselect)
                conn.Open()

                Dim sql As OleDb.OleDbCommand
                sql = New OleDb.OleDbCommand
                sql.CommandText = SQLstatement
                sql.Connection = conn
                result = sql.ExecuteNonQuery().ToString & " SQL Statement Succeeded"
                sql.Dispose()

            Catch ex As Exception
                Error_Handler(ex)
                result = "0 SQL Statement Failed"
            Finally
                Try
                    conn.Close()
                Catch ex1 As Exception
                    Error_Handler(ex1)
                End Try
                conn.Dispose()
            End Try
        Catch ex As Exception
            Error_Handler(ex)
            result = "0 SQL Statement Failed"


        End Try
        Return result
    End Function



    Public Function WorkerExecuteSpecialScalar(ByVal SQLstatement As String, ByVal dbselect As Integer) As String
        Dim result As String
        Try

            Dim conn As OleDb.OleDbConnection
            Try
                conn = Get_Connection(dbselect)

                conn.Open()



                Dim sql As OleDb.OleDbCommand = New OleDb.OleDbCommand
                sql.CommandText = SQLstatement
                sql.Connection = conn

                result = sql.ExecuteScalar().ToString
                sql.Dispose()

            Catch ex As Exception
                Error_Handler(ex)
                result = "0 SQL Statement Failed"
            Finally
                Try
                    conn.Close()
                Catch ex1 As Exception
                    Error_Handler(ex1)
                End Try
                conn.Dispose()
            End Try
        Catch ex As Exception
            Error_Handler(ex)
            result = "0 SQL Statement Failed"


        End Try
        Return result
    End Function

    Private Function StripHTMLTags(ByVal input As String) As String
        Dim output As String = ""
        Try
            Dim firstopen, firstclose As Integer
            output = input
            output = output.Replace("&nbsp;", " ")
            While output.IndexOf("<") <> -1 Or output.IndexOf(">") <> -1
                firstopen = -1
                firstclose = -1
                firstopen = output.IndexOf("<")
                firstclose = output.IndexOf(">")
                If firstopen <> -1 And firstclose <> -1 Then
                    If firstopen < firstclose Then
                        output = output.Remove(firstopen, firstclose - firstopen + 1)
                    Else
                        output = output.Remove(0, firstclose + 1)
                    End If
                End If
                If firstopen = -1 And firstclose = -1 Then
                    output = output
                End If
                If firstopen <> -1 And firstclose = -1 Then
                    output = output.Remove(firstopen, output.Length - firstopen)
                End If
                If firstopen = -1 And firstclose <> -1 Then
                    output = output.Remove(0, firstclose + 1)
                End If
            End While
        Catch ex As Exception
            Error_Handler(ex, "Strip HTML Tags (Error on '" & output & "')")
        End Try
        Return output
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim result As DialogResult
        result = OpenFileDialog1.ShowDialog
        If result = Windows.Forms.DialogResult.OK Then
            TextBox2.Text = OpenFileDialog1.FileName
        End If
    End Sub
End Class
