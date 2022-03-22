Imports System
Imports System.Windows.Forms
Imports Neurotec.Biometrics.Client
Imports Neurotec.Images


Imports Neurotec.Biometrics
Imports Neurotec.Devices
Imports System.IO
Namespace Manager

    Public Class FaceRecognition
        Public Event FaceIdentified(sender As Object, e As FaceRecognizeEventArgs)

#Region "Saved Fields"
        Public Settings As Configuration.Face
#End Region

#Region "Ignored Fields"
        Public EmployeeFaceSubjects As List(Of NSubject)
        Public ConfFilename As String
        Public OpeningCamera As Boolean


        Private _biometricClient As NBiometricClient
        Private _biometricClient2 As NBiometricClient
        Private _isSegmentationActivated? As Boolean

        Private CameraSubject1 As NSubject

        Public ProcessType As FaceProcessType

        Private EnrollmentTask As NBiometricTask

#End Region


        Public CameraList As List(Of NDevice)

        Public ReadOnly Property Runnable As Boolean
            Get
                Return EmployeeFaceSubjects IsNot Nothing
            End Get
        End Property

        Public Sub Setup()
            Controller.FaceProfile.licenseSetup()

            Try
                _biometricClient = New NBiometricClient With {.BiometricTypes = NBiometricType.Face}
                Settings.ConfigureEngine(_biometricClient)

                _biometricClient2 = New NBiometricClient() With {.BiometricTypes = NBiometricType.Face, .UseDeviceManager = False, .FacesMatchingSpeed = NMatchingSpeed.High}
            Catch ex As Exception
                Utils.ShowException(ex)
            End Try
        End Sub

        Private Sub setCamera(ByRef _subject As NSubject, fv As Gui.NFaceView, Optional captureOptions As NBiometricCaptureOptions = NBiometricCaptureOptions.Manual)
            OpeningCamera = True
            Dim face As New NFace() With {.CaptureOptions = captureOptions}
            _subject = New NSubject()
            _subject.Faces.Add(face)
            fv.Face = face
        End Sub

        Public Sub FillBiometricTask()
            EnrollmentTask = New NBiometricTask(NBiometricOperations.Enroll)
            EmployeeFaceSubjects.ForEach(Sub(item As NSubject)
                                             EnrollmentTask.Subjects.Add(item)
                                         End Sub)
        End Sub

#Region "Methods for Recognition"
        Public Async Function StartProcess(_parentform As Form, fv As Gui.NFaceView, _processType As FaceProcessType, Optional useSecond As Boolean = False) As System.Threading.Tasks.Task ', Optional firstCapture As Boolean = False) As System.Threading.Tasks.Task
            ProcessType = _processType

            setupTimer1(True)
            Settings.ConfigureEngine(_biometricClient)
            setCamera(CameraSubject1, fv, NBiometricCaptureOptions.Manual)
            Await _biometricClient.CaptureAsync(CameraSubject1)
        End Function

        Public Async Sub StopProcess() 'As Task(Of Boolean)
            If Camera1InUse Then
                setupTimer1(False)
                Await _biometricClient.ClearAsync()
                CameraSubject1.Faces(0).Image = Nothing
            End If
        End Sub

        Public Sub ForceCapture()
            _biometricClient.ForceStart()
        End Sub

        Public Async Function CaptureAsync(fv As Gui.NFaceView, Optional captureOptions As NBiometricCaptureOptions = NBiometricCaptureOptions.Manual) As Task(Of Object())
            Dim s As New NSubject
            setCamera(s, fv, captureOptions)
            Dim status = Await _biometricClient.CaptureAsync(s)
            Return {s, status}
        End Function

        Public Async Function DetectFacesAsync(ByVal image As NImage, _processType As FaceProcessType) As Task(Of NFace()) 'System.Threading.Tasks.Task
            If image IsNot Nothing Then
                OpeningCamera = False
                Try
                    Dim faces As New List(Of NFace)
                    Dim face = Await _biometricClient.DetectFacesAsync(image)
                    For i As Integer = 0 To face.Objects.Count - 1
                        Dim nla As New NLAttributes With {.BoundingRect = face.Objects(i).BoundingRect}
                        Dim f = NFace.FromImageAndAttributes(face.Image, nla)
                        faces.Add(f)
                    Next
                    If faces.Count > 0 Then
                        If _processType = FaceProcessType.Identify Then
                            IdentifyFaces(faces.ToArray, image)
                        Else : Return faces.ToArray
                        End If
                    End If
                    ' Return Nothing
                Catch ex As Exception
                    Utils.ShowException(ex)
                End Try
            End If
            Return Nothing
        End Function

        Private Async Sub IdentifyFaces(faces As NFace(), image As NImage)
            For i As Integer = 0 To faces.Length - 1
                Await subjectWorker_Worker_DoWork({faces(i), faces.Length})
            Next
        End Sub

