using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class AMEvent
    {
        public string Session { get; set; }
        public string Name { get; set; }
        public string SAEventCategoryName { get; set; }
        public string Time { get; set; }
        public List<string> AgeCategory { get; set; }
        public string EventTypeStandardName { get; set; }
        public string SAEventName { get; set; }
    }
}
