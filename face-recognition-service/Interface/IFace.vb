Imports Neurotec.Biometrics

Namespace IInterface

    Public Interface IFace
        Property Id As String
        Property Active As Boolean
        Property Admin As Boolean


        Property face_data1 As Byte()
        Property FaceImage_1 As NSubject
    End Interface
End Namespace
