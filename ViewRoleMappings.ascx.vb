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

Imports System
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Collections.Generic
Imports System.Collections
Imports System.Web.UI
Imports System.Reflection
Imports System.IO
Imports System.Data.Sql
Imports System.Data
Imports System.Data.SqlClient
Imports DotNetNuke
Imports DotNetNuke.Common
Imports DotNetNuke.Security
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Security.Membership

Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.FileSystem
Imports DotNetNuke.Security.Permissions
Imports DotNetNuke.Entities.Tabs
Imports Telerik.Web.UI
Imports Telerik.Web

Imports System.Globalization
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules.Actions
'
' UF.Resarch 
' Copyright (c) 2010 UF Research
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
' OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.


''' ----------------------------------------------------------------------------- 
''' <summary> 
''' The ViewRoleMapping class displays Role Mappings in a RadGrid
''' </summary> 
''' ----------------------------------------------------------------------------- 
''' 
Namespace UF.Research.Authentication.Shibboleth
    Partial Class ViewRoleMappings
        Inherits PortalModuleBase
        Implements IActionable

        ''' ----------------------------------------------------------------------------- 
        ''' <summary> 
        ''' OnInit runs when the control is initialized 
        ''' </summary> 
        ''' ----------------------------------------------------------------------------- 
        Protected Overloads Overrides Sub OnInit(ByVal e As EventArgs)
            MyBase.OnInit(e)
            AddHandler Load, AddressOf Page_Load
        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Try

                Dim slnPath As String = ""
                Dim reDir As String = ""
                GetSolutionPath(slnPath)

                If Me.RadGrid1.MasterTableView.EditFormSettings.UserControlName = "rmDetail.ascx" Then
                    Dim myURL As String = Globals.NavigateURL
                    Dim lastSlashPos As Integer = myURL.LastIndexOf("/")
                    myURL = Left(myURL, lastSlashPos)
                    reDir = myURL + "/DesktopModules/AuthenticationServices/Shibboleth/" + RadGrid1.MasterTableView.EditFormSettings.UserControlName
                    reDir = "~/DesktopModules/AuthenticationServices/Shibboleth/" + RadGrid1.MasterTableView.EditFormSettings.UserControlName

                    Me.RadGrid1.MasterTableView.EditFormSettings.UserControlName = reDir

                End If

                ' Obtain PortalSettings from controller
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            Catch exc As ModuleLoadException
                'Module failed to load 
                Exceptions.ProcessModuleLoadException(Me, exc)
            End Try
        End Sub


#Region "Optional Interfaces"

        ''' ----------------------------------------------------------------------------- 
        ''' <summary> 
        ''' Registers the module actions required for interfacing with the portal framework 
        ''' </summary> 
        ''' ----------------------------------------------------------------------------- 
        ReadOnly Property ModuleActions() As Actions.ModuleActionCollection Implements IActionable.ModuleActions
            Get
                Dim actions As New ModuleActionCollection()
                actions.Add(GetNextActionID(), Localization.GetString("AddContent.Action", LocalResourceFile), ModuleActionType.AddContent, "", "", EditUrl(), _
                False, SecurityAccessLevel.Edit, True, False)
                Return actions
            End Get
        End Property

