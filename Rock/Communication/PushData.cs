using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Communication
{
    internal class PushData
    {
        public int? MobilePageId { get; set; }
        public Dictionary<string, string> MobilePageQueryString { get; set; }
        public int? MobileApplicationId { get; set; }
        public string Url { get; set; }
    }
}
