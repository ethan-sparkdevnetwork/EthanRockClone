<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <h2>Mobile Check-in</h2>
            <p>Hi Ted, Great to see you back. Select the Check-in button below to get started.</p>

            <div class="alert alert-warning">This is the <b>StarkDetail</b> block prototyped as <b>Mobile Launcher</b> block. </div>

            <Rock:BootstrapButton ID="bbtnLogin" runat="server" Text="Login" OnClick="bbtnLogin_Click" CssClass="btn btn-default btn-block"></Rock:BootstrapButton>
            <Rock:BootstrapButton ID="bbtnCheckin" runat="server" Text="Check-in" OnClick="bbtnCheckin_Click" CssClass="btn btn-primary btn-block"></Rock:BootstrapButton>

            <div class="center-block margin-t-xl"><asp:Literal ID="lCheckinResultsHtml" runat="server" /></div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>