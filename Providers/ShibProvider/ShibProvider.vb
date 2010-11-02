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

'Imports System.DirectoryServices

Imports UF.Research.Authentication.Shibboleth
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Entities.Portals
Imports System.Collections.Generic
Imports System.Collections

Namespace UF.Research.Authentication.Shibboleth.SHIB

    Public Class ShibProvider
        Inherits ShibAuthenticationProvider

        Private _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
        Private _shibConfig As ShibConfiguration = ShibConfiguration.GetConfig()

#Region "Private Methods"

        'Private Function GetSimplyUser(ByVal UserName As String) As ShibUserInfo
        '    Dim objAuthUser As New ShibUserInfo

        '    With objAuthUser
        '        .PortalID = _portalSettings.PortalId
        '        .IsNotSimplyUser = False
        '        '.GUID = ""
        '        .Username = UserName
        '        '.FirstName = Utilities.TrimUserDomainName(UserName)
        '        '.LastName = Utilities.GetUserDomainName(UserName)
        '        .IsSuperUser = False
        '        '.Location = _adsiConfig.ConfigDomainPath
        '        '.PrincipalName = Utilities.TrimUserDomainName(UserName) & "@" & .Location
        '        '.DistinguishedName = Utilities.ConvertToDistinguished(UserName)

        '        Dim strEmail As String = _shibConfig.DefaultEmailDomain
        '        If Not strEmail.Length = 0 Then
        '            If strEmail.IndexOf("@") = -1 Then
        '                strEmail = "@" & strEmail
        '            End If
        '            strEmail = .FirstName & strEmail
        '        Else
        '            strEmail = .FirstName & "@" & .LastName & ".com" ' confusing?
        '        End If
        '        ' Membership properties
        '        .Username = UserName
        '        .Email = strEmail
        '        .Membership.Approved = True
        '        .Membership.LastLoginDate = Date.Now
        '        .Membership.Password = Utilities.GetRandomPassword() 'Membership.GeneratePassword(6)
        '        .AuthenticationExists = False
        '    End With

        '    Return objAuthUser

        'End Function


