'
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

Imports System.Text.RegularExpressions
Imports System.Security.Principal

Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.Exceptions
Imports System.Net

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

Namespace UF.Research.Authentication.Shibboleth.SHIB

    Public Class Utilities

        Sub New()
        End Sub


        Public Shared Function GetAllSHIBGroupnames() As ArrayList

            Dim sh As ShibHandler = New ShibHandler
            Dim LoggedOnUserName As String = sh.EPPN

            Dim alGroupNames As ArrayList = sh.AdGroups
            Dim alGroupNames_A As ArrayList = New ArrayList
            For Each group As String In alGroupNames
                alGroupNames_A.Add(group)
            Next
            Return alGroupNames_A

        End Function

        Public Shared Function GetAllSHIBRolenames() As ArrayList

            Dim sh As ShibHandler = New ShibHandler

            Dim LoggedOnUserName As String = sh.EPPN

            Dim alRoleNames As ArrayList = sh.PSRoles
            Dim alRoleNames_P As ArrayList = New ArrayList
            For Each role As String In alRoleNames
                alRoleNames_P.Add(role)
            Next
            Return alRoleNames_P

        End Function


        Public Shared Function GetUser(ByVal Name As String) As ShibUserInfo
            Dim objUserInfo As ShibUserInfo = New ShibUserInfo

            Return objUserInfo

        End Function

        Public Shared Function CheckNullString(ByVal value As Object) As String
            If value Is Nothing Then
                Return ""
            Else
                Return value.ToString
            End If
        End Function

        Public Shared Function GetRandomPassword() As String
            Dim rd As New System.Random
            Return Convert.ToString(rd.Next)
        End Function

        Public Shared Function GetPSRoles(ByVal Name As String) As ArrayList

            Dim sh As ShibHandler = New ShibHandler
            Dim LoggedOnUserName As String = sh.EPPN

            Dim alPSRoles As ArrayList = sh.PSRoles
            Dim alPSRoles_P As ArrayList = New ArrayList
            For Each role As String In alPSRoles
                role = "P_" & role
                alPSRoles_P.Add(role)
            Next
            Return alPSRoles_P

        End Function

        Public Shared Function GetIP4Address(ByVal strPassedIP As String) As String
            Dim IP4Address As String = String.Empty

            For Each IPA As IPAddress In Dns.GetHostAddresses(strPassedIP)
                If IPA.AddressFamily.ToString() = "InterNetwork" Then
                    IP4Address = IPA.ToString()
                    Exit For
                End If
            Next

            If IP4Address <> String.Empty Then
                Return IP4Address
            End If

            For Each IPA As IPAddress In Dns.GetHostAddresses(Dns.GetHostName())
                If IPA.AddressFamily.ToString() = "InterNetwork" Then
                    IP4Address = IPA.ToString()
                    Exit For
                End If
            Next

            Return IP4Address
        End Function

        Public Shared Function GetCurrentTrustLevel() As AspNetHostingPermissionLevel
            For Each trustLevel As AspNetHostingPermissionLevel In New AspNetHostingPermissionLevel() {AspNetHostingPermissionLevel.Unrestricted, AspNetHostingPermissionLevel.High, AspNetHostingPermissionLevel.Medium, AspNetHostingPermissionLevel.Low, AspNetHostingPermissionLevel.Minimal}
                Try
                    Dim perm As New AspNetHostingPermission(trustLevel)
                    perm.Demand()
                Catch generatedExceptionName As System.Security.SecurityException
                    Continue For
                End Try

                Return trustLevel
            Next

            Return AspNetHostingPermissionLevel.None
        End Function

    End Class
End Namespace
