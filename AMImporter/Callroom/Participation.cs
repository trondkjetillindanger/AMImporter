using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class Participation
    {
        public int event_id { get; set; }
        public bool callroomstatus { get; set; }
        public string callroomstatus_modificationtime { get; set; }
        public string year_best { get; set; }
        public string personal_best { get; set; }
    }
}
