Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports Neurotec.Biometrics.Client
Imports Neurotec.Images
Imports Neurotec.Licensing


Imports Neurotec.Biometrics
Imports Neurotec.Devices
Imports System.IO

Public Class dlgFaceRegistration
    Public FaceManager As Manager.FaceRecognition
    Public Faces As IInterface.IFace
    Public fs1 As NSubject
    Public fs2 As NSubject
    Public fs3 As NSubject
    Public res As DialogResult
    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        DialogResult = DialogResult.None
    End Sub

    Private WriteOnly Property availableFaceView As NSubject
        Set(value As NSubject)
            If fv1.Face Is Nothing Then
                fs1 = value
                fv1.Face = fs1.Faces(0)
            End If
        End Set
    End Property

    Private Sub dlgFaceRegistration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '  fv1.Face.Image = NImage.FromMemory(User.FaceImage_1.GetTemplateBuffer)

        StartRegister()
    End Sub

    Private Sub dlgFaceRegistration_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not closeForm Then
            closeForm = True
            stopScanning = True
            FaceManager.ForceCapture()
            tmClosingAttempt.Enabled = True
            e.Cancel = True
        Else : FaceManager.StopProcess()
        End If
    End Sub

    Private Sub btnCapture_Click(sender As Object, e As EventArgs) Handles btnCapture.Click
        FaceManager.ForceCapture()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnRestart.Click
        fv1.Face = Nothing
        stopScanning = True

        FaceManager.ForceCapture()
        StartRegister()
    End Sub

    Private Async Sub StartRegister()
        Try
            Dim firstCapture As Boolean = True
            While True
                Dim f = Await FaceManager.CaptureAsync(fvStream, IIf(firstCapture, NBiometricCaptureOptions.Manual, NBiometricCaptureOptions.Stream))
                firstCapture = False
                If Not stopScanning Then
                    Dim s As NSubject = f(0)
                    Dim status = f(1)
                    If status = NBiometricStatus.Ok Then
                        availableFaceView = s
                        If fv1.Face IsNot Nothing Then
                            res = DialogResult.OK
                            Close()
                            Exit While
                        End If
                    Else ': MsgBox("Please Try Again.")
                    End If
                Else : Exit While
                End If
            End While
        Catch ex As Exception
            Utils.ShowException(ex)
        End Try
    End Sub

    Private stopScanning As Boolean
    Private closeForm As Boolean
    Private Sub tmClosingAttempt_Tick(sender As Object, e As EventArgs) Handles tmClosingAttempt.Tick
        tmClosingAttempt.Enabled = False
        Me.Close()
    End Sub
End Class