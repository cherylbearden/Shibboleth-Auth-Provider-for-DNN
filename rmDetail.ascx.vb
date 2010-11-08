
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

Imports System
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Collections.Generic
Imports DotNetNuke
Imports DotNetNuke.Common
Imports DotNetNuke.Security
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Users
Imports System.Collections
Imports System.Web.UI
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Entities.Portals
Imports System.Reflection
Imports System.IO
Imports DotNetNuke.Services.FileSystem
Imports DotNetNuke.Security.Permissions
Imports DotNetNuke.Entities.Tabs
Imports Telerik.Web.UI
Imports Telerik.Web

Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient

Imports System.Data.Sql
Imports System.Globalization
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Modules.Actions


Namespace UF.Research.Authentication.Shibboleth

    Partial Public Class rmDetail
        'Inherits System.Web.UI.UserControl
        Inherits DotNetNuke.Entities.Modules.PortalModuleBase


        Private _dataItem As Object = Nothing

        Private _RMID As String

        Public Property RMID() As String
            Get
                'Return _Key
                Return labelRMID.Text
            End Get
            Set(ByVal value As String)
                _RMID = value
            End Set
        End Property

        'Public Overloads Sub RaisePostBackEvent(ByVal eventArgument As String) _
        '  'Implements System.Web.UI.IPostBackEventHandler.RaisePostBackEvent

        'End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs)



        End Sub


#Region "Web Form Designer generated code"

        '/ <summary>
        '/        Required method for Designer support - do not modify
        '/        the contents of this method with the code editor.
        '/ </summary>

#End Region


        Public Property DataItem() As Object
            Get
                Return Me._dataItem
            End Get
            Set(ByVal value As Object)
                Me._dataItem = value
            End Set
        End Property

        Private Sub GetRoles()

            Dim _portalSettings As PortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)

            Dim objRoles As New RoleController
            Dim arrRoles As ArrayList = objRoles.GetPortalRoles(_portalSettings.PortalId)
            Dim i As Integer = 0

            For Each rInfo As RoleInfo In arrRoles

                Me.ddlDNNRoles.Items.Add("RoleName")
                Me.ddlDNNRoles.Items(i).Value = rInfo.RoleID
                Me.ddlDNNRoles.Items(i).Text = rInfo.RoleName
                i = i + 1
            Next

            Me.ddlDNNRoles.DataBind()

        End Sub

        Private Sub GetShibTypes()

            Dim alShibType As New ArrayList(New String() {"Active Directory", "PeopleSoft"})
            Me.ddlSHIBType.DataSource = alShibType
            ddlSHIBType.DataBind()

            Dim ShibTypeValue As Object = DataBinder.Eval(DataItem, "ShibRoleType")

            If ShibTypeValue.Equals(DBNull.Value) Then
                ShibTypeValue = "Active Directory"

            End If
        End Sub

        Private Sub GetSHIBRoles()

            'Dim objConn As New SqlConnection("Data Source=dev.data.research.ufl.edu;Initial Catalog=DotNetNuke_Professional;Integrated Security=True")

            'Dim objCmd As New SqlCommand("GetAllSHIBRoles", objConn)
            'objCmd.CommandType = CommandType.Text

            'objCmd.CommandText = "Select RoleID, RoleName from dbo.dnn_SHIBRoles"

            'objConn.Open()
            'Dim DR As SqlDataReader = objCmd.ExecuteReader()

            'Dim dt As New DataTable
            'dt.Load(DR)

            'For i = 0 To dt.Rows.Count - 1

            '    Me.ddlSHIBRoles.Items.Add(dt.Rows(i).Field(Of String)("RoleName"))
            '    Me.ddlSHIBRoles.Items(i).Value = dt.Rows(i).Field(Of Integer)("RoleID")
            '    Me.ddlSHIBRoles.Items(i).Text = dt.Rows(i).Field(Of String)("RoleName")
            'Next
            'Me.ddlSHIBRoles.Items.FindByText(Me.txtSHIBRoleName.Text).Selected = True
            'cmbType.SelectedValue = GridView1.DataKeys[e.Row.RowIndex].Values[1].ToString();
            'ddlSHIBRoles.SelectedValue = 2

        End Sub

        Private Sub Page_DataBinding(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.DataBinding

            GetRoles()

            GetShibTypes()

            'GetSHIBRoles()

        End Sub

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
            Me.btnCancel.Text = Localization.GetString("btnCancel", LocalResourceFile)
            Me.btnInsert.Text = Localization.GetString("btnInsert", LocalResourceFile)
            Me.btnUpdate.Text = Localization.GetString("btnUpdate.Header", LocalResourceFile)
        End Sub

        Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender

            If Me.txtDNNRoleName.Text <> "" Then
                Me.ddlDNNRoles.Items.FindByText(Me.txtDNNRoleName.Text).Selected = True
            End If

            Me.btnCancel.Text = Localization.GetString("btnCancel", LocalResourceFile)
            Me.btnInsert.Text = Localization.GetString("btnInsert", LocalResourceFile)
            Me.btnUpdate.Text = Localization.GetString("btnUpdate.Header", LocalResourceFile)

        End Sub


        Private Sub btnInsert_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnInsert.Click
            Dim intDNNRoleID As Integer = Me.ddlDNNRoles.SelectedItem.Value

            Dim strDNNRoleName As String = Me.ddlDNNRoles.SelectedItem.ToString
            Dim strSHIBRoleName As String
            If Me.ddlSHIBType.SelectedItem.ToString = "PeopleSoft" Then
                strSHIBRoleName = "PS:"
            Else
                strSHIBRoleName = "AD:"
            End If
            strSHIBRoleName = strSHIBRoleName & Me.txtSHIBRoleName.Text

            Dim strRM As String = strDNNRoleName.ToString & ";" & strSHIBRoleName.ToString

        End Sub

        Private Sub btnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdate.Click
            Dim strDNNRoleName As String = Me.ddlDNNRoles.SelectedItem.ToString

            Dim strSHIBRoleName As String = Me.txtSHIBRoleName.Text

            If Me.ddlSHIBType.SelectedItem.ToString = "PeopleSoft" Then
                strSHIBRoleName = "PS:"
            Else
                strSHIBRoleName = "AD:"
            End If
            strSHIBRoleName = strSHIBRoleName & Me.txtSHIBRoleName.Text

            Dim strRM As String = strDNNRoleName.ToString & ";" & strSHIBRoleName.ToString

            Dim strSettingName As String = Me.labelRMID.Text

            Dim editedItem As GridEditableItem = Me.Parent.NamingContainer

        End Sub

        Private Sub txtSHIBRoleName_DataBinding(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSHIBRoleName.DataBinding

        End Sub

        Private Sub txtSHIBRoleName_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSHIBRoleName.PreRender
            If Left(txtSHIBRoleName.Text, 3) = "PS:" Or Left(txtSHIBRoleName.Text, 3) = "AD:" Then

                txtSHIBRoleName.Text = Mid(txtSHIBRoleName.Text, 4)

            End If

        End Sub

        Private Sub ddlSHIBType_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlSHIBType.PreRender

            If Left(txtSHIBRoleName.Text, 3) = "PS:" Then
                'ddlSHIBType.SelectedItem.Text = "PeopleSoft"
                ddlSHIBType.SelectedIndex = 1
            Else
                'ddlSHIBType.SelectedItem.Text = "Active Directory"
                ddlSHIBType.SelectedIndex = 0
            End If
        End Sub

    End Class
End Namespace
