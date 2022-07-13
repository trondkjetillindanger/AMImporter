using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMImporter
{
    public class Event
    {
        public void Create(string _path, TimeSchedule timeSchedule)
        {
            AMEventType amEventType = new AMEventType(_path);
            int seqno = 1;
            string eventsCSV = "'db_federations.name*';'db_competitions.name*';'db_events.name*';'db_events.abbreviation';'db_events.info';'db_eventtypes.standardname*';'db_events.seqno';'db_events.medals';'db_events.status'" + Environment.NewLine;
            eventsCSV = eventsCSV +
                (from el in timeSchedule.AMEvents.Values.ToList<AMEvent>()
                 let time = el.Time
                 orderby time
                 select String.Format("'Norwegian Athletic Federation';'SI-stevne 1';'{0}';'{1}';'';'{2}';'{3}';'C';'2'{4}",
             el.Name,
             amEventType.GetAMEventTypeAbbreviation(el.EventTypeStandardName),
             el.EventTypeStandardName,
             seqno++,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            int lastNewLine = eventsCSV.LastIndexOf(Environment.NewLine);
            eventsCSV = eventsCSV.Substring(0, lastNewLine - 1);

            string filename = $"{_path}\\create\\events.csv";
            FileUtil.CreateNewCSV(filename, eventsCSV);
        }
    }
}
