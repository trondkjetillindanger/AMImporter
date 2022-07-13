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

        public void CreateRound(string _path, TimeSchedule timeSchedule)
        {
            string roundsCSV = "'db_events.name*';'db_competitions.name*';'db_federations.name*';'db_rounds.combinedeventtype';'db_rounds.seqno';'db_sessions.name';'db_rounds.name';'db_rounds.abbreviation';'db_rounds.info';'db_rounds.timescheduled';'db_rounds.heatduration';'db_rounds.timeofficial';'db_rounds.lanesavailable';'db_rounds.customdistance';'db_rounds.jumpoffpossible';'db_seedingalgorithms.name';'db_qualifyingalgorithms.name';'db_rounds.qualificationparameters';'db_rounds.participantsperteam';'db_rounds.participantsforteampoints';'db_teampointcalculationalgorithms.name';'db_rounds.seedingparameters';'db_pointcalculationalgorithms.name';'db_rounds.attempts';'db_rounds.startheight';'db_rounds.intervalheight';'db_rounds.status'" + Environment.NewLine;
            roundsCSV = roundsCSV +
                (from el in timeSchedule.AMEvents.Values.ToList<AMEvent>()
                 let time = el.Time
                 orderby time
                 select String.Format("'{0}';'SI-stevne 1';'Norwegian Athletic Federation';'0';'1';'Øvingar';'*';'';'';'{1};'5';'{1}';'16';'0';'0';'';'';'';'0';'0';'';'';'';'0';'1.50';'5';'2'{2}",
             el.Name,
             time,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            int lastNewLine = roundsCSV.LastIndexOf(Environment.NewLine);
            roundsCSV = roundsCSV.Substring(0, lastNewLine - 1);

            string filename = $"{_path}\\create\\rounds.csv";
            FileUtil.CreateNewCSV(filename, roundsCSV);
        }
    }
}
