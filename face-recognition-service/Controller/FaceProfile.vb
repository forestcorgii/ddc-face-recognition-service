Imports System.Windows.Forms
Imports Neurotec.Biometrics
Imports Neurotec.Licensing

Namespace Controller
    Public Class FaceProfile
        Public Shared Sub licenseSetup(Optional useTrial As Boolean = False)
            For Each lic As String In Utils.GetLicenses
                NLicense.Add(lic)
            Next

            Const Components As String = "Biometrics.FaceExtraction,Biometrics.FaceMatching,Biometrics.FaceDetection,Devices.Cameras" ',Biometrics.FaceSegmentsDetection"
            For Each component As String In Components.Split(New Char() {","}, StringSplitOptions.RemoveEmptyEntries)
                If NLicense.ObtainComponents("/local", "5000", component.Trim) = False Then
                    MessageBox.Show(component & " Component License is not activated", Nothing, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
            Next
        End Sub

        Public Shared Function CollectFaceSubjects(faces As Object, useSingleImage As Boolean) As List(Of NSubject)
            ' Create subjects from selected templates
            Dim faceSubjects As New List(Of NSubject)
            Dim addedUser As New List(Of String)
            For i As Integer = 0 To faces.Count - 1
                With faces(i)
                    If Not addedUser.Contains(.Id) Then
                        addedUser.Add(.Id)
                        If .Active AndAlso .FaceImage_1 IsNot Nothing Then
                            .FaceImage_1.Id = .Id & "_1"
                            faceSubjects.Add(.FaceImage_1)
                        End If
                    End If
                End With
            Next i

            Return faceSubjects
        End Function
        Public Shared Function EditFaceTemplate(verilookManager As Manager.FaceRecognition, ByRef Faces As IInterface.IFace) As Boolean

            Using freg As New dlgFaceRegistration With {.FaceManager = verilookManager, .Faces = Faces}
                freg.ShowDialog()
                With Faces
                    If freg.fs1 IsNot Nothing Then
                        .face_data1 = freg.fs1.GetTemplateBuffer.ToArray
                    End If
                End With
            End Using
            Return True
        End Function


    End Class

End Namespace
