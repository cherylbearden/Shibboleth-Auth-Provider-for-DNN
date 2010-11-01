Imports Microsoft.VisualBasic
'
' UF Deptartment of Research
' Copyright (c) 2010
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

'testing muliple portal notes: 
'open your browser and delete all cookies so you start with no shib cookie, no dnn cookies
'if it's been awhile though, over half an hour and your shib cookie has expired, you will need to relogin again. 


Imports System
Imports System.Web
Imports System.Web.HttpServerUtility
Imports System.Web.UI.Control
Imports System.Web.UI
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Entities.Portals
Imports UF.Research.Authentication.Shibboleth
Imports System.Net
Imports System.Security
Imports DNNUserInfo = DotNetNuke.Entities.Users.UserInfo
Imports DotNetNuke.Security.Membership
Imports System.Web.Services
Imports System.Collections.Specialized
Imports System.Web.Security
Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules
Imports DNNUserController = DotNetNuke.Entities.Users.UserController
Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.UI.Skins.Controls.ModuleMessage
Imports DotNetNuke.UI.WebControls
Imports System.Data
Imports DotNetNuke.Entities.Profile
Imports DotNetNuke.Entities.Users
Imports System.Collections.Generic
Imports System.IO
Imports DotNetNuke.Services.Messaging.Data
Imports DotNetNuke.Services.Mail
Imports DotNetNuke.UI.UserControls
Imports DotNetNuke.Security.Permissions
Imports DotNetNuke.Entities.Host
Imports DotNetNuke.Entities.Tabs

