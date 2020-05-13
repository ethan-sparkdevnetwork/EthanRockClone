using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Import to POST to ~/api/Interactions/Import" )]
    public class InteractionsImport
    {
        /// <summary>
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>
        /// The interactions.
        /// </value>
        public List<InteractionImport> Interactions { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Interactions.Count} Interactions";
        }
    }
}
