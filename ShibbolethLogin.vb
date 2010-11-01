'
' UF Deptartment of Research
' Copyright (c) 2010
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Imports System.Web
Imports System.Web.Services

Imports System.Collections.Specialized
Imports System.Web.Security

Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DNNUserController = DotNetNuke.Entities.Users.UserController
Imports DNNUserInfo = DotNetNuke.Entities.Users.UserInfo

Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.UI.Skins.Controls.ModuleMessage
Imports DotNetNuke.UI.WebControls
Imports DotNetNuke.Security
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
Imports UF.Research.Authentication.Shibboleth

Imports UF.Research.Authentication.Shibboleth.SHIB
Imports System
Imports System.Web.UI.Control

Imports System.Web.UI
Imports DotNetNuke
Imports DotNetNuke.Common.Globals
Imports System.Net
Imports System.Security

Imports DotNetNuke.Entities.Tabs


Namespace UF.Research.Authentication.Shibboleth

    Public Class ShibbolethLogin
        Inherits DotNetNuke.Services.Authentication.AuthenticationLoginBase


        Protected Property LoginStatus() As UserLoginStatus
            Get
                Dim _LoginStatus As UserLoginStatus = UserLoginStatus.LOGIN_FAILURE
                If Not ViewState("LoginStatus") Is Nothing Then
                    _LoginStatus = CType(ViewState("LoginStatus"), UserLoginStatus)
                End If
                Return _LoginStatus
            End Get
            Set(ByVal value As UserLoginStatus)
                ViewState("LoginStatus") = value
            End Set
        End Property



        Private Shared Sub AddEventLog(ByVal portalId As Integer, ByVal username As String, ByVal userId As Integer, ByVal portalName As String, ByVal Ip As String, ByVal loginStatus As UserLoginStatus)

            Dim objEventLog As New DotNetNuke.Services.Log.EventLog.EventLogController

            ' initialize log record
            Dim objEventLogInfo As New DotNetNuke.Services.Log.EventLog.LogInfo
            Dim objSecurity As New PortalSecurity
            objEventLogInfo.AddProperty("IP", Ip)
            objEventLogInfo.LogPortalID = portalId
            objEventLogInfo.LogPortalName = portalName
            objEventLogInfo.LogUserName = objSecurity.InputFilter(username, PortalSecurity.FilterFlag.NoScripting Or PortalSecurity.FilterFlag.NoAngleBrackets Or PortalSecurity.FilterFlag.NoMarkup)
            objEventLogInfo.LogUserID = userId

            ' create log record
            objEventLogInfo.LogTypeKey = loginStatus.ToString
            objEventLog.AddLog(objEventLogInfo)

        End Sub

#Region "Public Properties"

        Public Overrides ReadOnly Property Enabled() As Boolean
            Get
                Try
                    'Make sure app is running at full trust
                    Dim HostingPermissions As New AspNetHostingPermission(System.Security.Permissions.PermissionState.Unrestricted)
                    HostingPermissions.Demand()

                    'Check if Windows Auth is enabled for the portal
                    Return ShibConfiguration.GetConfig().ShibbolethAuthProvider
                Catch ex As Exception
                    Return False
                End Try
            End Get
        End Property

#End Region

        Public Sub Login()

            Dim LoginStatus As AuthenticationStatus

            Dim Request As HttpRequest = HttpContext.Current.Request
            Dim Response As HttpResponse = HttpContext.Current.Response

            Dim portalID As Integer

            Dim objPortalSettings As PortalSettings = Nothing
            objPortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
            If objPortalSettings Is Nothing Then Exit Sub
            portalID = objPortalSettings.PortalId

            'Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            Dim config As ShibConfiguration = ShibConfiguration.GetConfig()
            If config Is Nothing Then
                Exit Sub
            End If

            Dim prjSettings As UF.Research.Authentication.Shibboleth.ProjectSettings = New ProjectSettings
            Dim slnPath As String = prjSettings.slnPath

            Dim ipAddress As String = ""

            Dim sh As ShibHandler = New ShibHandler
            Dim eppn As String = sh.EPPN

            If eppn Is Nothing Then

            Else

                Dim UserName As String = sh.EPPN

                LoginStatus = UserLoginStatus.LOGIN_SUCCESS
                Dim testUserName As String = UserName + CType(DateTime.Now, String)

                Dim objAuthentication As ShibAuthController = New ShibAuthController
                Dim objUser As DNNUserInfo = objAuthentication.ManualLogon(UserName, LoginStatus, ipAddress)

                Dim authenticated As Boolean = Null.NullBoolean
                Dim message As String = Null.NullString
                authenticated = (LoginStatus <> UserLoginStatus.LOGIN_FAILURE)

                If objUser Is Nothing Then
                    AddEventLog(portalID, UserName, Null.NullInteger, objPortalSettings.PortalName, ipAddress, LoginStatus)
                Else

                    objAuthentication.AuthenticationLogon()
                    Dim eventArgs As UserAuthenticatedEventArgs = New UserAuthenticatedEventArgs(objUser, UserName, LoginStatus, "Shibboleth")
                    eventArgs.Authenticated = authenticated
                    eventArgs.Message = message
                    OnUserAuthenticated(eventArgs)
                End If

            End If

        End Sub

    End Class

End Namespace