#End Region

#Region "Workers Methods"

        Private Async Function subjectWorker_Worker_DoWork(arg As Object) As Task
            Try
                Dim s As New NSubject, msg As String = ""
                Dim _id As String = "", score As String = "", st As NBiometricStatus

                s.Faces.Add(arg(0))
                If s IsNot Nothing Then
                    Await _biometricClient2.ClearAsync()

                    Dim taskAsync = Await _biometricClient2.PerformTaskAsync(EnrollmentTask)
                    If taskAsync.Status = NBiometricStatus.Ok Then
                        Dim identifyTask = Await _biometricClient2.IdentifyAsync(s)
                        If identifyTask = NBiometricStatus.Ok OrElse identifyTask = NBiometricStatus.MatchNotFound Then
                            Dim m = (From res In s.MatchingResults Where res.Score >= Settings.MatchingScoreThreshold Order By res.Score Descending Take 1 Select {res.Id, res.Score}).FirstOrDefault
                            If m IsNot Nothing Then
                                _id = m(0) : score = m(1)
                                msg = String.Format("ID: {0}, Score: {1}", _id, score)
                            Else : msg = String.Format("Identification failed: {0}", identifyTask)
                                Exit Function
                            End If
                        Else : msg = String.Format("Identification failed: {0}", identifyTask)
                            Exit Function
                        End If
                        st = identifyTask
                    Else : st = taskAsync.Status
                        msg = String.Format("Enrollment failed: {0}", taskAsync.Status)
                        Exit Function
                    End If
                End If

                Dim args As New FaceRecognizeEventArgs
                With args
                    .Subject = s
                    .UserID = _id
                    .Score = score
                    .Message = msg
                    .Status = st
                    .ElapseTime = (Now - stTime)
                    .Faces = arg(1)
                    .RawMatches = (From res In s.MatchingResults Select res.Id & " " & res.Score).ToArray
                End With
                RaiseEvent FaceIdentified(Me, args)
            Catch ex As Exception
                Utils.ShowException(ex)
            End Try
        End Function

#End Region

#Region "Detection Timer"
        Private stTime As Date
        Private WithEvents tmDetectCam1 As New Timer

        Public Camera1InUse As Boolean

        Private Async Sub tmDetectCam1_Tick(sender As Object, e As EventArgs) Handles tmDetectCam1.Tick
            tmDetectCam1.Enabled = False
            stTime = Now
            For Each f As NFace In CameraSubject1.Faces
                Await DetectFacesAsync(f.Image, ProcessType)
            Next
            tmDetectCam1.Enabled = True
        End Sub

        Private Sub setupTimer1(enable As Boolean)
            If enable Then
                tmDetectCam1 = New Timer
                tmDetectCam1.Interval = 1000
            End If
            tmDetectCam1.Enabled = enable
            Camera1InUse = enable
        End Sub
#End Region
    End Class



    Public Class FaceRecognizeEventArgs
        Inherits EventArgs

        Public UserID As String
        Public Subject As NSubject

        Public Status As NBiometricStatus = NBiometricStatus.None
        Public Score As String
        Public Message As String

        Public ElapseTime As TimeSpan

        Public Faces As Integer
        Public RawMatches As String()
    End Class

    Public Enum FaceProcessType
        Detect
        Identify
    End Enum

End Namespace