#End Region

        Private Sub FillShibUserInfo(ByVal UserInfo As ShibUserInfo)

            Dim sh As ShibHandler = New ShibHandler

            With UserInfo

                .Username = UserInfo.Username
                .Membership.Approved = True
                .Membership.LastLoginDate = Date.Now
                .DistinguishedName = sh.DistinguishedName
                .Department = sh.Department
                .Email = sh.EPPN
                .DisplayName = sh.CN
                .FirstName = sh.GivenName
                .LastName = sh.SN
                .Profile.FirstName = sh.GivenName
                .Profile.LastName = sh.SN


            End With
        End Sub

        Public Overloads Overrides Function GetUser(ByVal LoggedOnUserName As String) As ShibUserInfo

            Dim objAuthUser As ShibUserInfo

            ' Return authenticated if no error 
            objAuthUser = New ShibUserInfo
            'ACD-6760
            InitializeUser(objAuthUser)

            Dim portalID As Integer
            portalID = _portalSettings.PortalId

            With objAuthUser
                .PortalID = portalID
                .Username = LoggedOnUserName
            End With

            FillShibUserInfo(objAuthUser)

            Return objAuthUser
        End Function
       
        Public Overloads Function GetShibGroups() As ArrayList
            ' Normally number of roles in DNN less than groups in Authentication,
            ' so start from DNN roles to get better performance
            Try
                Dim colGroup As New ArrayList
                Dim objRoleController As New DotNetNuke.Security.Roles.RoleController
                Dim lstRoles As ArrayList = objRoleController.GetPortalRoles(_portalSettings.PortalId)
                Dim objRole As DotNetNuke.Security.Roles.RoleInfo
                Dim AllAdGroupNames As ArrayList = Utilities.GetAllSHIBGroupnames
             
                For Each objRole In lstRoles
                    ' Auto assignment roles have been added by DNN, so don't need to get them
                    If Not objRole.AutoAssignment Then

                        ' It's possible in multiple domains network that search result return more than one group with the same name (i.e Administrators)
                        ' We better check them all
                        If AllAdGroupNames.Contains(objRole.RoleName) Then
                            Dim group As New GroupInfo

                            With group
                                .PortalID = objRole.PortalID
                                .RoleID = objRole.RoleID
                                 .RoleName = objRole.RoleName
                                .Description = objRole.Description
                                .ServiceFee = objRole.ServiceFee
                                .BillingFrequency = objRole.BillingFrequency
                                .TrialPeriod = objRole.TrialPeriod
                                .TrialFrequency = objRole.TrialFrequency
                                .BillingPeriod = objRole.BillingPeriod
                                .TrialFee = objRole.TrialFee
                                .IsPublic = objRole.IsPublic
                                .AutoAssignment = objRole.AutoAssignment
                            End With

                            colGroup.Add(group)
                        End If
                    End If
                Next

                Return colGroup

            Catch exc As System.Runtime.InteropServices.COMException
                LogException(exc)
                Return Nothing
            End Try
        End Function

        Public Overloads Overrides Function GetGroups(ByVal strRG As String) As ArrayList
            ' Normally number of roles in DNN less than groups in Authentication,
            ' so start from DNN roles to get better performance
            Try
                Dim colGroup As New ArrayList
                Dim objRoleController As New DotNetNuke.Security.Roles.RoleController

                Dim PortalID As Integer
                PortalID = _portalSettings.PortalId

                'Dim lstRoles As ArrayList = objRoleController.GetPortalRoles(_portalSettings.PortalId)
                Dim lstRoles As ArrayList = objRoleController.GetPortalRoles(PortalID)

                Dim AllAdGroupRoleNames As ArrayList
                If strRG = "ADGroups" Then
                    AllAdGroupRoleNames = Utilities.GetAllSHIBGroupnames
                Else 'strRG = "PSRoles"
                    AllAdGroupRoleNames = Utilities.GetAllSHIBRolenames
                End If

                Dim objRole As DotNetNuke.Security.Roles.RoleInfo = New DotNetNuke.Security.Roles.RoleInfo

                Dim mappedDNNRoles As ArrayList = New ArrayList

                'get a list of role mappings first. Role mappings have a Setting Name of 'Shib_RM_'.
                Dim psDict As System.Collections.Generic.Dictionary(Of String, String) = _
                New System.Collections.Generic.Dictionary(Of String, String)

                'you need to reset the cache before reading portal settings to make sure you 
                'get what is really current.

                ShibConfiguration.ResetConfig()

                psDict = PortalController.GetPortalSettingsDictionary(PortalID)

                Dim rmCount As Integer
                'Dim i As Integer = 0

                'Dim keys As List(Of String) = psDict.Keys.ToList()

                'Go thru loop once for each role mapping

                rmCount = 0

                Dim strRole As String = ""
                Dim strDNNRole As String = ""

                mappedDNNRoles.Clear()

                'for each shib role, go to portal settings to check if there are any role mappings for this shib role. 
                'You'll have to get the Shib Role as the part of the string in SettingValue after the delimiter. Starting
                'with "AD:" or "PS:". 

                For Each kvp As KeyValuePair(Of String, String) In psDict

                    If InStr(kvp.Key, "Shib_RM_") > 0 Then

                        If InStr(kvp.Value, "AD:") > 0 Then
                            strRole = Mid(kvp.Value, InStr(kvp.Value, "AD:") + 3)
                            strDNNRole = Left(kvp.Value, InStr(kvp.Value, "AD:") - 2)
                        Else
                            strRole = Mid(kvp.Value, InStr(kvp.Value, "PS:") + 3)
                            strDNNRole = Left(kvp.Value, InStr(kvp.Value, "PS:") - 2)
                        End If


                        If (InStr(kvp.Value, "AD:") > 0 And strRG = "ADGroups") Or _
                           (InStr(kvp.Value, "PS:") > 0 And strRG = "PSRoles") Then

                            For Each strShibRole As String In AllAdGroupRoleNames

                                'Get a list of DNN roles paired with this shib role... there may be > 1. 
                                'for each of these dnn roles store in array mappedDNNRoles. 

                                If strRole = strShibRole Then
                                    mappedDNNRoles.Add(strDNNRole)
                                End If
                            Next
                        End If

                    End If

                Next kvp

                'Add a group info record for each mapped DNN role. 

                For Each objRole In lstRoles

                    If mappedDNNRoles.Contains(objRole.RoleName) Then

                        Dim group As New GroupInfo

                        With group
                            .PortalID = objRole.PortalID
                            .RoleID = objRole.RoleID
                            .RoleName = objRole.RoleName
                            .Description = objRole.Description
                            .ServiceFee = objRole.ServiceFee
                            .BillingFrequency = objRole.BillingFrequency
                            .TrialPeriod = objRole.TrialPeriod
                            .TrialFrequency = objRole.TrialFrequency
                            .BillingPeriod = objRole.BillingPeriod
                            .TrialFee = objRole.TrialFee
                            .IsPublic = objRole.IsPublic
                            .AutoAssignment = objRole.AutoAssignment
                        End With

                        colGroup.Add(group)
                    End If
                Next

            
                Return colGroup

            Catch exc As System.Runtime.InteropServices.COMException
                LogException(exc)
                Return Nothing
            End Try
        End Function


        Private Sub InitializeUser(ByVal objUser As ShibUserInfo)
            If _portalSettings IsNot Nothing Then
                objUser.Profile.InitialiseProfile(_portalSettings.PortalId)

                objUser.Profile.PreferredLocale = _portalSettings.DefaultLanguage
                objUser.Profile.TimeZone = _portalSettings.TimeZoneOffset
            Else
                objUser.Profile.InitialiseProfile(2)
            End If
        End Sub

    End Class
End Namespace
