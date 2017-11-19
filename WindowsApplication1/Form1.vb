Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Public Class Form1
    Dim serverSocket, clientSocket As Socket
    Dim ic As ListenClient
    Dim acceptThread As Thread
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim hostName As String
        hostName = Dns.GetHostName

        Dim listIP() As IPAddress
        listIP = Dns.GetHostEntry(hostName).AddressList
        Dim i As Integer
        Dim idx As Integer
        For i = 0 To listIP.Length - 1
            ComboBox1.Items.Add(listIP(i))
            If listIP(i).ToString.IndexOf(".") <> -1 Then
                idx = i
            End If
        Next
        ComboBox1.SelectedIndex = idx
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            serverSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Dim serverIP As IPAddress = IPAddress.Parse(ComboBox1.Text)
            Dim serverhost As New IPEndPoint(serverIP, TextBox1.Text)
            serverSocket.Bind(serverhost)
            serverSocket.Listen(10)

            Console.WriteLine("Server is Listening")
            'clientSocket = serverSocket.Accept()
            'Console.WriteLine(clientSocket.RemoteEndPoint.ToString & "is connected.")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK)
        End Try
       
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            'clientSocket = serverSocket.Accept()
            'Console.WriteLine(clientSocket.RemoteEndPoint.ToString & "is connected.")
            ic = New ListenClient(serverSocket, Me)
            acceptThread = New Thread(AddressOf ic.ServerThreadProc)
            acceptThread.Start()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    'Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
    '    Dim bytes(1024) As Byte
    '    Dim i As Integer = clientSocket.Receive(bytes, 0, clientSocket.Available, SocketFlags.None)
    '    TextBox2.Text = Encoding.Default.GetString(bytes, 0, i)
    'End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ic.clientSocket.Shutdown(SocketShutdown.Both)
        ic.clientSocket.Close()
        serverSocket.Close()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim msg As Byte() = Encoding.Default.GetBytes("Mesg" & TextBox2.Text)
        ic.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None)
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not ic Is Nothing Then
            If Not ic.clientSocket Is Nothing Then
                ic.clientSocket.Shutdown(SocketShutdown.Both)
                ic.clientSocket.Close()
            End If
        End If
        If Not serverSocket Is Nothing Then
            serverSocket.Close()
        End If
        If Not serverSocket Is Nothing Then
            acceptThread.Abort()
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim preBuffer() As Byte = Encoding.Default.GetBytes("File")
            Dim postBuffer() As Byte = Encoding.Default.GetBytes("####")
            ic.clientSocket.SendFile(OpenFileDialog1.FileName, preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread)
        End If
    End Sub

    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox2.KeyDown
        Select Case e.KeyCode
            Case Keys.Left
                Dim msg As Byte() = Encoding.Default.GetBytes("Left")
                ic.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None)
            Case Keys.Right
                Dim msg As Byte() = Encoding.Default.GetBytes("Rigt")
                ic.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None)
            Case Keys.Up
                Dim msg As Byte() = Encoding.Default.GetBytes("Up  ")
                ic.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None)
            Case Keys.Down
                Dim msg As Byte() = Encoding.Default.GetBytes("Down")
                ic.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None)
        End Select
    End Sub
End Class
Class ListenClient
    Private serverSocket As Socket
    Public clientSocket As Socket
    Private f1 As Form1
    Delegate Sub setTextDel(ByVal tmpStr As String)
    Public Sub New(ByVal tmpSocket As Socket, ByVal tmpFrom1 As Form1)
        serverSocket = tmpSocket
        f1 = tmpFrom1
    End Sub
    Public Sub ServerThreadProc()
        Do While True
            Try
                clientSocket = serverSocket.Accept
                Dim t As New Thread(AddressOf receiveThreadProc)
                t.Start()
                Console.WriteLine(clientSocket.RemoteEndPoint.ToString & " is connected ")
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
            End Try
        Loop
    End Sub
    Private Sub receiveThreadProc()
        Try
            Dim bytes(1024) As Byte
            Dim rcvBytes As Integer
            Dim tmpStr As String
            Do
                rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None)
                tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes)
                setText(tmpStr)
            Loop While clientSocket.Available <> 0 Or rcvBytes <> 0
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub
    Private Sub setText(ByVal tmpStr As String)
        If f1.TextBox2.InvokeRequired = True Then
            Dim d As New setTextDel(AddressOf setText)
            f1.Invoke(d, tmpStr)
        Else
            f1.TextBox2.Text = tmpStr
        End If

    End Sub
End Class
