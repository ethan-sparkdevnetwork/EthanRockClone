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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Stark Detail" )]
    [Category( "Utility" )]
    [Description( "Template block for developers to use to start a new detail block." )]

    #region Block Attributes

    [BooleanField(
        "Show Email Address",
        Key = AttributeKey.ShowEmailAddress,
        Description = "Should the email address be shown?",
        DefaultBooleanValue = true,
        Order = 1 )]

    [EmailField(
        "Email",
        Key = AttributeKey.Email,
        Description = "The Email address to show.",
        DefaultValue = "ted@rocksolidchurchdemo.com",
        Order = 2 )]

    [LinkedPage(
        "Mobile Check-in Launcher Home Page",
        Key = "core_MobileCheckInLauncherHomePage",
        Description = "",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Login Page",
        Key = "core_LoginPage",
        Description = "",
        IsRequired = false,
        Order = 3 )]

    [LinkedPage(
        "Check-in Select Action Page",
        Key = "core_CheckInSelectActionPage",
        Description = "",
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes
    public partial class StarkDetail : CheckInBlock // Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            if ( !Page.IsPostBack )
            {
                if ( String.IsNullOrWhiteSpace( PageParameter( "theme" ) ) )
                {
                    // if the site's theme doesn't match the configured theme, reload the page with the theme parameter so that the correct theme gets loaded and the theme cookie gets set
                    Dictionary<string, string> themeParameters = new Dictionary<string, string>();
                    themeParameters.Add( "theme", "CheckinElectric" );

                    NavigateToCurrentPageReference( themeParameters );
                }
                DisplayQRCode();
            }

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

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        public void DisplayQRCode()
        {
            var attendanceCheckInSessionCookie = this.Page.Request.Cookies["AttendanceCheckInSession"];

            if ( attendanceCheckInSessionCookie != null )
            {
                lCheckinResultsHtml.Text += "<img class='img-responsive center-block' src='https://api.qrserver.com/v1/create-qr-code/?size=250x250&amp;data=" + attendanceCheckInSessionCookie.Value + "' alt=''>";
            }
        }

        protected void bbtnCheckin_Click( object sender, EventArgs e )
        {
            LocalDeviceConfig.CurrentCheckinTypeId = 14; // 34;
            LocalDeviceConfig.CurrentGroupTypeIds = new List<int>() { 18, 19, 20, 21, 22 };
            LocalDeviceConfig.CurrentKioskId = 12;
            LocalDeviceConfig.CurrentTheme = "CheckinElectric";

            var checkInState = new CheckInState( LocalDeviceConfig );
            checkInState.MobilleLauncherHomePage = GetAttributeValue( "core_MobileCheckInLauncherHomePage" ).AsGuid();
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
                new {
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
    }
}