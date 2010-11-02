'
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

Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Framework.Providers
Imports DotNetNuke.Security
Imports DotNetNuke.Common
Imports System.Collections.Generic


Namespace UF.Research.Authentication.Shibboleth

    Public Class ShibConfiguration

        Public Const AUTHENTICATION_PATH As String = "/DesktopModules/AuthenticationServices/Shibboleth/"
        Public Const AUTHENTICATION_LOGON_PAGE As String = "login.aspx"
        Public Const AUTHENTICATION_KEY As String = "authentication"
        Public Const AUTHENTICATION_STATUS_KEY As String = "authentication.status"
        Public Const LOGON_USER_VARIABLE As String = "LOGON_USER"
        '
        Private Const AUTHENTICATION_CONFIG_CACHE_PREFIX As String = "Authentication.ShibbolethConfiguration"

        'cache key names for settingss
        Private Const ksEnabled As String = "Shib_Authentication"
        Private Const ksProviderTypeName As String = "Shib_ProviderTypeName"
        Private Const ksIncludeHelp As String = "Shib_IncludeHelp"
        Private Const ksAutoCreateUsers As String = "Shib_AutoCreateUsers"
        Private Const ksSynchronizeRoles As String = "Shib_SynchronizeRoles"
        Private Const ksDelimiter As String = "Shib_Delimiter"
        Private Const ksLogoutPage As String = "Shib_LogoutPage"
        Private Const ksLoginPage As String = "Shib_LoginPage"
        Private Const ksRoleMappingCount As String = "Shib_RoleMappingCount"


        'private fields for Properties
        Private mPortalId As Integer
        Private mEnabled As Boolean = False
        Private mProviderTypeName As String = DefaultProviderTypeName
        Private mIncludeHelp As Boolean = False
        Private mAutoCreateUsers As Boolean = True
        Private mSynchronizeRoles As Boolean = True
        Private mDelimiter As Char = ";"
        Private mLogoutPage As String = "Default.aspx"
        Private mLoginPage As String = "Login.aspx".ToLower
        Private mNumberofRoleMappings As Integer = 0
        Private mUserRoleMappings As List(Of String)


