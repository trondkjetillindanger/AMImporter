using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AMImporter.Timeschedule;

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
                 let time = el.Time.Split('|').First()
                 orderby time
                 select String.Format("'{0}';'{1}';'{2}';'{3}';'';'{4}';'{5}';'C';'2'{6}",
             competition.CompetitionDTO.FederationName,
             competition.CompetitionDTO.Name,
             el.Name,
             "",//amEventType.GetAMEventTypeAbbreviation(el.EventTypeStandardName),
             el.EventTypeStandardName,
             el.Id,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\events.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(eventsCSV), Encoding.GetEncoding("iso-8859-1"));
        }

        public void CreateRound(string _path, Competition competition, TimeSchedule timeSchedule)
        {
            string roundsCSV = "'db_events.name*';'db_competitions.name*';'db_federations.name*';'db_rounds.combinedeventtype';'db_rounds.seqno';'db_sessions.name';'db_rounds.name';'db_rounds.abbreviation';'db_rounds.info';'db_rounds.timescheduled';'db_rounds.heatduration';'db_rounds.timeofficial';'db_rounds.customdistance';'db_rounds.jumpoffpossible';'db_seedingalgorithms.name';'db_qualifyingalgorithms.name';'db_rounds.qualificationparameters';'db_rounds.participantsperteam';'db_rounds.participantsforteampoints';'db_teampointcalculationalgorithms.name';'db_rounds.seedingparameters';'db_pointcalculationalgorithms.name';'db_rounds.status'" + Environment.NewLine;
            roundsCSV = roundsCSV +
                (from el in timeSchedule.AMEvents.Values.ToList<AMEvent>()
                 let time = el.Time.Split('|').First()
                 let roundname = el.Time.Split('|')[1]??"*"
                 let seqno = el.Time.Split('|')[2]??"1"
                 //let seqno = ExtractSeqno(roundname) // Extract number in parentheses
                 //let roundnameWithoutSeqno = RemoveSeqno(roundname, seqno)
                 orderby time.First()
                 select String.Format("'{0}';'{1}';'{2}';'0';'{7}';'{3}';'{4}';'';'';'{5}';'5';'{5}';'0';'0';'';'';'';'0';'0';'';'';'';'2'{6}",
             el.Name,
             competition.CompetitionDTO.Name,
             competition.CompetitionDTO.FederationName,
             el.Session,
             roundname,
             time,
             Environment.NewLine,
             seqno))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\rounds.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(roundsCSV), Encoding.GetEncoding("iso-8859-1"));
        }

        private int ExtractSeqno(string roundname)
        {
            Regex regex = new Regex(@"\((\d+)\)");
            Match match = regex.Match(roundname);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value;
                if (int.TryParse(numberString, out int seqno))
                {
                    return seqno;
                }
            }

            return -1; 
        }

        // Helper method to remove the number within parentheses from roundname
        private string RemoveSeqno(string roundname, int seqno)
        {
            if (seqno != -1)
            {
                return Regex.Replace(roundname, @"\(\d+\)", "").Trim();
            }
            else
            {
                return roundname;
            }
        }
    }
}
