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
        public void Create(string _path, Competition competition, TimeSchedule timeSchedule)
        {
            AMEventType amEventType = new AMEventType(_path);
            int seqno = 1;
            string eventsCSV = "'db_federations.name*';'db_competitions.name*';'db_events.name*';'db_events.abbreviation';'db_events.info';'db_eventtypes.standardname*';'db_events.seqno';'db_events.medals';'db_events.status'" + Environment.NewLine;
            eventsCSV = eventsCSV +
                (from el in timeSchedule.AMEvents.Values.ToList<AMEvent>()
                 let time = el.Time
                 orderby time
                 select String.Format("{0};{1};'{2}';'{3}';'';'{4}';'{5}';'C';'2'{6}",
             competition.CompetitionDTO.FederationName,
             competition.CompetitionDTO.Name,
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

            string filename = $"{_path}\\create\\events.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(eventsCSV));
        }

        public void CreateRound(string _path, Competition competition, TimeSchedule timeSchedule)
        {
            string roundsCSV = "'db_events.name*';'db_competitions.name*';'db_federations.name*';'db_rounds.combinedeventtype';'db_rounds.seqno';'db_sessions.name';'db_rounds.name';'db_rounds.abbreviation';'db_rounds.info';'db_rounds.timescheduled';'db_rounds.heatduration';'db_rounds.timeofficial';'db_rounds.customdistance';'db_rounds.jumpoffpossible';'db_seedingalgorithms.name';'db_qualifyingalgorithms.name';'db_rounds.qualificationparameters';'db_rounds.participantsperteam';'db_rounds.participantsforteampoints';'db_teampointcalculationalgorithms.name';'db_rounds.seedingparameters';'db_pointcalculationalgorithms.name';'db_rounds.status'" + Environment.NewLine;
            roundsCSV = roundsCSV +
                (from el in timeSchedule.AMEvents.Values.ToList<AMEvent>()
                 let time = el.Time
                 orderby time
                 select String.Format("'{0}';'{1}';'{2}';'0';'1';'{3}';'*';'';'';'{4};'5';'{4}';'0';'0';'';'';'';'0';'0';'';'';'';'2'{5}",
             el.Name,
             competition.CompetitionDTO.Name,
             competition.CompetitionDTO.FederationName,
             el.Session,
             time,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\rounds.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(roundsCSV));
        }
    }
}