Namespace UF.Research.Authentication.Shibboleth.HttpModules

    Public Class AuthenticationModule
        Implements IHttpModule

        Public ReadOnly Property ModuleName() As String

            Get
                Return "AuthenticationModule"
            End Get
        End Property

        Public Sub Init(ByVal application As HttpApplication) Implements IHttpModule.Init
            AddHandler application.AuthenticateRequest, AddressOf Me.OnAuthenticateRequest
        End Sub

        Public Sub OnAuthenticateRequest(ByVal s As Object, ByVal e As EventArgs)

            Dim Request As HttpRequest = HttpContext.Current.Request
            Dim Response As HttpResponse = HttpContext.Current.Response

            ''check if we are upgrading/installing/using a web service/rss feeds (ACD-7748)
            'Abort if NOT Default.aspx
            If Not Request.Url.LocalPath.ToLower.EndsWith("default.aspx") _
                OrElse Request.RawUrl.ToLower.Contains("rssid") Then
                Exit Sub
            End If
            
            If Request.ServerVariables("HTTP_USER_AGENT").Contains("gsa-crawler") Then
                Exit Sub
            End If

            Dim _portalSettings As PortalSettings = DotNetNuke.Common.GetPortalSettings

            'comment these out for testing since they cause more iterations thru AuthenticationModule.vb
            'If InStr(Request.RawUrl, "ScriptResource.axd") > 0 Or InStr(Request.RawUrl, "WebResource.axd") > 0 Then
            '    Exit Sub
            'End If

            'get the solution path
            Dim reDir As String
            Dim slnPath As String = ""
            GetSolutionPath(slnPath)

            If InStr(Request.RawUrl, "ShibHandler") > 0 Then
                Exit Sub
            End If

            Dim portalID As Integer

            Dim objPortalSettings As PortalSettings = Nothing
            objPortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
            If objPortalSettings Is Nothing Then Exit Sub
            portalID = objPortalSettings.PortalId

            Dim objAuthentication As New ShibAuthController
            Dim objShibUserController As ShibUserController = New ShibUserController
            'Dim authStatus As AuthenticationStatus = ShibAuthController.GetStatus(objPortalSettings.PortalId)
            Dim authStatus As AuthenticationStatus = GetCookieStatus(objPortalSettings.PortalId)


            ''''''''''''''''''''''''''''''''''''''
            'add code to stop page caching so dnn will display the latest version of a page
            'otherwise, when going from a non-shib site to a shib site, an earlier non-logged in version
            'of the shib site may display causing view mac state errors.

            'If InStr(Request.RawUrl.ToLower, "home.aspx") > 0 Then

            Response.CacheControl = "no-cache"
            Response.AddHeader("Pragma", "no-cache")
            Response.Expires = -1

            'End If

            ''''''''''''''''''''''''''''''''''''''''''


            Dim psDictionary As Dictionary(Of String, String) = PortalController.GetPortalSettingsDictionary(portalID)

            Dim config As ShibConfiguration = ShibConfiguration.GetConfig()
            If config Is Nothing Then
                Exit Sub
            End If

            Dim blnShibEnabled As Boolean = config.ShibbolethAuthProvider

            'set the logout page
            Dim myLogoutPage As String

            If psDictionary.ContainsKey("Shib_Authentication") Then
                myLogoutPage = config.LogoutPage
            Else
                myLogoutPage = "default.aspx"
            End If

            '0. Check 1st special condition: you've logged out, are on the MyLogout page, and clicked login

            If InStr(Request.RawUrl.ToLower, "login.aspx") > 0 And InStr(Request.RawUrl.ToLower, myLogoutPage.ToLower) > 0 _
            And CheckForShibCookie() And authStatus = AuthenticationStatus.SHIBLogoff Then

                SetDNNReturnToCookieAfterLogout(Request, Response, objPortalSettings)

                If psDictionary.ContainsKey("Shib_Authentication") Then

                    If psDictionary.Item("Shib_Authentication") = "True" Then

                        If Not (CheckForDNNCookie(portalID) And authStatus = AuthenticationStatus.Undefined) Then

                            reDir = "~/DesktopModules/AuthenticationServices/Shibboleth/Login/ShibHandler.ashx?" & portalID

                            ShibLogon(portalID)
                            'cb_1101
                            reDir = slnPath + "default.aspx"
                            HttpContext.Current.Response.Redirect(reDir)
                            'cb_1101
                            Exit Sub

                        End If

                    End If

                ElseIf CheckForDNNCookie(portalID) And authStatus <> AuthenticationStatus.DNNLogoff Then

                    If Not (CheckForDNNCookie(portalID) And authStatus = AuthenticationStatus.Undefined) Then

                        ShibLogon(portalID)
                        Exit Sub

                    End If

                End If
            End If

            '1. is this a logoff?
            If InStr(Request.RawUrl.ToLower, "logoff") > 0 Then
                objAuthentication.AuthenticationLogoff()
                ReSetDNNReturnToCookie(Request, Response, objPortalSettings)
                Exit Sub
            Else
                '2. is this a login?
                If (InStr(Request.RawUrl.ToLower, "/login.aspx?") > 0) Then 'Or InStr(Request.RawUrl, "/MyLogout.aspx") > 0 Then

                    SetDNNReturnToCookie(Request, Response, objPortalSettings)

                    'if you've been on thru shib before, log on thru shib again
                    If authStatus = AuthenticationStatus.SHIBLogon Or authStatus = AuthenticationStatus.SHIBLogoff Then

                        ShibLogon(portalID)
                        reDir = slnPath + "default.aspx"
                        HttpContext.Current.Response.Redirect(reDir)
                        Exit Sub

                    Else

                        If psDictionary.ContainsKey("Shib_Authentication") Then

                            If psDictionary.Item("Shib_Authentication") = "True" Then

                                ShibLogon(portalID)
                                reDir = slnPath + "default.aspx"
                                HttpContext.Current.Response.Redirect(reDir)
                                Exit Sub

                            Else
                                reDir = slnPath + "login.aspx?"
                                HttpContext.Current.Response.Redirect(reDir)
                                Exit Sub
                            End If

                        Else
                            reDir = slnPath + "login.aspx?"
                            HttpContext.Current.Response.Redirect(reDir)
                            Exit Sub
                        End If

                    End If

                End If

            End If



            Dim sh As ShibHandler = New ShibHandler
            Dim eppn As String = sh.EPPN

            If eppn IsNot Nothing Then
                If (DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo().Username = String.Empty) Then

                    If GetCookieStatus(portalID) = AuthenticationStatus.SHIBLogon Then
                        'check to see if username that you are logging in with using the DNN login is different from your existing
                        'Shib user name.  If it is, logoff and exit sub. 
                        'if you aren't allowing DNN login you don't need this.

                        ShibLogon(portalID)
                        reDir = slnPath + "default.aspx"
                        HttpContext.Current.Response.Redirect(reDir)

                    End If
                    'cb_103010
                    'dnn user is still there, status is DNNLogoff, force logoff and exit
                    'this was a problem in alpha1 domain, but not in dnn1 and dnn2 domains,
                    'so there may be a difference in the way the certificates behave
                    'ElseIf CheckForShibCookie() And GetCookieStatus(portalID) = AuthenticationStatus.DNNLogoff _
                    '    And (InStr(Request.RawUrl.ToLower, "/default.aspx") > 0) Then
                    '    Try
                    '        objAuthentication.AuthenticationLogoff()
                    '        ReSetDNNReturnToCookie(Request, Response, objPortalSettings)
                    '    Catch
                    '    End Try
                    '    Exit Sub
                    'cb_1030   
                End If

            End If

        End Sub

        Public Sub ShibLogon(ByVal portalID As Integer)

            Dim sh As ShibHandler = New ShibHandler
            Dim eppn As String = sh.EPPN

            'DNN caching settings: 
            'File, memory, heavy, server, no compression

            If CheckForShibCookie() And eppn IsNot Nothing Then

                'Dim reDir As String = "~/DesktopModules/AuthenticationServices/Shibboleth/ShibLogonHandler.ashx?" & portalID
                '' '' ''Dim reDir As String = "~/DesktopModules/AuthenticationServices/Shibboleth/ShibbolethLogon.ashx?"

                '' '' ''Dim instance As HttpServerUtility = System.Web.HttpContext.Current.Server
                '' '' ''instance.Transfer(reDir)
                '' '' ''must be calling isapi extension with Shibboleth because instance.transfer doesn't work
                'HttpContext.Current.RewritePath(reDir)

                Dim sl As ShibbolethLogin = New ShibbolethLogin
                sl.Login()


            Else

                Dim reDir As String = "~/DesktopModules/AuthenticationServices/Shibboleth/Login/ShibHandler.ashx?" & portalID
                HttpContext.Current.Response.Redirect(reDir)
               
            End If
        End Sub

        'Public Sub ClearPageCache()
        '    Dim keys As List(Of String) = New List(Of String)()

        '    Dim enumerator As IDictionaryEnumerator = Cache.GetEnumerator()

        '    Do While enumerator.MoveNext()

        '        keys.Add(enumerator.Key.ToString())

        '    Loop



        '    For Each key As String In keys

        '        Cache.Remove(key)

        '    Next key


        'End Sub


        ''sub to start a process
        'Public Sub ShellandWait(ByVal ProcessPath As String)
        '    Dim objProcess As System.Diagnostics.Process
        '    Try
        '        objProcess = New System.Diagnostics.Process()
        '        objProcess.StartInfo.FileName = ProcessPath
        '        objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal
        '        objProcess.Start()

        '        'Wait until the process passes back an exit code 
        '        objProcess.WaitForExit()

        '        'Free resources associated with this process
        '        objProcess.Close()
        '    Catch
        '        'MessageBox.Show("Could not start process " & ProcessPath, "Error")
        '    End Try
        'End Sub

        Public Function GetCookieStatus(ByVal portalID As Integer) As AuthenticationStatus

            Dim cookieName As String = "authentication.status." & portalID.ToString
            Dim strStatus As String
            If HttpContext.Current.Request.Cookies(cookieName) IsNot Nothing Then
                Try
                    strStatus = FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies(cookieName).Value).UserData

                    Return CType([Enum].Parse(GetType(AuthenticationStatus), strStatus), AuthenticationStatus)
                Catch
                    'http://forums.asp.net/p/1608654/4107624.aspx
                    Return AuthenticationStatus.Undefined
                End Try

            Else
                Return 0
            End If

        End Function

        Public Sub Dispose() Implements IHttpModule.Dispose
            ' Should check to see why this routine is never called
        End Sub

        Private Function GetRedirectURL(ByVal Request As HttpRequest, ByVal _portalSettings As PortalSettings) As String

            If Request.ApplicationPath = "/" Then
                Return ShibConfiguration.AUTHENTICATION_PATH & ShibConfiguration.AUTHENTICATION_LOGON_PAGE & "?tabid=" & _portalSettings.ActiveTab.TabID.ToString
            Else
                Return Request.ApplicationPath & ShibConfiguration.AUTHENTICATION_PATH & ShibConfiguration.AUTHENTICATION_LOGON_PAGE & "?tabid=" & _portalSettings.ActiveTab.TabID.ToString
            End If
        End Function

        Private Sub SetDNNReturnToCookie(ByVal Request As HttpRequest, ByVal Response As HttpResponse, ByVal _portalSettings As PortalSettings)
            Try
                Dim refUrl As String = Request.RawUrl
                Response.Clear()

                refUrl = refUrl.Replace("%2f", "/")

                Dim slnPath As String = ""
                GetSolutionPath(slnPath)

                refUrl = refUrl.Replace("%3d", "/")

                refUrl = slnPath + refUrl
                refUrl = slnPath + "default.aspx"

                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Value = refUrl
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Path = "/"
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Expires = DateTime.Now.AddMinutes(2)

            Catch ex As Exception
                LogException(ex)
            End Try

        End Sub

        Private Sub SetDNNReturnToCookieAfterLogout(ByVal Request As HttpRequest, ByVal Response As HttpResponse, ByVal _portalSettings As PortalSettings)

            Dim objPortalSettings As PortalSettings = Nothing
            objPortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
            If objPortalSettings Is Nothing Then Exit Sub

            Dim portalID As Integer = objPortalSettings.PortalId

            Dim psDictionary As Dictionary(Of String, String) = PortalController.GetPortalSettingsDictionary(portalID)

            Dim sh As ShibHandler = New ShibHandler
            Dim eppn As String = sh.EPPN

            Dim LoggedOnUserName As String = sh.EPPN

            Try
                Dim refUrl As String = Request.RawUrl
                Response.Clear()

                Dim portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

                Dim myLogoutPage As String
                Dim config As ShibConfiguration = ShibConfiguration.GetConfig()

                If psDictionary.ContainsKey("Shib_Authentication") = False Or eppn Is Nothing Then
                    myLogoutPage = "default.aspx"
                Else
                    myLogoutPage = config.LogoutPage
                End If

                Dim objTabController As New TabController
                Dim TabInfo As DotNetNuke.Entities.Tabs.TabInfo = objTabController.GetTabByName(myLogoutPage, _portalSettings.PortalId)
                If TabInfo Is Nothing Then
                    myLogoutPage = "default.aspx"
                End If

                Dim slnPath As String = ""
                GetSolutionPath(slnPath)

                refUrl = slnPath + myLogoutPage

                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Value = refUrl
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Path = "/"
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Expires = DateTime.Now.AddMinutes(2)

            Catch ex As Exception
                LogException(ex)
            End Try

        End Sub

        Private Sub ReSetDNNReturnToCookie(ByVal Request As HttpRequest, ByVal Response As HttpResponse, ByVal _portalSettings As PortalSettings)
            Try

                Dim objPortalSettings As PortalSettings = Nothing
                objPortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
                If objPortalSettings Is Nothing Then Exit Sub


                Dim sh As ShibHandler = New ShibHandler
                Dim eppn As String = sh.EPPN

                Dim LoggedOnUserName As String = sh.EPPN

                Dim portalID As Integer = objPortalSettings.PortalId

                Dim refUrl As String = Request.RawUrl
                Response.Clear()

                Dim slnPath As String = ""
                GetSolutionPath(slnPath)

                Dim myLogoutPage As String
                Dim config As ShibConfiguration = ShibConfiguration.GetConfig()

                Dim psDictionary As Dictionary(Of String, String) = PortalController.GetPortalSettingsDictionary(portalID)

                If psDictionary.ContainsKey("Shib_Authentication") = False Or eppn Is Nothing Then
                    myLogoutPage = "default.aspx"
                Else
                    myLogoutPage = config.LogoutPage
                End If

                refUrl = slnPath + myLogoutPage

                HttpContext.Current.Response.Redirect(refUrl)

                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Value = refUrl
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Path = "/"
                Response.Cookies("DNNReturnTo" & _portalSettings.PortalId.ToString()).Expires = DateTime.Now.AddMinutes(2)
                Dim objAuthentication As New ShibAuthController

            Catch ex As Exception
                LogException(ex)
            End Try

        End Sub

        Private Sub GetSolutionPath(ByRef slnPath As String)
            Dim prjSettings As ProjectSettings = New ProjectSettings
            slnPath = prjSettings.slnPath
        End Sub


        'Public Shared Sub InsertPortalSettings()

        '    Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

        '    If _portalSettings IsNot Nothing Then

        '        'Dim ps As PortalSettings = New PortalSettings
        '        'ps = CreateNewPortalSettings(_portalSettings.PortalId)

        '        PortalController.UpdatePortalSetting(_portalSettings.PortalId, "Shib_PortalSettings", _portalSettings.ToString)

        '    End If


        'End Sub


        Public Function CheckForShibCookie() As Boolean

            Dim Request As HttpRequest = HttpContext.Current.Request
            Dim blnShibCookieFound As Boolean
            blnShibCookieFound = False

            For i = 0 To Request.Cookies.Count - 1
                If InStr(HttpContext.Current.Request.Cookies.Item(i).Name, "_shibsession_") > 0 Then


                    blnShibCookieFound = True

                    Exit For
                End If
            Next

            Return blnShibCookieFound
        End Function

        Public Function CheckForDNNCookie(ByVal portalID As Integer) As Boolean

            Dim Request As HttpRequest = HttpContext.Current.Request
            Dim blnDNNCookieFound As Boolean
            blnDNNCookieFound = False

            For i = 0 To Request.Cookies.Count - 1
                If InStr(HttpContext.Current.Request.Cookies.Item(i).Name, "authentication.status." & portalID) > 0 Then

                    blnDNNCookieFound = True

                    Exit For
                End If
            Next

            Return blnDNNCookieFound
        End Function

    End Class

End Namespace

