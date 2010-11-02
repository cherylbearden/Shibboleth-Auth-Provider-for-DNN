'
' UF Office of Research
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

    Public Class ShibHandler
        Inherits DotNetNuke.Services.Authentication.AuthenticationLoginBase
        Implements System.Web.IHttpHandler


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

        Private mGLID As String = ""
        Private mDistinguishedName As String = ""
        '' Additional properties which are not provided by MemberRole
        Private mDepartment As String

        Private mEPPN As String
        Private mDisplayName As String
        Private mGivenName As String
        Private mCN As String
        Private mSN As String
        Private mMiddleName As String
        Private mUFID As String
        Private mPostalAddress As String

        Private alADGroups As ArrayList
        Private alPSRoles As ArrayList


        Public Property GLID() As String
            Get
                Return Context.Request.ServerVariables("HTTP_GLID")
            End Get
            Set(ByVal value As String)
                mGLID = value
            End Set
        End Property

        Public Property DistinguishedName() As String
            Get
                Return Context.Request.ServerVariables("HTTP_BUSINESSNAME")
            End Get
            Set(ByVal value As String)
                mDistinguishedName = value
            End Set
        End Property

        Public Property Department() As String
            Get
                Return Context.Request.ServerVariables("HTTP_DEPARTMENTNUMBER")
            End Get
            Set(ByVal value As String)
                mDepartment = value
            End Set
        End Property

        Public Property EPPN() As String
            Get
                Return Context.Request.ServerVariables("HTTP_EPPN")
            End Get
            Set(ByVal value As String)
                mEPPN = value
            End Set
        End Property

        Public Property DisplayName() As String
            Get
                Return Context.Request.ServerVariables("HTTP_DisplayName")
            End Get
            Set(ByVal value As String)
                mDisplayName = value
            End Set
        End Property

        Public Property GivenName() As String
            Get
                Return Context.Request.ServerVariables("HTTP_GIVENNAME")
            End Get
            Set(ByVal value As String)
                mGivenName = value
            End Set
        End Property

        Public Property CN() As String
            Get
                Return Context.Request.ServerVariables("HTTP_CN")
            End Get
            Set(ByVal value As String)
                mCN = value
            End Set
        End Property

        Public Property SN() As String
            Get
                Return Context.Request.ServerVariables("HTTP_SN")
            End Get
            Set(ByVal value As String)
                mSN = value
            End Set
        End Property

        Public Property MiddleName() As String
            Get
                Return Context.Request.ServerVariables("HTTP_MiddleName")
            End Get
            Set(ByVal value As String)
                mMiddleName = value
            End Set
        End Property

        Public Property UFID() As String
            Get
                Return Context.Request.ServerVariables("HTTP_UFID")
            End Get
            Set(ByVal value As String)
                mUFID = value
            End Set
        End Property

        Public Property PostalAddress() As String
            Get
                Return Context.Request.ServerVariables("HTTP_PostalAddress")
            End Get
            Set(ByVal value As String)
                mPostalAddress = value
            End Set
        End Property

        Public Property AdGroups() As ArrayList
            Get
                If alADGroups Is Nothing Then
                    ProcessHeaders()
                End If

                Return alADGroups
            End Get
            Set(ByVal value As ArrayList)
                alADGroups = value
            End Set
        End Property

        Public Property PSRoles() As ArrayList
            Get
                If alPSRoles Is Nothing Then
                    ProcessHeaders()
                End If

                Return alPSRoles
            End Get
            Set(ByVal value As ArrayList)
                alPSRoles = value
            End Set
        End Property


#Region "Private Members"

        Private memberProvider As DotNetNuke.Security.Membership.MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider.Instance()

