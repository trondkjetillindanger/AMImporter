using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class AMRecordDetailDTO
    {
        public string Time { get; set; }
        public string Wind { get; set; }
        public string Date { get; set; }
        public bool? IllegalWind { get; set; }
    }

    public class AMRecordDTO
    {
        public AMRecordDetailDTO SB { get; set; } = new AMRecordDetailDTO();
        public AMRecordDetailDTO PB { get; set; } = new AMRecordDetailDTO();
    }
}
