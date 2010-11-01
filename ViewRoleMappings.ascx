<%@ Control Language="vb"
 AutoEventWireup="false"
 CodeBehind="ViewRoleMappings.ascx.vb"
 Explicit="true"
 Inherits="UF.Research.Authentication.Shibboleth.ViewRoleMappings" %>
 

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<div class="center">
    
<asp:panel id="pnlAddTask" runat="server">
    
    <telerik:RadGrid ID="RadGrid1" runat="server" GridLines="None" AllowPaging="false"
        AllowSorting="True" AutoGenerateColumns="False" ShowStatusBar="true" CssClass="RadGrid">
        <MasterTableView Width="100%" CommandItemDisplay="Top" DataKeyNames="RMID" EditMode="EditForms" >
             
            <Columns>
                        
                <telerik:GridEditCommandColumn UniqueName="EditCommandColumn"  ItemStyle-Width="30px"></telerik:GridEditCommandColumn>
                <telerik:GridButtonColumn UniqueName="DeleteColumn" Text="Delete" CommandName="Delete" ItemStyle-Width="30px"/>

                <telerik:GridBoundColumn UniqueName="RMID" HeaderText="RMID" DataField="RMID">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </telerik:GridBoundColumn>
                            
                <telerik:GridBoundColumn UniqueName="DNNRoleName" HeaderText="DNN Role Name" DataField="DNNRoleName">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </telerik:GridBoundColumn>

                <telerik:GridBoundColumn UniqueName="SHIBRoleType" HeaderText="SHIB Role Type" DataField="SHIBRoleType">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </telerik:GridBoundColumn> 

                <telerik:GridBoundColumn UniqueName="SHIBRoleName" HeaderText="SHIB Role Name" DataField="SHIBRoleName">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </telerik:GridBoundColumn>        
                                       
            </Columns>

                <EditFormSettings UserControlName="rmDetail.ascx" EditFormType="WebUserControl">
                    <EditColumn UniqueName="EditCommandColumn1">
                    </EditColumn>
                    
                </EditFormSettings>
            <ExpandCollapseColumn ButtonType="ImageButton" Visible="False" UniqueName="ExpandColumn">
                <HeaderStyle Width="19px"></HeaderStyle>
            </ExpandCollapseColumn>
 
        </MasterTableView>

    </telerik:RadGrid><br />
    
    <br /> 
    
    <asp:Button ID="btnUpdateRoleMappings" runat="server" Text="Update Role Mappings" 
        onclick="btnUpdateRoleMappings_Click" />
    <br />
       
</asp:panel>

<br />
<br />

<asp:LinkButton ID="lnkSettings" runat="server" style="font-weight:bold">Shibboleth Settings</asp:LinkButton>
