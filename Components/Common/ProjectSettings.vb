'
' UF Office of Research
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

Imports DotNetNuke.Entities.Portals
Imports System.Web
Imports System.Web.Services

Imports System.Collections.Specialized
Imports System.Web.Security

Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules
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

    Public Class ProjectSettings

        Public ReadOnly Property slnPath() As String
            Get
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                If _portalSettings IsNot Nothing Then


                    If DotNetNuke.Common.Globals.NavigateURL <> "" Then

                        Return DotNetNuke.Common.Globals.NavigateURL.Replace("Home.aspx", "")
                    Else
                        Return ""
                    End If

                Else : Return ""

                End If

            End Get

        End Property

        Public ReadOnly Property appDefaultPagePath() As String
            Get
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                If _portalSettings IsNot Nothing Then

                    Dim intPos As Integer
                    Dim defaultPagePath As String

                    intPos = InStr(slnPath(), DotNetNuke.Common.Globals.ApplicationPath) + Len(DotNetNuke.Common.Globals.ApplicationPath)
                    defaultPagePath = "~" + Mid(slnPath(), intPos) + "default.aspx"
                    Return defaultPagePath

                Else : Return ""

                End If

            End Get

        End Property

        'from http://dnnhelpsystem.codeplex.com/releases/view/46505
        'Public ReadOnly Property PhysicalPath() As String
        '    Get

        '        Dim PortalID As Integer = 0

        '        Dim _PhysicalPath As String
        '        Dim PortalSettings As PortalSettings = Nothing
        '        If Not HttpContext.Current Is Nothing Then
        '            PortalSettings = PortalController.GetCurrentPortalSettings()
        '        End If

        '        Dim homeDir As String = PortalSettings.HomeDirectory
        '        Dim RelativePath As String = ""

        '        If PortalId = Null.NullInteger Then
        '            _PhysicalPath = DotNetNuke.Common.Globals.HostMapPath + RelativePath
        '        Else
        '            If PortalSettings Is Nothing OrElse PortalSettings.PortalId <> PortalId Then
        '                ' Get the PortalInfo  based on the Portalid
        '                Dim objPortals As New PortalController()
        '                Dim objPortal As PortalInfo = objPortals.GetPortal(PortalId)
        '                _PhysicalPath = objPortal.HomeDirectoryMapPath + RelativePath
        '            Else
        '                _PhysicalPath = PortalSettings.HomeDirectoryMapPath + RelativePath
        '            End If
        '        End If
        '        Return _PhysicalPath.Replace("/", "\")
        '    End Get
        'End Property


    End Class

End Namespace
