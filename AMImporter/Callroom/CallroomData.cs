using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class CallroomData
    {
        public List<AMImporter.Callroom.@Event> events  { get; set; }
        public List<AMImporter.Callroom.Participant> participants { get; set; }
    }
}
