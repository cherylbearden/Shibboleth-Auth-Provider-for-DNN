<%@ Control Language="vb"
 AutoEventWireup="false"
 CodeBehind="ViewRoleMappings.ascx.vb"
 Explicit="true"
 Inherits="UF.Research.Authentication.Shibboleth.ViewRoleMappings" %>
 

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<%@ Register Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"
    TagPrefix="cc1" %>

<div class="center3">
    
     <!-- <cc1:dnngrid runat="server"></cc1:dnngrid>   -->
    
<asp:panel id="pnlAddShibHdrVar" runat="server">
 
    <p>
    <asp:CheckBox ID="chkShibEnabled" runat="server" Text="Shibboleth Enabled?" enabled="false"/>
    <asp:CheckBox ID="chkShibSimulation" runat="server" Text="Shibboleth Simulation?" enabled="false"/>
    </p>
    
    <p>
        <asp:Label ID="lblShibUserName" runat="server" Text="Shibboleth UserName Variable: "></asp:Label>
        <asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
        <br /> <br />
        <asp:Button ID="btnUpdateUserName" runat="server" Text="Update Shibboleth UserName Variable" onclick="btnUpdateUserName_Click"/>
    </p>
    
    <p></p>
    
    <cc1:dnngrid ID="RadGrid3" runat="server" GridLines="None" AllowPaging="false"
        AllowSorting="True" AutoGenerateColumns="False" ShowStatusBar="true" CssClass="RadGrid">
        <MasterTableView Width="100%" CommandItemDisplay="Top" DataKeyNames="RMID" EditMode="EditForms" >
             
            <Columns>
                        
                <Telerik:GridEditCommandColumn UniqueName="EditCommandColumn"  ItemStyle-Width="30px"></Telerik:GridEditCommandColumn>
                <Telerik:GridButtonColumn UniqueName="DeleteColumn" Text="Delete" CommandName="Delete" ItemStyle-Width="30px"/>

                <cc1:dnnGridBoundColumn UniqueName="RMID" DataField="RMID" visible="false">
                    <HeaderStyle Width="10px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>
                            
                <cc1:dnnGridBoundColumn UniqueName="SHIBHdrVarName"  DataField="SHIBHdrVarName">
                    <HeaderStyle Width="30px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>

                <cc1:dnnGridBoundColumn UniqueName="SHIBHdrVarDelim"  DataField="SHIBHdrVarDelim">
                    <HeaderStyle Width="30px"></HeaderStyle>
                </cc1:dnnGridBoundColumn> 
                     
            </Columns>

                <EditFormSettings UserControlName="hvDetail.ascx" EditFormType="WebUserControl">
                    <EditColumn UniqueName="EditCommandColumn1">
                    </EditColumn>
                    
                </EditFormSettings>
            <ExpandCollapseColumn ButtonType="ImageButton" Visible="False" UniqueName="ExpandColumn">
                <HeaderStyle Width="19px"></HeaderStyle>
            </ExpandCollapseColumn>
 
        </MasterTableView>

    </cc1:dnngrid><br />


    <br /> 

    <asp:Button ID="btnUpdateShibHdrVar" runat="server" Text="Update Shibboleth Header Variable" 
        onclick="btnUpdateShibHdrVar_Click" />
    <br />
    <br />   
   </asp:panel>
      
</div>

<div class="center">
    
