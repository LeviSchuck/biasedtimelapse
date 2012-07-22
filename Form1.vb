Imports System.IO
Imports System.Drawing.Imaging
Imports System.Threading.Tasks

Public Class Form1
    Dim folderpath As String = ""
    Dim framenum As Integer = 0
    Dim framebool As Boolean = False
    Dim frameA As Bitmap = Nothing
    Dim frameB As Bitmap = Nothing
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Application.Exit()
        End If
        folderpath = FolderBrowserDialog1.SelectedPath
        While True
            If Not File.Exists(getFileName()) Then
                Exit While
            End If
            framenum += 1
        End While
        Label1.Text = "Not started."
    End Sub
    Private Function getFileName() As String
        Return folderpath & "\screen_" & framenum.ToString().PadLeft(6, "0") & ".png"
    End Function
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Button1.Enabled = False
        'BackgroundWorker1_DoWork(Nothing, Nothing)
        BackgroundWorker1.RunWorkerAsync()
    End Sub
    Delegate Sub updateLabelCallback()
    Private Sub updateLabel()
        If Label1.InvokeRequired Then
            Label1.Invoke(New updateLabelCallback(AddressOf updateLabel), New Object() {})
            Return
        End If
        Label1.Text = "Frame screen_" & framenum.ToString().PadLeft(6, "0")
    End Sub
    Private Sub takePicture(ByRef frame As Bitmap)
        frame = New Bitmap(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height)
        Dim g As Graphics = Graphics.FromImage(frame)
        g.CopyFromScreen(0, 0, 0, 0, frame.Size)
        g.Dispose()
    End Sub
    Private Sub savePicture(ByRef frame As Bitmap, ByVal location As String)
        frame.Save(location, ImageFormat.Png)
    End Sub
    Private Function diffBitmaps(ByRef frameA As Bitmap, ByRef frameB As Bitmap) As Integer
        Dim result As Integer = 0
        'Parallel.For(0, Screen.PrimaryScreen.WorkingArea.Width - 1,
        '             Sub(x)
        '             End Sub)
        Dim bmpAData As System.Drawing.Imaging.BitmapData = frameA.LockBits(New Rectangle(0, 0, frameA.PhysicalDimension.Width, frameA.PhysicalDimension.Height), _
            Drawing.Imaging.ImageLockMode.ReadOnly, _
            Imaging.PixelFormat.Format24bppRgb)
        ' Get the address of the first line
        Dim ptrA As IntPtr = bmpAData.Scan0
        ' Declare an array to hold the bytes of the bitmap
        Dim bytesA As Integer = Math.Abs(bmpAData.Stride) * frameA.PhysicalDimension.Height
        Dim rgbValuesA(bytesA - 1) As Byte
        System.Runtime.InteropServices.Marshal.Copy(ptrA, rgbValuesA, 0, bytesA)
        Dim bmpBData As System.Drawing.Imaging.BitmapData = frameB.LockBits(New Rectangle(0, 0, frameB.PhysicalDimension.Width, frameB.PhysicalDimension.Height), _
           Drawing.Imaging.ImageLockMode.ReadOnly, _
           Imaging.PixelFormat.Format24bppRgb)
        ' Get the address of the first line
        Dim ptrB As IntPtr = bmpBData.Scan0
        ' Declare an array to hold the bytes of the bitmap
        Dim bytesB As Integer = Math.Abs(bmpBData.Stride) * frameB.PhysicalDimension.Height
        Dim rgbValuesB(bytesB - 1) As Byte
        System.Runtime.InteropServices.Marshal.Copy(ptrB, rgbValuesB, 0, bytesB)
        Dim l As Integer = 0
        For x As Integer = 0 To Screen.PrimaryScreen.WorkingArea.Width - 1
            For y As Integer = 0 To Screen.PrimaryScreen.WorkingArea.Height - 1
                If Math.Abs(CInt(rgbValuesA(l)) - CInt(rgbValuesB(l))) > 20 Or Math.Abs(CInt(rgbValuesA(l + 1)) - CInt(rgbValuesB(l + 1))) > 20 Or Math.Abs(CInt(rgbValuesA(l + 2)) - CInt(rgbValuesB(l + 2))) > 20 Then
                    result += 1
                End If
                l += 3
            Next
        Next
        frameA.UnlockBits(bmpAData)
        frameB.UnlockBits(bmpBData)
        'For x As Integer = 0 To Screen.PrimaryScreen.WorkingArea.Width - 1
        '    For y As Integer = 0 To Screen.PrimaryScreen.WorkingArea.Height - 1
        '        Dim pixA = frameA.GetPixel(x, y)
        '        Dim pixB = frameB.GetPixel(x, y)
        '        If Math.Abs(CInt(pixA.R) - CInt(pixB.R)) > 20 Or Math.Abs(CInt(pixA.G) - CInt(pixB.G)) > 20 Or Math.Abs(CInt(pixA.B) - CInt(pixB.B)) > 20 Then
        '            result += 1
        '        End If
        '    Next
        'Next

        Return result
    End Function
    Const threshold = 200
    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        While True


            If Not framebool Then
                takePicture(frameA)
            Else
                takePicture(frameB)
            End If
            If frameA Is Nothing Or frameB Is Nothing Then
                'It doesn't matter, we should store it.
                If Not framebool Then
                    savePicture(frameA, getFileName())
                Else
                    savePicture(frameB, getFileName())
                End If
                framebool = Not framebool
                framenum += 1
            Else
                If diffBitmaps(frameA, frameB) > threshold Then
                    framebool = Not framebool
                    If Not framebool Then
                        savePicture(frameA, getFileName())
                    Else
                        savePicture(frameB, getFileName())
                    End If
                    framenum += 1
                End If
            End If

            updateLabel()
            Threading.Thread.Sleep(150)
            GC.Collect()
        End While
    End Sub
End Class