#End Region

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

        Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

            Dim Request As HttpRequest = HttpContext.Current.Request
            Dim Response As HttpResponse = HttpContext.Current.Response

            Dim portalID As Integer = Request.QueryString.Item(0)

            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            Dim ps As PortalSettings = New PortalSettings
            ps = CreateNewPortalSettings(portalID)

            _portalSettings = ps

            Dim config As ShibConfiguration = ShibConfiguration.GetConfig()
            If config Is Nothing Then
                Exit Sub
            End If

            Dim objRequest As HttpRequest = context.Request

            Dim prjSettings As UF.Research.Authentication.Shibboleth.ProjectSettings = New ProjectSettings
            Dim slnPath As String = prjSettings.slnPath
            Dim strURL As String

            Dim ipAddress As String

            Dim eppn As String = context.Request.ServerVariables("HTTP_EPPN")

            If context.Request.ServerVariables("HTTP_EPPN") Is Nothing Then
                'if there is nothing returned from Shibboleth, then you probably
                'don't have the Login directory secured by Shibboleth and you're 
                'getting here without logging in first. 
                'ToDo
                'send a message asking the user if directory has been secured to shibboleth
                'and return.
                Console.WriteLine("No Data Returned From Shibboleth!  Is your \Login directory secured?")
                strURL = slnPath + "Home.aspx"
                HttpContext.Current.Response.Redirect(strURL, True)

            Else

                ProcessHeaders()

                Dim UserName = context.Request.ServerVariables("HTTP_EPPN")

                LoginStatus = UserLoginStatus.LOGIN_SUCCESS
                Dim testUserName As String = UserName + CType(DateTime.Now, String)

                Dim objAuthentication As ShibAuthController = New ShibAuthController
                Dim objUser As DNNUserInfo = objAuthentication.ManualLogon(UserName, LoginStatus, ipAddress)

                Me.DistinguishedName = context.Request.ServerVariables("HTTP_BUSINESSNAME")

                Dim authenticated As Boolean = Null.NullBoolean
                Dim message As String = Null.NullString
                authenticated = (LoginStatus <> UserLoginStatus.LOGIN_FAILURE)

                'If objUser is nothing then there must've been a problem logging in. Write to the eventlog.
                If objUser Is Nothing Then
                    AddEventLog(portalID, UserName, Null.NullInteger, PortalSettings.PortalName, ipAddress, LoginStatus)
                Else

                    objAuthentication.AuthenticationLogon()
                    Dim eventArgs As UserAuthenticatedEventArgs = New UserAuthenticatedEventArgs(objUser, UserName, LoginStatus, "Shibboleth")
                    eventArgs.Authenticated = authenticated
                    eventArgs.Message = message
                    OnUserAuthenticated(eventArgs)
                End If

                If Not HttpContext.Current.Request.Cookies("DNNReturnTo" + _portalSettings.PortalId.ToString()) Is Nothing Then
                    strURL = HttpContext.Current.Request.Cookies("DNNReturnTo" + _portalSettings.PortalId.ToString()).Value
                Else
                    strURL = slnPath + "Home.aspx"

                End If

                HttpContext.Current.Response.Redirect(strURL, True)

                If Not HttpContext.Current.Request.Cookies("DNNReturnTo" + _portalSettings.PortalId.ToString()) Is Nothing Then
                    strURL = HttpContext.Current.Request.Cookies("DNNReturnTo" + _portalSettings.PortalId.ToString()).Value
                    'strURL = HttpContext.Current.Request.Cookies("DNNReturnTo").Value
                Else
                    strURL = slnPath + "Home.aspx"

                End If

                HttpContext.Current.Response.Redirect(strURL, True)
                'HttpContext.Current.Response.Redirect("~/Default.aspx")
                'Dim instance As HttpServerUtility = System.Web.HttpContext.Current.Server
                'instance.Transfer("~/Default.aspx")


            End If


        End Sub

        Public Sub ProcessHeaders()

            Dim objRequest As HttpRequest = Context.Request

            For Each Header As String In objRequest.Headers

                Select Case Header.ToUpper
                    Case "UFAD_GROUPS"
                        alADGroups = OutputArray(Header, ";")
                    Case "UFAD_PSROLES"
                        alPSRoles = OutputArray(Header, "$")
                    Case "HTTPS_SERVER_ISSUER", "HTTPS_SERVER_SUBJECT", "CERT_SERVER_SUBJECT", "CERT_SERVER_ISSUER"

                    Case "ALL_HTTP", "ALL_RAW"

                    Case Else

                        If objRequest.Headers.Item(Header) = "" Then
                            'Me.Page.Response.Write(" (blank)<br><br>")
                        Else
                            'Me.Page.Response.Write("<br>--> " & objRequest.Headers.Item(Header) & "<br><br>")
                        End If

                End Select

            Next Header

        End Sub

        Public Function OutputArray(ByVal sHeader As String, ByVal sDelimiter As String) As ArrayList

            Dim al As ArrayList
            Dim objRequest As HttpRequest = Context.Request
            Dim sArray As String()
            Dim sArrayList As ArrayList = New ArrayList

            sArray = Split(objRequest.Headers.Item(sHeader), sDelimiter)
            For Each sItem As String In sArray
                sArrayList.Add(sItem)
            Next
            If sHeader.ToUpper = "UFAD_GROUPS" Then
                al = sArrayList
                Return al
            Else
                If sHeader.ToUpper = "UFAD_PSROLES" Then
                    al = sArrayList
                    Return al
                Else
                    Return Nothing
                End If
            End If

        End Function

        ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return False
            End Get
        End Property

        Public Shared Function CreateNewPortalSettings(ByVal portalId As Integer) As DotNetNuke.Entities.Portals.PortalSettings
            'new settings object
            Dim ps As PortalSettings = New PortalSettings()
            'controller instances
            Dim pc As PortalController = New PortalController()
            Dim tc As TabController = New TabController()
            Dim pac As PortalAliasController = New PortalAliasController()

            'get the first portal alias found to be used as the current portal alias
            Dim portalAlias As PortalAliasInfo = Nothing
            Dim aliases As PortalAliasCollection = pac.GetPortalAliasByPortalID(portalId)
            Dim aliasKey As String = ""
            If Not aliases Is Nothing AndAlso aliases.Count > 0 Then
                'get the first portal alias in the list and use that
                For Each key As String In aliases.Keys
                    aliasKey = key
                    portalAlias = aliases(key)
                    Exit For
                Next key
            End If
            'get the portal and copy across the settings
            Dim portal As PortalInfo = pc.GetPortal(portalId)
            If Not portal Is Nothing Then

                ps.PortalAlias = portalAlias
                ps.PortalId = portal.PortalID
                ps.PortalName = portal.PortalName
                ps.LogoFile = portal.LogoFile
                ps.FooterText = portal.FooterText
                ps.ExpiryDate = portal.ExpiryDate
                ps.UserRegistration = portal.UserRegistration
                ps.BannerAdvertising = portal.BannerAdvertising
                ps.Currency = portal.Currency
                ps.AdministratorId = portal.AdministratorId
                ps.Email = portal.Email
                ps.HostFee = portal.HostFee
                ps.HostSpace = portal.HostSpace
                ps.PageQuota = portal.PageQuota
                ps.UserQuota = portal.UserQuota
                ps.AdministratorRoleId = portal.AdministratorRoleId
                ps.AdministratorRoleName = portal.AdministratorRoleName
                ps.RegisteredRoleId = portal.RegisteredRoleId
                ps.RegisteredRoleName = portal.RegisteredRoleName
                ps.Description = portal.Description
                ps.KeyWords = portal.KeyWords
                ps.BackgroundFile = portal.BackgroundFile
                ps.GUID = portal.GUID
                ps.SiteLogHistory = portal.SiteLogHistory
                ps.AdminTabId = portal.AdminTabId
                ps.SuperTabId = portal.SuperTabId
                ps.SplashTabId = portal.SplashTabId
                ps.HomeTabId = portal.HomeTabId
                ps.LoginTabId = portal.LoginTabId
                ps.UserTabId = portal.UserTabId
                ps.DefaultLanguage = portal.DefaultLanguage
                ps.TimeZoneOffset = portal.TimeZoneOffset
                ps.HomeDirectory = portal.HomeDirectory
                'ps.Version = portal.Version
                'ps.Application.Version = portal.Version

                '' ''ps.AdminSkin = SkinController.GetSkin(SkinInfo.RootSkin, portalId, SkinType.Admin)
                ' ''ps.DefaultAdminSkin = SkinController.GetSkin(SkinInfo.RootSkin, portalId, SkinType.Admin)


                '' ''If ps.AdminSkin Is Nothing Then
                '' ''    ps.AdminSkin = SkinController.GetSkin(SkinInfo.RootSkin, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Admin)
                '' ''End If

                ' ''If ps.DefaultAdminSkin Is Nothing Then
                ' ''    ps.DefaultAdminSkin = SkinController.GetSkin(SkinInfo.RootSkin, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Admin)
                ' ''End If

                '' ''ps.PortalSkin = SkinController.GetSkin(SkinInfo.RootSkin, portalId, SkinType.Portal)
                ' ''ps.DefaultPortalSkin = SkinController.GetSkin(SkinInfo.RootSkin, portalId, SkinType.Portal)

                '' ''If ps.PortalSkin Is Nothing Then
                '' ''    ps.PortalSkin = SkinController.GetSkin(SkinInfo.RootSkin, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Portal)
                '' ''End If
                ' ''If ps.DefaultPortalSkin Is Nothing Then
                ' ''    ps.DefaultPortalSkin = SkinController.GetSkin(SkinInfo.RootSkin, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Portal)
                ' ''End If

                '' ''ps.AdminContainer = SkinController.GetSkin(SkinInfo.RootContainer, portalId, SkinType.Admin)
                ' ''ps.DefaultAdminContainer = SkinController.GetSkin(SkinInfo.RootContainer, portalId, SkinType.Admin)

                '' ''If ps.AdminContainer Is Nothing Then
                '' ''    ps.AdminContainer = SkinController.GetSkin(SkinInfo.RootContainer, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Admin)
                '' ''End If

                ' ''If ps.DefaultAdminContainer Is Nothing Then
                ' ''    ps.DefaultAdminContainer = SkinController.GetSkin(SkinInfo.RootContainer, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Admin)
                ' ''End If

                '' ''ps.PortalContainer = SkinController.GetSkin(SkinInfo.RootContainer, portalId, SkinType.Portal)
                ' ''ps.DefaultPortalContainer = SkinController.GetSkin(SkinInfo.RootContainer, portalId, SkinType.Portal)

                '' ''If ps.PortalContainer Is Nothing Then
                '' ''    ps.PortalContainer = SkinController.GetSkin(SkinInfo.RootContainer, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Portal)
                '' ''End If

                ' ''If ps.DefaultPortalContainer Is Nothing Then
                ' ''    ps.DefaultPortalContainer = SkinController.GetSkin(SkinInfo.RootContainer, DotNetNuke.Common.Utilities.Null.NullInteger, SkinType.Portal)
                ' ''End If

                ps.Pages = portal.Pages
                ps.Users = portal.Users
                ' set custom properties
                If DotNetNuke.Common.Utilities.Null.IsNull(ps.HostSpace) Then
                    ps.HostSpace = 0
                End If
                If DotNetNuke.Common.Utilities.Null.IsNull(ps.DefaultLanguage) Then
                    ps.DefaultLanguage = DotNetNuke.Services.Localization.Localization.SystemLocale
                End If
                If DotNetNuke.Common.Utilities.Null.IsNull(ps.TimeZoneOffset) Then
                    ps.TimeZoneOffset = DotNetNuke.Services.Localization.Localization.SystemTimeZoneOffset
                End If
                Dim prjSettings As UF.Research.Authentication.Shibboleth.ProjectSettings = New ProjectSettings
                Dim slnPath As String = prjSettings.slnPath
                ps.HomeDirectory = DotNetNuke.Common.Globals.ApplicationPath & "/" & portal.HomeDirectory & "/"
                ps.HomeDirectory = DotNetNuke.Common.Globals.ApplicationPath & "/" & portal.HomeDirectory & "/"

                ' get application version
                Dim arrVersion As String() = DotNetNuke.Common.Assembly.glbAppVersion.Split("."c)
                Dim intMajor As Integer = 0
                Dim intMinor As Integer = 0
                Dim intBuild As Integer = 0
                Int32.TryParse(arrVersion(0), intMajor)
                Int32.TryParse(arrVersion(1), intMinor)
                Int32.TryParse(arrVersion(2), intBuild)
                'ps.Version = intMajor.ToString() & "." & intMinor.ToString() & "." & intBuild.ToString()
                'ps.Application.Version = intMajor.ToString() & "." & intMinor.ToString() & "." & intBuild.ToString()

            End If

            'Add each portal Tab to DekstopTabs
            Dim portalTab As TabInfo = Nothing
            'ps.DesktopTabs = New ArrayList()
            Dim first As Boolean = True
            For Each tabPair As KeyValuePair(Of Integer, TabInfo) In tc.GetTabsByPortal(ps.PortalId)
                ' clone the tab object ( to avoid creating an object reference to the data cache )
                portalTab = tabPair.Value.Clone()
                ' set custom properties
                If portalTab.TabOrder = 0 Then
                    portalTab.TabOrder = 999
                End If
                If DotNetNuke.Common.Utilities.Null.IsNull(portalTab.StartDate) Then
                    portalTab.StartDate = System.DateTime.MinValue
                End If
                If DotNetNuke.Common.Utilities.Null.IsNull(portalTab.EndDate) Then
                    portalTab.EndDate = System.DateTime.MaxValue
                End If
                'ps.DesktopTabs.Add(portalTab)

                'assign the first 'normal' tab as the active tab - could be the home tab, if it 
                'still exists, or it will be after the admin tab(s)
                If first AndAlso (portalTab.TabID = portal.HomeTabId OrElse portalTab.TabID > portal.AdminTabId) Then
                    ps.ActiveTab = portalTab
                    first = False
                End If
            Next tabPair
            'last gasp chance in case active tab was not set
            If ps.ActiveTab Is Nothing Then
                ps.ActiveTab = portalTab
            End If
            'Add each host Tab to DesktopTabs
            Dim hostTab As TabInfo = Nothing
            For Each tabPair As KeyValuePair(Of Integer, TabInfo) In tc.GetTabsByPortal(DotNetNuke.Common.Utilities.Null.NullInteger)
                ' clone the tab object ( to avoid creating an object reference to the data cache )
                hostTab = tabPair.Value.Clone()
                hostTab.PortalID = ps.PortalId
                hostTab.StartDate = System.DateTime.MinValue
                hostTab.EndDate = System.DateTime.MaxValue
                'ps.DesktopTabs.Add(hostTab)
            Next tabPair

            'now add the portal settings to the httpContext
            If System.Web.HttpContext.Current Is Nothing Then
                'if there is no HttpContext, then mock one up by creating a fake WorkerRequest
                Dim appVirtualDir As String = DotNetNuke.Common.Globals.ApplicationPath
                Dim appPhysicalDir As String = AppDomain.CurrentDomain.BaseDirectory
                Dim page As String = ps.PortalAlias.HTTPAlias
                Dim query As String = String.Empty
                Dim output As System.IO.TextWriter = Nothing
                'create a dummy simple worker request
                Dim workerRequest As System.Web.Hosting.SimpleWorkerRequest = New System.Web.Hosting.SimpleWorkerRequest(page, query, output)
                System.Web.HttpContext.Current = New System.Web.HttpContext(workerRequest)
            End If

            'stash the portalSettings in the Context Items, where the rest of the DNN Code expects it to be
            'always remove the old portal settings and put in the current one in case a portal setting from a 
            'different portal is still being stored. 
            'then, you can probably take out the portalID specification used everywhere. 

            If System.Web.HttpContext.Current.Items("PortalSettings") IsNot Nothing Then
                System.Web.HttpContext.Current.Items.Remove("PortalSettings")
            End If

            System.Web.HttpContext.Current.Items.Add("PortalSettings", ps)

            Return ps
        End Function

    End Class

End Namespace