#End Region


        Private Function GetRMTable() As DataTable

            If ViewState("rmDataTable") IsNot Nothing Then
                Return ViewState("rmDataTable")
                Exit Function
            End If

            Dim rmDataTable As DataTable = New DataTable("rmDataTable")
            Dim column As DataColumn
            Dim row As DataRow

            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Int32")
            column.ColumnName = "RMID"
            column.ReadOnly = False
            column.Unique = True
            column.AutoIncrement = True
            column.AutoIncrementSeed = 0
            rmDataTable.Columns.Add(column)

            ' Make the ID column the primary key column.
            Dim PrimaryKeyColumns(0) As DataColumn
            PrimaryKeyColumns(0) = rmDataTable.Columns("RMID")
            rmDataTable.PrimaryKey = PrimaryKeyColumns

            'add a column for the Portal Settings DNN Role field
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.String")
            column.ColumnName = "DNNRoleName"
            column.ReadOnly = False
            column.Unique = False

            rmDataTable.Columns.Add(column)

            'add a column for the Portal Settings DNN Role ID field
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Int32")
            column.ColumnName = "DNNRoleID"
            column.ReadOnly = False
            column.Unique = False

            rmDataTable.Columns.Add(column)

            'add a column for the Portal Settings SHIB Role Type field
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.String")
            column.ColumnName = "ShibRoleType"
            column.ReadOnly = False
            column.Unique = False

            rmDataTable.Columns.Add(column)

            'add a column for the Portal Settings Shibboleth Role Name field
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.String")
            column.ColumnName = "ShibRoleName"
            column.ReadOnly = False
            column.Unique = False

            rmDataTable.Columns.Add(column)


            'add a column for the Portal Settings Shibboleth Role ID field
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Int32")
            column.ColumnName = "ShibRoleID"
            column.ReadOnly = False
            column.Unique = False

            rmDataTable.Columns.Add(column)

            Dim strKeyName As String
            Dim strValue As String


            '''''''''''''''''''''''''
            ''get all portal settings for PortalID = 0 and read them into a datatable
            '''''''''''''''''''''''''

            '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            'get all of the portal settings for the current portal and read them into a dictionary field 

            Dim psDict As System.Collections.Generic.Dictionary(Of String, String) = _
              New System.Collections.Generic.Dictionary(Of String, String)
            ShibConfiguration.ResetConfig()
            psDict = PortalController.GetPortalSettingsDictionary(PortalId)

            ViewState("psDict") = psDict
            'this isn't the actual count of role mappings, it's the count of all portal settings
            'for this portal, which is greater than the count of role mappings,
            'but it will do to give us a starting integer counter for the number of role mappings
            'ViewState("psDictCounter") = psDict.Count

            Dim rmCount As Integer '= psDict("Shib_RoleMappingCount")

            'Go thru loop once for each role mapping
            For i = 1 To psDict.Count
                strKeyName = "Shib_RM_" & i
                If psDict.ContainsKey(strKeyName) Then
                    rmCount = i
                Else
                    Exit For
                End If
            Next
            For i = 1 To rmCount
             
                strKeyName = "Shib_RM_" & i.ToString
                'read the values from the dictionary into the datatable, row by row
                If psDict.Item(strKeyName) IsNot Nothing Then
                    strValue = psDict.Item(strKeyName)
                    row = rmDataTable.NewRow

                    row("DNNRoleID") = 0
                    row("ShibRoleID") = 0

                    'get DNN Role
                    Dim intSemiPosn As Integer = InStr(strValue, ";")
                    row("DNNRoleName") = Left(strValue, intSemiPosn - 1)

                    'get SHIB Role Name
                    row("ShibRoleName") = Mid(strValue, intSemiPosn + 1)

                    'get SHIB Role Type
                    If Left(row("SHIBRoleName"), 3) = "AD:" Then
                        row("SHIBRoleType") = "Active Directory"
                    Else
                        row("SHIBRoleType") = "Peoplesoft"
                    End If

                    rmDataTable.Rows.Add(row)

                Else
                    Exit For
                End If
            Next

            ViewState("rmDataTable") = rmDataTable

            Return rmDataTable

        End Function

        Private Sub RadGrid1_NeedDataSource(ByVal source As Object, ByVal e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles RadGrid1.NeedDataSource
            'GetRMTable()
            Dim dataView As DataView = New DataView(GetRMTable())
            dataView.Sort = "DNNRoleName ASC, SHIBRoleName ASC"
            Me.RadGrid1.DataSource = dataView

        End Sub


        Private Sub GetSolutionPath(ByRef slnPath As String)

            Dim prjSettings As ProjectSettings = New ProjectSettings
            slnPath = prjSettings.slnPath

        End Sub

        Protected Sub RadGrid1_DeleteCommand(ByVal source As Object, ByVal e As GridCommandEventArgs) Handles RadGrid1.DeleteCommand
           
            Dim ID As String = (CType(e.Item, GridDataItem)).OwnerTableView.DataKeyValues(e.Item.ItemIndex)("RMID").ToString

            Dim editedItem As GridEditableItem = CType(e.Item, GridEditableItem)

            Dim key As Integer = editedItem.GetDataKeyValue("RMID")
            Dim index As Integer = editedItem.DataSetIndex

            ' http://www.telerik.com/help/aspnet/grid/grdaccessingcellsandrows.html

            Dim rmDataTable As DataTable = GetRMTable()

            Dim myRow As DataRow = rmDataTable.Rows.Find(key)

            'delete the row
            myRow.Delete()
            rmDataTable.AcceptChanges()

            'save the change in viewstate
            ViewState("rmDataTable") = rmDataTable

        End Sub

        Private Sub RadGrid1_InsertCommand(ByVal source As Object, ByVal e As Telerik.Web.UI.GridCommandEventArgs) Handles RadGrid1.InsertCommand

            Dim MyUserControl As UserControl = _
                CType(e.Item.FindControl(GridEditFormItem.EditFormUserControlID), UserControl)

            Dim DNNRoleName As String = CType(MyUserControl.FindControl("ddlDNNRoles"), DropDownList).SelectedItem.ToString
            Dim SHIBRoleName As String = CType(MyUserControl.FindControl("txtSHIBRoleName"), TextBox).Text
            Dim SHIBType As String = CType(MyUserControl.FindControl("ddlSHIBType"), DropDownList).SelectedItem.ToString

            Dim strSHIBRoleName As String
            If SHIBType = "PeopleSoft" Then
                strSHIBRoleName = "PS:"
            Else
                strSHIBRoleName = "AD:"
            End If
            strSHIBRoleName = strSHIBRoleName & SHIBRoleName
            Dim strRM As String = DNNRoleName.ToString & ";" & strSHIBRoleName.ToString

            Dim rmDataTable As DataTable = GetRMTable()

            Dim row As DataRow = rmDataTable.NewRow()
            row.SetField(Of String)("DNNRoleName", DNNRoleName)
            row.SetField(Of String)("SHIBRoleName", strSHIBRoleName)
            row.SetField(Of String)("SHIBRoleType", SHIBType)

            rmDataTable.Rows.Add(row)

            rmDataTable.AcceptChanges()

            'save the change in viestate
            ViewState("rmDataTable") = rmDataTable

            Dim item As GridEditableItem = DirectCast(e.Item, GridEditableItem)
            item.Selected = True

        End Sub

        Protected Sub RadGrid1_UpdateCommand(ByVal source As Object, ByVal e As GridCommandEventArgs) Handles RadGrid1.UpdateCommand

            Dim MyUserControl As UserControl = _
               CType(e.Item.FindControl(GridEditFormItem.EditFormUserControlID), UserControl)

            'Dim editMan As GridEditManager = editedItem.EditManager
            Dim editedItem As GridEditableItem = CType(e.Item, GridEditableItem)
            Dim key As Integer = editedItem("RMID").Text

            Dim lblKey As Label = CType(MyUserControl.FindControl("labelRMID"), Label)
            Dim intID = e.Item.ItemIndex

            Dim DNNRoleName As String = CType(MyUserControl.FindControl("ddlDNNRoles"), DropDownList).SelectedItem.ToString
            Dim SHIBRoleName As String = CType(MyUserControl.FindControl("txtSHIBRoleName"), TextBox).Text
            Dim SHIBType As String = CType(MyUserControl.FindControl("ddlSHIBType"), DropDownList).SelectedItem.ToString

            Dim strSHIBRoleName As String
            If SHIBType = "PeopleSoft" Then
                strSHIBRoleName = "PS:"
            Else
                strSHIBRoleName = "AD:"
            End If
            strSHIBRoleName = strSHIBRoleName & SHIBRoleName
            Dim strRM As String = DNNRoleName.ToString & ";" & strSHIBRoleName.ToString

            Dim rmDataTable As DataTable = GetRMTable()
            Dim myRow As DataRow = rmDataTable.Rows.Find(key)

            'update the row
            myRow.SetField(Of String)("DNNRoleName", DNNRoleName)
            myRow.SetField(Of String)("SHIBRoleName", strSHIBRoleName)
            myRow.SetField(Of String)("SHIBRoleType", SHIBType)

            rmDataTable.AcceptChanges()

            'save the change in viestate
            ViewState("rmDataTable") = rmDataTable
            '------------------------------------------------------------------------------

        End Sub

        Protected Sub UpdateRoleMappings()

            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings

            'Store the original settings dictionary that you got in viewstate.
            'Then retrieve that dictionary and delete all corresponding role mapping items
            'from portal settings. 

            Dim rmDataTable As DataTable = GetRMTable()
            Dim psDict As System.Collections.Generic.Dictionary(Of String, String) _
             = ViewState("psDict")

            Dim strKeyName As String
            Dim i As Integer

            For i = 1 To psDict.Count
                strKeyName = "Shib_RM_" & i
                If psDict.ContainsKey(strKeyName) Then

                    PortalController.DeletePortalSetting(PortalId, strKeyName)
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

                PortalController.UpdatePortalSetting(PortalId, strSettingName, strSettingValue)

            Next

        End Sub


        Protected Sub btnUpdateRoleMappings_Click(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim rmDataTable As DataTable = GetRMTable()
            Dim psDict As System.Collections.Generic.Dictionary(Of String, String) _
             = ViewState("psDict")

            Dim _portalSettings As PortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
            Try
                'ShibConfiguration.UpdateConfig(_portalSettings.PortalId, Me.chkEnabled.Checked, Me.chkAutoCreateUsers.Checked, Me.chkSynchronizeRoles.Checked, Me.txtDelimiter.Text, Me.ddlLogoutPage.SelectedValue, Me.ddlLoginPage.SelectedValue, CType(Me.txtRoleMappingCount.Text, Integer))
                ShibConfiguration.UpdateConfigRoleMappings(_portalSettings.PortalId, rmDataTable, psDict)

                'the configuration is cached.  If you change the portal_settings table, the cache
                'will not be rebuilt and your test may fail.  If you use the settings module to 
                'update the portal_settings value, the cache will be rebuilt with the new values.
                ShibConfiguration.ResetConfig()

            Catch exc As Exception 'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try

        End Sub


        Private Sub lnkSettings_Command(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.CommandEventArgs) Handles lnkSettings.Command

        End Sub

    End Class

End Namespace