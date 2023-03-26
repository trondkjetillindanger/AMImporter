using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class Event
    {
        public string @event { get; set; }
        public string agegroup { get; set; }
        public string event_id { get; set; }
        public string competition_id { get; set; }
        public string event_time { get; set; }
        public string callroomregistration_open_time { get; set; }
        public string callroomregistration_closed_time { get; set; }
        public string callroomregistration_time { get; set; }
        public string enterStadium_time { get; set; }

    }
}