<asp:panel id="pnlAddTask" runat="server">

    <cc1:dnngrid ID="RadGrid1" runat="server" GridLines="None" AllowPaging="false"
        AllowSorting="True" AutoGenerateColumns="False" ShowStatusBar="true" CssClass="RadGrid">
        <MasterTableView Width="100%" CommandItemDisplay="Top" DataKeyNames="RMID" EditMode="EditForms" >
             
            <Columns>
                        
                <Telerik:GridEditCommandColumn UniqueName="EditCommandColumn"  ItemStyle-Width="30px"></Telerik:GridEditCommandColumn>
                <Telerik:GridButtonColumn UniqueName="DeleteColumn" Text="Delete" CommandName="Delete" ItemStyle-Width="30px"/>

                <cc1:dnnGridBoundColumn UniqueName="RMID" DataField="RMID" visible="false">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>
                            
                <cc1:dnnGridBoundColumn UniqueName="DNNRoleName"  DataField="DNNRoleName">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>

                <cc1:dnnGridBoundColumn UniqueName="SHIBRoleType"  DataField="SHIBRoleType">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn> 

                     
            <cc1:dnnGridTemplateColumn DataField="ShibRoleName" HeaderText="ShibRoleName" 
                  UniqueName="ShibRoleName">
                 <ItemTemplate>
                 <asp:TextBox ID="ShibRoleName" runat="server" Text='<%# Eval("ShibRoleName") %>'
                 TextMode="MultiLine" width="500px" readonly="true"></asp:TextBox>
                 </ItemTemplate>
            </cc1:dnnGridTemplateColumn>           
                              
            </Columns>

                <EditFormSettings UserControlName="rmDetail.ascx" EditFormType="WebUserControl">
                    <EditColumn UniqueName="EditCommandColumn1">
                    </EditColumn>
                    
                </EditFormSettings>
            <ExpandCollapseColumn ButtonType="ImageButton" Visible="False" UniqueName="ExpandColumn">
                <HeaderStyle Width="19px"></HeaderStyle>
            </ExpandCollapseColumn>
 
        </MasterTableView>

    </cc1:dnngrid><br />


    <br /> 

    <asp:Button ID="btnUpdateRoleMappings" runat="server" Text="Update Role Mappings" 
        onclick="btnUpdateRoleMappings_Click" />
    <br />
       
   </asp:panel>
      
</div>


<div class="center2">
    
<br />

<asp:panel id="pnlAddUserAttribute" runat="server">
    
    <cc1:dnngrid ID="RadGrid2" runat="server" GridLines="None" AllowPaging="false"
        AllowSorting="True" AutoGenerateColumns="False" ShowStatusBar="true" CssClass="RadGrid">
        <MasterTableView Width="100%" CommandItemDisplay="Top" DataKeyNames="RMID" EditMode="EditForms" >
             
            <Columns>
                        
                <Telerik:GridEditCommandColumn UniqueName="EditCommandColumn"  ItemStyle-Width="30px"></Telerik:GridEditCommandColumn>
                <Telerik:GridButtonColumn UniqueName="DeleteColumn" Text="Delete" CommandName="Delete" ItemStyle-Width="30px"/>

                <cc1:dnnGridBoundColumn UniqueName="RMID"  DataField="RMID" visible="false">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>
                            
                <cc1:dnnGridBoundColumn UniqueName="Type"  DataField="Type">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>

                <cc1:dnnGridBoundColumn UniqueName="DNNProperty"  DataField="DNNProperty">
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn> 

                <cc1:dnnGridBoundColumn UniqueName="Source"  DataField="Source" >
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn>        
                     
                             
                <cc1:dnnGridTemplateColumn  DataField="uaOverwrite"  HeaderText="Overwrite" UniqueName="uaOverwrite">  
                        <HeaderStyle Width="20px"></HeaderStyle>
                        <ItemTemplate >  
                            <asp:CheckBox ID="uaChkOverwrite" runat="server" enabled="false"/>  
                        </ItemTemplate>  
                </cc1:dnnGridTemplateColumn>  
                 
                
                <cc1:dnnGridBoundColumn UniqueName="Overwrite"  DataField="Overwrite" visible=false>
                    <HeaderStyle Width="20px"></HeaderStyle>
                </cc1:dnnGridBoundColumn> 
                           
                                       
            </Columns>

                <EditFormSettings UserControlName="uaDetail.ascx" EditFormType="WebUserControl">
                    <EditColumn UniqueName="EditCommandColumn1">
                    </EditColumn>
                    
                </EditFormSettings>
            <ExpandCollapseColumn ButtonType="ImageButton" Visible="False" UniqueName="ExpandColumn">
                <HeaderStyle Width="19px"></HeaderStyle>
            </ExpandCollapseColumn>
 
        </MasterTableView>

    </cc1:dnngrid><br />
    
    <br /> 
    
    <asp:Button ID="btnUpdateAttributes" runat="server" Text="Update User Attributes" 
        onclick="btnUpdateAttributes_Click" />
    <br />
       
</asp:panel>

<br />

</div>

