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

using Rock.WebStartup;

namespace Rock
{
    /// <summary>
    /// Initializer that runs prior to RockWeb's Global.Application_Start. (see comments on PreApplicationStartMethod in AssemblyInfo.cs)
    /// This calls <seealso cref="RockApplicationStartupHelper.RunApplicationStartup"/> to take care of most for all startup logic relating to the web project.
    /// </summary>
    public static class AssemblyInitializer
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            System.Diagnostics.Debugger.Launch();
            System.Diagnostics.Debugger.Break();
            RockApplicationStartupHelper.RunApplicationStartup();
        }
    }
}
