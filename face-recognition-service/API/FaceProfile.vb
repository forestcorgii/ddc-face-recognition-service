Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Reflection
Imports System.Text
Imports System.Windows.Forms
Imports Newtonsoft.Json
Namespace Manager
    Namespace API
        Public Class FaceProfile
            <JsonIgnore> Private Client As New HttpClient
            Public Url As String = ""
            Public ApiToken As String = ""
            Public Terminal As String = ""
            Public Site As String = ""

            Sub New()
                Client.Timeout = TimeSpan.FromMinutes(100)
                'c1916f2ac06191b5e1e77abd4315df5ee062a34d
            End Sub

            Public Async Function SendUserDetailRequest(employee_id As String) As Task(Of String)
                Dim response As HttpResponseMessage = Await Client.GetAsync(String.Format("{0}user/{1}/", Url, employee_id))
                Dim responseString As String = Await response.Content.ReadAsStringAsync()

                Return responseString
            End Function


            Public Async Function SendSyncPOSTRequest(postData As String) As Task(Of String)
                Try
                    Client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Token", ApiToken)

                    Dim content As New StringContent(postData, Encoding.UTF8, "application/json")

                    Dim response As HttpResponseMessage = Await Client.PostAsync(String.Format("{0}sync/", Url), content)

                    Dim responseString As String = Await response.Content.ReadAsStringAsync()

                    Return responseString
                Catch ex As Exception
                    MessageBox.Show(ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
                Return ""
            End Function

            Public Async Function SendSyncGETRequest(last_synced As Date) As Task(Of String)
                Client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Token", ApiToken)

                Dim response As HttpResponseMessage = Await Client.GetAsync(String.Format("{0}sync/?site={1}&terminal={2}&last_synced={3}", Url, Site, Terminal, last_synced.ToString("s")))
                Dim responseString As String = Await response.Content.ReadAsStringAsync()

                'If responseString = "[]" Then
                '    Return New EmployeeRecordCollection
                'End If

                Return responseString 'JsonConvert.DeserializeObject(Of EmployeeRecordCollection)(responseString)
            End Function
        End Class



    End Namespace
End Namespace
