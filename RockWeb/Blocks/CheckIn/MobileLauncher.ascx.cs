// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Workflow.Action.CheckIn;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Mobile Launcher" )]
    [Category( "Check-in" )]
    [Description( "Launch page for checking in from a person's mobile device." )]

    #region Block Attributes

    [TextField(
        "Devices",
        Key = AttributeKey.DeviceIdList,
        Category = "CustomSetting",
        Description = "The devices to consider for determining the kiosk. No value would consider all devices in the system. If none are selected, then use all devices.",
        IsRequired = false,
        Order = 1 )]

    [TextField(
        "Check-in Theme",
        Key = AttributeKey.CheckinTheme,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in theme to pass to the check-in pages.",
        Order = 2
        )]

    [TextField(
        "Check-in Configuration",
        Key = AttributeKey.CheckinConfiguration_GroupTypeId,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in configuration to use." )]

    [TextField(
        "Check-in Areas",
        Key = AttributeKey.ConfiguredAreas_GroupTypeIds,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in areas to use." )]

    #endregion Block Attributes
    public partial class MobileLauncher : CheckInBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DeviceIdList = "DeviceIdList";

            public const string CheckinTheme = "CheckinTheme";

            /// <summary>
            /// The checkin configuration unique identifier (which is a GroupType)
            /// </summary>
            public const string CheckinConfiguration_GroupTypeId = "CheckinConfiguration_GroupTypeId";

            /// <summary>
            /// The configured Checkin Areas (which are really Group Types)
            /// </summary>
            public const string ConfiguredAreas_GroupTypeIds = "ConfiguredAreas_GroupTypeIds";

            public const string PhoneIdentificationPage = "PhoneIdentificationPage";

            public const string LoginPage = "LoginPage";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="T:Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="T:System.Boolean" /> flag that indicates if the user can configure the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="T:System.Boolean" /> flag that indicates if the user can edit the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> containing all the icon <see cref="T:System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canEdit )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Settings";
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-edit" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbEdit_Click( object sender, EventArgs e )
        {
            ShowSettings();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        private void ShowSettings()
        {
            BindThemes();

            ddlTheme.SetValue( this.GetAttributeValue( AttributeKey.CheckinTheme ).ToLower() );

            BindDevices();

            var selectedDevicesIds = this.GetAttributeValue( AttributeKey.DeviceIdList ).SplitDelimitedValues().AsIntegerList();

            lbDevices.SetValues( selectedDevicesIds );

            BindCheckinTypes();

            var selectedCheckinType = GroupTypeCache.Get( this.GetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId ).AsInteger() );

            ddlCheckinType.SetValue( selectedCheckinType );

            var configuredAreas_GroupTypeIds = this.GetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds ).SplitDelimitedValues().AsIntegerList();

            // Bind Areas (which are Group Types)
            BindAreas( selectedCheckinType, selectedDevicesIds );

            lbAreas.SetValues( configuredAreas_GroupTypeIds );

            pnlEditSettings.Visible = true;
            mdEditSettings.Show();
        }

        /// <summary>
        /// Binds the group types (checkin areas)
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void BindAreas( GroupTypeCache selectedCheckinType, IEnumerable<int> selectedDeviceIds )
        {
            // keep any currently selected areas after we repopulate areas for the selectedCheckinType
            var selectedAreaIds = lbAreas.SelectedValues.AsIntegerList();

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            // Get all locations (and their children) associated with the select devices
            List<int> locationIds;
            if ( selectedDeviceIds.Any() )
            {
                locationIds = locationService
                   .GetByDevice( selectedDeviceIds, true )
                   .Select( l => l.Id )
                   .ToList();
            }
            else
            {
                locationIds = locationService
                   .GetAllDeviceLocations( true )
                   .Select( l => l.Id )
                   .ToList();
            }

            var locationGroupTypeIds = groupLocationService
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.LocationId ) )
                .Where( gl => gl.Group.GroupType.TakesAttendance )
                .Select( gl => gl.Group.GroupTypeId )
                .Distinct()
                .ToList();

            lbAreas.SetValues( locationGroupTypeIds );
        }

        /// <summary>
        /// Binds the checkin types.
        /// </summary>
        private void BindCheckinTypes()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new GroupTypeService( rockContext );

                var checkinTemplateTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

                var checkinTypes = groupTypeService
                    .Queryable().AsNoTracking()
                    .Where( t => t.GroupTypePurposeValueId.HasValue && t.GroupTypePurposeValueId == checkinTemplateTypeId )
                    .OrderBy( t => t.Name )
                    .Select( t => new
                    {
                        t.Name,
                        t.Guid
                    } )
                    .ToList();

                ddlCheckinType.Items.Clear();
                ddlCheckinType.Items.AddRange( checkinTypes.Select( a => new ListItem( a.Name, a.Guid.ToString() ) ).ToArray() );
            }
        }

        /// <summary>
        /// Binds the themes.
        /// </summary>
        private void BindThemes()
        {
            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name.ToLower() ) );
            }
        }

        /// <summary>
        /// Binds the devices.
        /// </summary>
        private void BindDevices()
        {
            int? kioskDeviceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );

            var rockContext = new RockContext();

            DeviceService deviceService = new DeviceService( rockContext );
            var devices = deviceService.Queryable().AsNoTracking().Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Guid,
                    a.Name
                } ).ToList();

            lbDevices.Items.Clear();
            lbDevices.Items.AddRange( devices.Select( a => new ListItem( a.Name, a.Guid.ToString() ) ).ToArray() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the bbtnCheckin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnCheckin_Click( object sender, EventArgs e )
        {
            var configuredCheckinTypeId = this.GetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId ).AsIntegerOrNull();

            LocalDeviceConfig.CurrentCheckinTypeId = configuredCheckinTypeId;
            LocalDeviceConfig.CurrentGroupTypeIds = this.GetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds ).SplitDelimitedValues().AsIntegerList();

            LocalDeviceConfig.CurrentTheme = this.GetAttributeValue( AttributeKey.CheckinTheme );

            // TODO: Detemine device by geolocation and block configuration
            //LocalDeviceConfig.CurrentKioskId = this.GetAttributeValue( AttributeKey.)

            var checkInState = new CheckInState( LocalDeviceConfig );
            checkInState.MobileLauncherHomePage = GetAttributeValue( "core_MobileCheckInLauncherHomePage" ).AsGuidOrNull();
            checkInState.CheckIn = new CheckInStatus();
            checkInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );
            //FindFamilies
            //LoadDeckerFamily( checkInState, 69 ); // similar to FindFamilies.cs wf action

            // Store and save the CurrentCheckInState 
            //CurrentCheckInState = checkInState;
            //Session[SessionKey.CheckInState] = checkInState;

            // Now, we simulate as if we just came from the Family Select block...
            var errors = new List<string>();

            if ( ProcessSelection( null, () => false, "this.ConditionMessage" ) )
            {
                System.Diagnostics.Debug.WriteLine( "success" );
            }
        }

        #endregion

        #region Methods

        #endregion

        /// <summary>
        /// Handles the Click event of the bbtnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnLogin_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "ReturnUrl", Request.RawUrl );

            NavigateToLinkedPage( "core_LoginPage", queryParams );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId, ddlCheckinType.SelectedValue );
            this.SetAttributeValue( AttributeKey.CheckinTheme, ddlTheme.SelectedValue );
            this.SetAttributeValue( AttributeKey.DeviceIdList, lbDevices.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds, lbAreas.SelectedValues.AsDelimited( "," ) );
            this.SaveAttributeValues();
            mdEditSettings.Hide();
            pnlEditSettings.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedCheckinType = GroupTypeCache.Get( ddlCheckinType.SelectedValue.AsInteger() );
            var selectedDeviceIds = lbDevices.SelectedValuesAsInt;
            BindAreas( selectedCheckinType, selectedDeviceIds );
        }
    }
}