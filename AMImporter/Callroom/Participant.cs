using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class Participant
    {
        public string participant_id { get; set; }
        public string participant_bib { get; set; }
        public string participant_name { get; set; }
        public string participant_team { get; set; }
        public string email { get; set; }
        public string email2 { get; set; }
        public string competition_id { get; set; }
        public List<AMImporter.Callroom.Participation> events { get; set; }

    }
}