#Region "  Constructors "

        ''' -----------------------------------------------------------------------------------
        ''' <summary>
        ''' - Obtain Authentication settings from database
        ''' </summary>
        ''' <remarks>
        ''' - Setting records are stored in ModuleSettings table, separately for each portal,
        ''' this method allows each portal to have different settings for the provider.
        ''' </remarks>
        ''' -----------------------------------------------------------------------------------    

        Sub New()
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            mPortalId = _portalSettings.PortalId
            Dim _providerConfiguration As ProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(AUTHENTICATION_KEY)
            Dim objSecurity As New PortalSecurity

            Try
                If _providerConfiguration.DefaultProvider Is Nothing Then
                    ' No provider specified, so disable authentication feature
                    Return
                Else
                    'Cambrian = DNN v5
                    'PortalController.UpdatePortalSetting(PortalId, "Shib_Authentication", ShibbolethAuthProvider.ToString)
                    'Dim CambrianSettings As System.Collections.Generic.Dictionary(Of String, String) = _
                    'PortalController.GetPortalSettingsDictionary(PortalId)


                    Dim CambrianSettings As Dictionary(Of String, String) = PortalController.GetPortalSettingsDictionary(PortalId)

                    Try
                        mEnabled = CType(Null.GetNull(CambrianSettings(ksEnabled), mEnabled), Boolean)

                    Catch ex As Exception
                        'ksEnabled not yet in portal settings, do nothing
                    End Try

                    Try
                        mAutoCreateUsers = CType(Null.GetNull(CambrianSettings(ksAutoCreateUsers), mAutoCreateUsers), Boolean)
                    Catch ex As Exception
                        'ksAutoCreateUsers not yet in portal settings, do nothing
                    End Try

                    Try
                        mSynchronizeRoles = CType(Null.GetNull(CambrianSettings(ksSynchronizeRoles), mSynchronizeRoles), Boolean)
                    Catch ex As Exception
                        'ksSynchronizeRoles not yet in portal settings, do nothing
                    End Try

                    Try
                        mDelimiter = CType(Null.GetNull(CambrianSettings(ksDelimiter), mDelimiter), Char)
                    Catch ex As Exception
                        'ksDelimiter not yet in portal settings, do nothing
                    End Try

                    Try
                        mLogoutPage = CType(Null.GetNull(CambrianSettings(ksLogoutPage), mLogoutPage), String)
                    Catch ex As Exception
                        'ksLogoutPage not yet in portal settings, do nothing
                    End Try

                    Try
                        mLoginPage = CType(Null.GetNull(CambrianSettings(ksLoginPage), mLoginPage), String)
                    Catch ex As Exception
                        'ksLoginPage not yet in portal settings, do nothing
                    End Try

                    Dim rmElement As UF.Research.Shibboleth.Authentication.RoleMappingElement _
                      = New UF.Research.Shibboleth.Authentication.RoleMappingElement()

                    Dim roleMappings As RoleMappings = New RoleMappings()

                End If

            Catch ex As Exception
                ' Do nothing: we cannot access data at this time
            End Try

        End Sub

#End Region


        Public Shared Function GetConfig() As ShibConfiguration

            Dim config As ShibConfiguration = Nothing
            Dim portalID As Integer

            Try

                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                portalID = _portalSettings.PortalId

                Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr(portalID)

                config = CType(DataCache.GetCache(strKey), ShibConfiguration)

                If config Is Nothing Then
                    config = New ShibConfiguration
                    DataCache.SetCache(strKey, config)
                End If

            Catch exc As Exception
                ' Problems reading Shib config, just return nothing
                Console.WriteLine(String.Format("Configuration.GetConfig - Exception: {0}", exc.ToString))
            End Try
            Return config

        End Function

        Public Shared Sub ResetConfig()
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Dim portalID As Integer
            portalID = _portalSettings.PortalId
            
            Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr(portalID)

            DataCache.RemoveCache(strKey)
            strKey = "ShibAuthenticationProvider" & CStr(portalID)
            DataCache.RemoveCache(strKey)
        End Sub

        Public Shared Sub UpdateConfig(ByVal PortalID As Integer, _
        ByVal ShibbolethAuthProvider As Boolean, _
        ByVal AutoCreateUsers As Boolean, _
        ByVal SynchronizeRoles As Boolean, _
        ByVal Delimiter As Char, _
        ByVal LogoutPage As String, _
        ByVal LoginPage As String)

            Dim objSecurity As New PortalSecurity

            'Cambrian Changes

            PortalController.UpdatePortalSetting(PortalID, ksEnabled, ShibbolethAuthProvider.ToString)
            PortalController.UpdatePortalSetting(PortalID, ksAutoCreateUsers, AutoCreateUsers.ToString)
            PortalController.UpdatePortalSetting(PortalID, ksSynchronizeRoles, SynchronizeRoles.ToString)
            PortalController.UpdatePortalSetting(PortalID, ksDelimiter, Delimiter.ToString)
            PortalController.UpdatePortalSetting(PortalID, ksLogoutPage, LogoutPage.ToString)
            PortalController.UpdatePortalSetting(PortalID, ksLoginPage, LoginPage.ToString)

        End Sub
        'bringing in rmDataTable - table of updated role mappings and psDict - dictionary of shib configuration values for this portal

        Public Shared Sub UpdateConfigRoleMappings(ByVal PortalID As Integer, _
          ByVal rmDataTable As DataTable, _
          ByVal psDict As System.Collections.Generic.Dictionary(Of String, String))

            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            'psDict is the original settings dictionary that you got in viewstate in ViewRoleMappings.
            'rmDataTable is the datatable that was stored in viewstate in ViewRoleMappings
            'Then retrieve that dictionary and delete all corresponding role mapping items
            'from portal settings. 


            Dim strKeyName As String
            Dim i As Integer

            For i = 1 To psDict.Count
                strKeyName = "Shib_RM_" & i
                If psDict.ContainsKey(strKeyName) Then

                    PortalController.DeletePortalSetting(PortalID, strKeyName)
                    DataCache.RemoveCache(strKeyName)

                End If
            Next

            'Then add back in whatever is left from inserts, updates, and deletes that
            'are stored in viewstate datatable rmDataTable.

            Dim strSHIBRoleName As String = ""
            Dim strSettingValue As String = ""

            For i = 0 To rmDataTable.Rows.Count - 1

                Dim strSettingName = "Shib_RM_" & i + 1

                Dim myRow As DataRow = rmDataTable.Rows(i)

                strSHIBRoleName = myRow.Field(Of String)("SHIBRoleName")

                'Cheryl:ToDo: need to read the delimiter here 
                strSettingValue = myRow.Field(Of String)("DNNRoleName") & ";"

                'on and insert you must add the PS or AD prefix
                If Left(strSHIBRoleName, 3) = "AD:" Or Left(strSHIBRoleName, 3) = "PS:" Then

                Else

                    If myRow.Field(Of String)("SHIBRoleType") = "Peoplesoft" Then
                        strSHIBRoleName = "PS:" & strSHIBRoleName
                    Else
                        strSHIBRoleName = "AD:" & strSHIBRoleName
                    End If

                End If

                strSettingValue = strSettingValue & strSHIBRoleName

                PortalController.UpdatePortalSetting(PortalID, strSettingName, strSettingValue)

            Next

        End Sub



        Public Shared ReadOnly Property DefaultProviderTypeName() As String
            Get
                Return "UF.Research.Authentication.Shibboleth.SHIB.SHIBProvider, UF.Research.Authentication.Shibboleth"
            End Get
        End Property

        Public ReadOnly Property ProviderTypeName() As String
            Get
                Return mProviderTypeName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' The current PortalID for this instance
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property PortalId() As Integer
            Get
                Return mPortalId
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Enabled property for the Shibboleth Authentication provider
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ShibbolethAuthProvider() As Boolean
            Get
                Return mEnabled
            End Get
        End Property

        ''' <summary>
        ''' Determines whether the the Shib Login provider automatically creates a new DNN user at user login (when True), or not (when False)
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AutoCreateUsers() As Boolean
            Get
                Return mAutoCreateUsers
            End Get
        End Property

        ''' <summary>
        ''' Determines whether the the Shib Login provider syncs user info to DNN from Shibboleth at user login (when True), or not (when False)
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SynchronizeRoles() As Boolean
            Get
                Return mSynchronizeRoles
            End Get
        End Property

        ''' <summary>
        ''' Determines Shib delimiter to use when parsing mapping strings.
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Delimiter() As Char
            Get
                Return mDelimiter
            End Get
        End Property


        ''' <summary>
        ''' Determines Shib logout landing page to redirect to after logging out.
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LogoutPage() As String
            Get
                Return mLogoutPage
            End Get
        End Property



        ''' <summary>
        ''' Determines Shib login landing page to redirect to for logging in.
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property LoginPage() As String
            Get
                Return mLoginPage
            End Get
        End Property

    End Class

End Namespace
