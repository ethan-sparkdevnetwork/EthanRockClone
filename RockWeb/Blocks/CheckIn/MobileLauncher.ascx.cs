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
        Key = AttributeKey.DeviceGuidList,
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
        Key = AttributeKey.CheckinConfigurationGuid,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in configuration to use." )]

    [TextField(
        "Check-in Areas",
        Key = AttributeKey.ConfiguredGroupTypeGuids,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in areas to use." )]

    #endregion Block Attributes
    public partial class MobileLauncher : CheckInBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DeviceGuidList = "DeviceGuidList";

            public const string CheckinTheme = "CheckinTheme";

            // The Checkin Type (which is a GroupType)
            public const string CheckinConfigurationGuid = "CheckinConfigurationGuid";

            // The Checkin Areas (which are also GroupTypes)
            public const string ConfiguredGroupTypeGuids = "ConfiguredGroupTypeGuids";

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

            lbDevices.SetValues( this.GetAttributeValue( AttributeKey.DeviceGuidList ).SplitDelimitedValues().ToList() );

            BindCheckinTypes();

            ddlCheckinType.SetValue( this.GetAttributeValue( AttributeKey.CheckinConfigurationGuid ).AsGuidOrNull() );

            // Bind Areas (which are Group Types)
            var rockContext = new RockContext();
            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            //var groupTypes

            lbAreas.Items.Clear();

            pnlEditSettings.Visible = true;
            mdEditSettings.Show();
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

        #endregion

        #region Methods
        #endregion

        protected void bbtnCheckin_Click( object sender, EventArgs e )
        {
            LocalDeviceConfig.CurrentCheckinTypeId = 14; // 34;
            LocalDeviceConfig.CurrentGroupTypeIds = new List<int>() { 18, 19, 20, 21, 22 };
            LocalDeviceConfig.CurrentKioskId = 12;
            LocalDeviceConfig.CurrentTheme = "CheckinElectric";

            var checkInState = new CheckInState( LocalDeviceConfig );
            checkInState.MobileLauncherHomePage = GetAttributeValue( "core_MobileCheckInLauncherHomePage" ).AsGuidOrNull();
            checkInState.CheckIn = new CheckInStatus();
            checkInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );
            LoadDeckerFamily( checkInState, 69 ); // similar to FindFamilies.cs wf action

            // Store and save the CurrentCheckInState 
            CurrentCheckInState = checkInState;
            Session[SessionKey.CheckInState] = checkInState;

            // Now, we simulate as if we just came from the Family Select block...
            var errors = new List<string>();

            if ( ProcessSelection( null, () => false, "this.ConditionMessage" ) )
            {
                System.Diagnostics.Debug.WriteLine( "success" );
            }
        }


        private static class SessionKey
        {
            public const string CheckInState = "CheckInState";
            public const string CheckInWorkflow = "CheckInWorkflow";
        }

        private void LoadDeckerFamily( CheckInState checkInState, int famliyId )
        {
            checkInState.CheckIn.Families = new List<CheckInFamily>();

            var rockContext = new RockContext();
            var memberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );

            int personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;
            var dvInactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

            // Load the family members
            var familyMembers = memberService
                .Queryable().AsNoTracking()
                .Where( m => m.Group.GroupTypeId == familyGroupTypeId && m.GroupId == famliyId ).Select( a =>
                new
                {
                    Group = a.Group,
                    GroupId = a.GroupId,
                    Order = a.GroupRole.Order,
                    BirthYear = a.Person.BirthYear,
                    BirthMonth = a.Person.BirthMonth,
                    BirthDay = a.Person.BirthDay,
                    Gender = a.Person.Gender,
                    NickName = a.Person.NickName,
                    RecordStatusValueId = a.Person.RecordStatusValueId
                } )
                .ToList();

            // Add each family
            foreach ( int familyId in familyMembers.Select( fm => fm.GroupId ).Distinct() )
            {
                // Get each of the members for this family
                var familyMemberQry = familyMembers
                    .Where( m =>
                        m.GroupId == familyId &&
                        m.NickName != null );

                if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeople && dvInactive != null )
                {
                    familyMemberQry = familyMemberQry
                        .Where( m =>
                            m.RecordStatusValueId != dvInactive.Id );
                }

                var thisFamilyMembers = familyMemberQry.ToList();

                if ( thisFamilyMembers.Any() )
                {
                    var group = thisFamilyMembers
                        .Select( m => m.Group )
                        .FirstOrDefault();

                    var firstNames = thisFamilyMembers
                        .OrderBy( m => m.Order )
                        .ThenBy( m => m.BirthYear )
                        .ThenBy( m => m.BirthMonth )
                        .ThenBy( m => m.BirthDay )
                        .ThenBy( m => m.Gender )
                        .Select( m => m.NickName )
                        .ToList();

                    var family = new CheckInFamily();
                    family.Group = group.Clone( false );
                    family.Caption = group.ToString();
                    family.FirstNames = firstNames;
                    family.SubCaption = firstNames.AsDelimited( ", " );
                    family.Selected = true; // IMPORTANT!! Otherwise there will be no "CurrentFamily" in the CheckInState
                    checkInState.CheckIn.Families.Add( family );
                }
            }
        }

        protected void bbtnLogin_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "ReturnUrl", Request.RawUrl );

            NavigateToLinkedPage( "core_LoginPage", queryParams );
        }

        protected void mdEditSettings_SaveClick( object sender, EventArgs e )
        {

        }

        protected void ddlTheme_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {

        }
    }
}