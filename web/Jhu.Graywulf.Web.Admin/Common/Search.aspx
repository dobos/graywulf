﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Jhu.Graywulf.Web.Admin.Common.Search"
    MasterPageFile="~/Admin.master" %>

<asp:Content runat="server" ContentPlaceHolderID="toolbar">
    &nbsp;
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="middle">
    <div class="dock-left dock-container" style="width: 500px; margin-right: 8px">
        <%--<asp:UpdatePanel runat="server" class="dock-fill dock-container" UpdateMode="Always">
            <ContentTemplate>--%>
                <div class="dock-top">
                    <jgwc:MultiViewTabHeader ID="MainTabs" runat="server" MultiViewID="MainMultiViewTabs"
                        CssClass="dock-top" />
                </div>
                <div class="TabFrame dock-fill dock-container">
                    <div class="dock-bottom">
                        <p class="FormButtons">
                            <div class="btn-group">
                                <asp:Button ID="ok" runat="server" CssClass="btn btn-default" Text="Search"
                                    OnClick="Ok_Click" />
                            </div>
                        </p>
                    </div>
                    <div class="dock-fill dock-container dock-scroll">
                        <asp:MultiView runat="server" ID="MainMultiViewTabs" ActiveViewIndex="0">
                            <jgwc:TabView ID="tabViewSearch" runat="server" Text="Search">
                                <table class="DetailsForm">
                                    <td class="FormLabel">
                                        <asp:Label ID="nameLabel" runat="server" Text="Name:" />
                                    </td>
                                    <td class="FormField">
                                        <asp:TextBox ID="name" runat="server" />
                                    </td>
                                </table>
                            </jgwc:TabView>
                        </asp:MultiView>
                    </div>
                </div>
            <%--</ContentTemplate>
        </asp:UpdatePanel>--%>
    </div>
    <div class="dock-fill dock-container">
        <%--<asp:UpdatePanel runat="server" class="dock-fill dock-container">
            <ContentTemplate>--%>
                <jgwac:EntityList runat="server" Visible="false" ID="SearchResultList">
                    <Columns>
                        <asp:BoundField DataField="DeploymentState" HeaderText="Deployment State" />
                        <asp:BoundField DataField="RunningState" HeaderText="Running State" />
                    </Columns>
                </jgwac:EntityList>
            <%--</ContentTemplate>
        </asp:UpdatePanel>--%>
    </div>
</asp:Content>
