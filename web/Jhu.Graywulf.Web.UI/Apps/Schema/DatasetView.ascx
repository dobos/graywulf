﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DatasetView.ascx.cs" Inherits="Jhu.Graywulf.Web.UI.Apps.Schema.DatasetView" %>
<div class="dock-fill dock-scroll">
    <h3>
        <asp:Label ID="datasetNameLabel" runat="server" Text="Dataset Name:" />
        <asp:Label ID="datasetName" runat="server" />
    </h3>
    <p runat="server" id="urlPar">
        <asp:Label runat="server" ID="urlLabel" Text="URL:" />
        <asp:HyperLink runat="server" ID="urlLink">
            <asp:Label runat="server" ID="url" /></asp:HyperLink>
    </p>
    <asp:Panel runat="server" ID="summaryPanel">
        <h3>Summary</h3>
        <p>
            <jgwc:HtmlLabel runat="server" ID="summary" />
        </p>
    </asp:Panel>
    <asp:Panel runat="server" ID="remarksPanel">
        <h3>Remarks</h3>
        <p>
            <jgwc:HtmlLabel runat="server" ID="remarks" />
        </p>
    </asp:Panel>
    <jgwc:HtmlIncluder runat="server" ID="detailsPage" />
</div>
