using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class AMRecordDTO
    {
        public string Time { get; set; }
        public string Wind { get; set; }
        public string Date { get; set; }
        public bool? IllegalWind { get; set; }
    }
}
