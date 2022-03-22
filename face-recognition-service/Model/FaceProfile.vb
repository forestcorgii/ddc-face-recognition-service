Imports Neurotec.Biometrics
Imports face_recognition_service.IInterface

Namespace Model
    Public Class FaceProfile
        Implements IFace
        Public Property Id As String Implements IFace.Id
        Public Property Active As Boolean Implements IFace.Active
        Public Property Admin As Boolean Implements IFace.Admin

        Public Property face_data1 As Byte() Implements IFace.face_data1
        Public Property FaceImage_1 As NSubject Implements IFace.FaceImage_1
    End Class

End Namespace
