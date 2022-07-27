using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMImporter
{
    public class Athlete
    {
        private XElement root = null;
        private Team teams = null;
        public Athlete(XElement _root, Team _teams)
        {
            root = _root;
            teams = _teams;
        }

        public List<string> GetTeams()
        {
            //var teamEnum = from el in root.Descendants("Person") select new { teamName = el.Attribute("clubName").Value, hasclub = teams.Contains(el.Attribute("clubName").Value) };
            return (from el in root.Descendants("Person")
                           select el.Attribute("clubName").Value.TrimEnd(' ')).Distinct().ToList<string>();
        }

        public void Create(string _path)
        {
            string athletesCSV = "'db_licenses.licensenumber*';'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.middlename';'db_athletes.gender';'db_athletes.birthdate';'db_countries.name';'db_teams.name';'db_federations.name'" + Environment.NewLine;
            athletesCSV = athletesCSV+
                (from el in root.Descendants("Person")
                 let name = (string?)el.Element("Name")?.Element("Family")
                 orderby name
                 select String.Format("'{0}';'{1}';'{2}';'{3}';'{4}';'{5}-{6}-{7}';'Norway';'{8}';'Norwegian Athletic Federation'{9}",
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' ') + (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' ') + (string?)el.Attribute("clubName").Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' '),
             "",
             (string?)el.Attribute("sex") == "K" ? "W" : "M",
             (string?)el.Element("BirthDate")?.Attribute("year"),
             (string)((int?)el.Element("BirthDate")?.Attribute("month") ?? 1).ToString("D2"),
             (string)((int?)el.Element("BirthDate")?.Attribute("day") ?? 1).ToString("D2"),
             (string?)el.Attribute("clubName").Value.TrimEnd(' '),
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\athletes.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(athletesCSV));
        }

        public void CreateLicense(string _path)
        {
            string licensesCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate';'db_teams.name';'db_federations.name';'db_licenses.licensenumber*';'db_licenses.bib'" + Environment.NewLine;
            licensesCSV = licensesCSV +
                (from el in root.Descendants("Person")
                 let name = (string?)el.Element("Name")?.Element("Family")
                 orderby name
                 select String.Format("'{0}';'{1}';'{2}-{3}-{4}';'{5}';'Norwegian Athletic Federation';'{6}';''{7}",
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' '),
             (string?)el.Element("BirthDate")?.Attribute("year"),
             (string)((int?)el.Element("BirthDate")?.Attribute("month") ?? 1).ToString("D2"),
             (string)((int?)el.Element("BirthDate")?.Attribute("day") ?? 1).ToString("D2"),
             (string?)el.Attribute("clubName").Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' ') + (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' ') + (string?)el.Attribute("clubName").Value.TrimEnd(' '),
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\licenses.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(licensesCSV));
        }

        public void CreateParticipation(string _path, TimeSchedule timeSchedule)
        {

            string participationCSV = "'db_athletes.firstname';'db_athletes.lastname';'db_licenses.licensenumber';'db_events.name';'db_rounds.name';'db_competitions.name';'db_categories.name';'db_federations.name'" + Environment.NewLine;
            participationCSV = participationCSV +
                (from el in root.Descendants("Competitor")
                 let name = (string?)el.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                 let person = el.Element("Person")
                 let entry = el.Element("Entry")
                 let givenname = (string?)person.Element("Name")?.Element("Given").Value.TrimEnd(' ')
                 let familyname = (string?)person.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                 orderby name
                 select String.Format("'{0}';'{1}';'{2}';'{3}';'*';'Vindsprint 2022';'{4}';'Norwegian Athletic Federation'{5}",
             givenname,
             familyname,
             givenname + familyname + (string?)person.Attribute("clubName").Value.TrimEnd(' '),
             timeSchedule.GetAMEventName((string)entry.Element("Exercise").Attribute("name"), (string)entry.Element("EntryClass").Attribute("classCode")),
             (string)entry.Element("EntryClass").Attribute("shortName"),
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\participations.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(participationCSV));
        }

        public void CreateCompetitor(string _path)
        {
            string competitorCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate*';'db_licenses.licensenumber*';'db_competitions.name*';'db_federations.name*';'db_competitors.bib';'db_competitors.displayname'" + Environment.NewLine;
            competitorCSV = competitorCSV +
                (from el in root.Descendants("Person")
                 let name = (string?)el.Element("Name")?.Element("Family")
                 orderby name
                 select String.Format("'{0}';'{1}';'{2}-{3}-{4}';'{5}';'Vindsprint 2022';'Norwegian Athletic Federation';'';'{6}'{7}",
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' '),
             (string?)el.Element("BirthDate")?.Attribute("year"),
             (string)((int?)el.Element("BirthDate")?.Attribute("month") ?? 1).ToString("D2"),
             (string)((int?)el.Element("BirthDate")?.Attribute("day") ?? 1).ToString("D2"),
             (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' ') + (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' ') + (string?)el.Attribute("clubName").Value.TrimEnd(' '),
             (string?)el.Element("Name")?.Element("Family")?.Value.TrimEnd(' ') + ", " + (string?)el.Element("Name")?.Element("Given")?.Value.TrimEnd(' '),
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\competitors.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(competitorCSV));
        }

        public void CreateRecord(string _path, TimeSchedule timeSchedule)
        {

            string recordsCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate';'db_eventtypes.standardname*';'db_records.date';'db_records.value';'db_records.wind';'db_records.seasonflag';'db_records.alltimeflag'" + Environment.NewLine;
            recordsCSV = recordsCSV +
                (from el in root.Descendants("Competitor")
                 let name = (string?)el.Element("Name")?.Element("Family")
                 let person = el.Element("Person")
                 let entry = el.Element("Entry")
                 let givenname = (string?)person.Element("Name")?.Element("Given")
                 let familyname = (string?)person.Element("Name")?.Element("Family")
                 let athleteSB = RecordImporter.GetAthleteSB(givenname, familyname, (string)entry.Element("Exercise").Attribute("name"))
                 orderby name
                 select String.Format("'{0}';'{1}';'{2}-{3}-{4}';'{5}';'{6} 00:00:00';'{7}';'{8}';'1';'0'{9}",
             givenname.TrimEnd(' '),
             familyname.TrimEnd(' '),
             (string?)person.Element("BirthDate")?.Attribute("year"),
             (string)((int?)person.Element("BirthDate")?.Attribute("month") ?? 1).ToString("D2"),
             (string)((int?)person.Element("BirthDate")?.Attribute("day") ?? 1).ToString("D2"),
             timeSchedule.GetAMEvent((string)entry.Element("Exercise").Attribute("name"), (string)entry.Element("EntryClass").Attribute("classCode")).EventTypeStandardName,
             athleteSB.Date,
             athleteSB.Time,
             athleteSB.Wind,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            string filename = $"{_path}\\create\\records.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(recordsCSV));
        }
    }
}
