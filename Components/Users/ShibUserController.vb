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

Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Security
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Security.Membership.Data

Namespace UF.Research.Authentication.Shibboleth

    Public Class ShibUserController

#Region "Private Shared Members"

        Private mProviderTypeName As String = ""
        Private Shared dataProvider As DataProvider = dataProvider.Instance()
        Private Shared mRoleName As String = ""

#End Region

        Sub New()
            Dim _config As ShibConfiguration = ShibConfiguration.GetConfig()
            mProviderTypeName = "UF.Research.Authentication.Shibboleth.SHIB.SHIBProvider, UF.Research.Authentication.Shibboleth"
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     User object with info obtained from Shibboleth
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Function GetUser(ByVal LoggedOnUserName As String) As ShibUserInfo
            Return ShibAuthenticationProvider.Instance(mProviderTypeName).GetUser(LoggedOnUserName)

        End Function

        Public Overloads Shared Sub AddUserRoles(ByVal PortalID As Integer, ByVal AuthenticationUser As ShibUserInfo)
            Try
                'Get Portal information
                Dim objPortals As New PortalController
                Dim objPortal As PortalInfo = objPortals.GetPortal(PortalID)
                'Get all portal roles that the user does belong to.
                Dim objRoleController As New DotNetNuke.Security.Roles.RoleController
                Dim arrUserRoles As String() = objRoleController.GetRolesByUser(AuthenticationUser.UserID, PortalID)
                'Get all portal roles that correspond to an active directory group.
                Dim objGroupController As New GroupController
                'arrADGroups contains all AD Groups for the user that have roles defined in DNN
                'arrPSRoles contains all PS Roles for the user that have roles defined in DNN
                'If an AD Group or a PS role does not have a matching role name defined to DNN
                'we don't care about it. 

                Dim arrADGroups As ArrayList = objGroupController.GetGroups("ADGroups")
                Dim arrPSRoles As ArrayList = objGroupController.GetGroups("PSRoles")
               
                Dim arrCurrentRoles As New ArrayList
                Dim arrCurrentGroups As New ArrayList
                Dim arrAddToRole As New ArrayList
               
                'arrADGroups and arrPSRoles are of type GroupInfo and are built in
                'ShibProvider.vb GetGroups.

                'Get the Groups the user belongs to that are also AD Groups from Shibboleth.
                'Build arrCurrentGroups - an array list of roles corresponding to ADGroups
                'by looping thru the current arrADGroups (which contains all 
                'AD Groups defined for the user that have a corresponding DNN role defined)
                'and adding the role for the user into arrCurrentGroups if the role name
                'exists in arrUserRoles - the list of currently defined roles for the user

                'comparing arrADGroups and arrCurrentGroups:
                'Whereas arrADGroups contains an array of all AD Groups for the user that are
                'defined in DNN, arrCurrentGroups contains an array of all AD Groups for the 
                'user that currently exist as roles for that user in DNN

                For Each authGroup As GroupInfo In arrADGroups
                    If authGroup Is Nothing Then
                    Else
                        If Not (authGroup.RoleID = objPortal.AdministratorRoleId) Then
                            mRoleName = authGroup.RoleName
                            If Array.Exists(arrUserRoles, AddressOf RolesExists) Then
                                arrCurrentGroups.Add(authGroup)
                            End If
                        End If
                    End If
                Next

                'Get the Roles the user belongs to that are also PS Roles from Shibboleth.
                'comparing arrPSRoles and arrCurrentRoles:
                'Whereas arrPSGroups contains an array of all PS roles for the user that are
                'defined in DNN, arrCurrentRoles contains an array of all PS Roles for the 
                'user that currently exist as roles for that user in DNN

                For Each authRole As GroupInfo In arrPSRoles
                    If authRole Is Nothing Then
                    Else
                        If Not (authRole.RoleID = objPortal.AdministratorRoleId) Then
                            mRoleName = authRole.RoleName
                            If Array.Exists(arrUserRoles, AddressOf RolesExists) Then
                                arrCurrentRoles.Add(authRole)
                            End If
                        End If
                    End If
                Next

                'Compare the CurrentRoles and Shibboleth Groups
                'If user belongs to Shib group but does not belong to DNN Role then add user to role.

                For Each authGroup As GroupInfo In arrADGroups
                    If authGroup Is Nothing Then
                    Else

                        If Not (arrCurrentGroups.Contains(authGroup)) Then
                            arrAddToRole.Add(authGroup) 'Add the Group
                            objRoleController.AddUserRole(PortalID, AuthenticationUser.UserID, authGroup.RoleID, Date.Today, Null.NullDate)

                        End If
                    End If
                Next

                For Each authGroup As GroupInfo In arrPSRoles
                    If authGroup Is Nothing Then
                    Else

                        If Not (arrCurrentRoles.Contains(authGroup)) Then
                            arrAddToRole.Add(authGroup) 'Add the Role
                            objRoleController.AddUserRole(PortalID, AuthenticationUser.UserID, authGroup.RoleID, Date.Today, Null.NullDate)

                        End If
                    End If
                Next

                'Compare the CurrentRoles and Shibboleth Groups
                'If user belongs to DNN Role but does not belong to the Shib role/group then remove the user from that Group
                For Each authGroup As GroupInfo In arrCurrentGroups
                    If authGroup Is Nothing Then
                    Else
                        If Not (arrADGroups.Contains(authGroup)) Then
                            objRoleController.DeleteUserRole(PortalID, AuthenticationUser.UserID, authGroup.RoleID)
                        End If

                    End If
                Next

                For Each authGroup As GroupInfo In arrCurrentRoles
                    If authGroup Is Nothing Then
                    Else
                        If Not (arrPSRoles.Contains(authGroup)) Then
                            objRoleController.DeleteUserRole(PortalID, AuthenticationUser.UserID, authGroup.RoleID)
                        End If

                    End If
                Next


            Catch exc As Exception
                LogException(exc)
            End Try
        End Sub

        Private Shared Function RolesExists(ByVal s As String) _
            As Boolean

            ' AndAlso prevents evaluation of the second Boolean
            ' expression if the string is so short that an error
            ' would occur.
            If (s.ToLower = mRoleName.ToLower) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function UpdateDNNUser(ByVal AuthenticationUser As UF.Research.Authentication.Shibboleth.ShibUserInfo) As Boolean
            'Updating user information
            Users.UserController.UpdateUser(AuthenticationUser.PortalID, AuthenticationUser)
            Return True
        End Function

    End Class

End Namespace

