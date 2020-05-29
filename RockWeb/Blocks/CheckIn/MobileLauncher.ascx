<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileLauncher.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.MobileLauncher" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEditSettings" runat="server" OnSaveClick="mdEditSettings_SaveClick" Title="Mobile Launcher Settings">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEditSettings">
                        <ContentTemplate>
                            <Rock:RockListBox ID="lbDevices" runat="server" Label="Enabled Devices" Help="Set devices to consider when determining the device type, or leave blank for all." AutoPostBack="true" OnSelectedIndexChanged="lbDevices_SelectedIndexChanged" />

                            <Rock:RockDropDownList ID="ddlTheme" runat="server" Label="Theme" />
                            <Rock:RockDropDownList ID="ddlCheckinType" runat="server" Label="Check-in Configuration" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" AutoPostBack="true" />

                            <Rock:RockListBox ID="lbAreas" runat="server" Label="Check-in Areas" Help="The check-in areas that will be used for the checkin process" />

                            <Rock:PagePicker ID="ppPhoneIdentificationPage" runat="server" Label="Phone Identication Page" Help="Page to use for identifying the person by phone number. If blank the button will not be shown." />
                            <Rock:PagePicker ID="ppLoginPage" runat="server" Label="Login Page" Help="The page to use for logging in the person. If blank the login buton will not be shown" />


                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <%-- Main Panel --%>

        <div class="checkin-header">
            <h1><asp:Literal ID="lCheckinHeader" runat="server" Text="Mobile Check-in" /></h1>
        </div>

        <div class="checkin-body">

            <div class="checkin-scroll-panel">
                <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <label class="control-label">
                            <asp:Literal ID="lMessage" runat="server" Text="Before we proceed we'll need to identify you for check-in" /></label>

                        <div class="controls">

                            <Rock:BootstrapButton ID="bbtnPhoneLookup" runat="server" Text="Phone Lookup" OnClick="bbtnPhoneLookup_Click" CssClass="btn btn-default btn-block" />
                            <Rock:BootstrapButton ID="bbtnLogin" runat="server" Text="Login" OnClick="bbtnLogin_Click" CssClass="btn btn-default btn-block" />
                        </div>


                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
