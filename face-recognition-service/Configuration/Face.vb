Imports Neurotec.Biometrics
Imports Neurotec.Biometrics.Client

Namespace Configuration
    Public Class Face
        Public SingleImageRecognition As Boolean
        Public UserReloginTime As Double

        Public MaximalYaw As Single
        Public MaximalRoll As Single
        Public MatchingScoreThreshold As Single
        Public ConfidenceThreshold As Single
        Public QualityThreshold As Single
        Public MatchingSpeed As NMatchingSpeed
        Public TemplateSize As NTemplateSize

        Public UseDeviceManager As Boolean

        Public UseLiveness As Boolean
        Public DetectFacialFeature As Boolean
        Public LivenessThreshold As Single
        Public LivenessBlinkThreshold As Single
        Public LivenessMode As NLivenessMode

        Private _ConfFilename As String
        Public Property ConfFilename As String
            Get
                Return _ConfFilename
            End Get
            Set(value As String)
                _ConfFilename = value
            End Set
        End Property
        Sub New()
        End Sub

        Sub New(_fPath As String)
            ConfFilename = _fPath
        End Sub

        Public Sub ConfigureEngine(ByRef eng As NBiometricClient)
            With eng
                .FacesMaximalRoll = MaximalRoll
                .FacesMaximalYaw = MaximalYaw

                .FacesMatchingSpeed = MatchingSpeed
                .FacesTemplateSize = TemplateSize

                .FacesLivenessMode = LivenessMode
                .FacesLivenessThreshold = LivenessThreshold
                .FacesLivenessBlinkTimeout = LivenessBlinkThreshold

                .FacesQualityThreshold = QualityThreshold
                .FacesConfidenceThreshold = ConfidenceThreshold

                .UseDeviceManager = UseDeviceManager
                .FacesDetectAllFeaturePoints = DetectFacialFeature
            End With
        End Sub


    End Class

End Namespace
