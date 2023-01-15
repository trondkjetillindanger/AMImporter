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
        private List<iSonenParticipationDTO> ISonenParticipations = null;
        private Team teams = null;
        private AMCategory amCategory = null;
        public Athlete(XElement _root, Team _teams)
        {
            root = _root;
            teams = _teams;
        }

        public Athlete(List<iSonenParticipationDTO> _ISonenParticipations, Team _teams, AMCategory _amCategory)
        {
            ISonenParticipations = _ISonenParticipations;
            teams = _teams;
            amCategory = _amCategory;
        }

        public List<string> GetTeams()
        {
            //var teamEnum = from el in root.Descendants("Person") select new { teamName = el.Attribute("clubName").Value, hasclub = teams.Contains(el.Attribute("clubName").Value) };
            if (root != null)
            {
                return (from el in root.Descendants("Person")
                        select el.Attribute("clubName").Value.TrimEnd(' ')).Distinct().ToList<string>();
            }
            else
            {
                return ISonenParticipations.Select(x => x.Team).Distinct<string>().ToList<string>();
            }
        }

        public void Create(string _path)
        {
            string athletesCSV = "'db_licenses.licensenumber*';'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.middlename';'db_athletes.gender';'db_athletes.birthdate';'db_countries.name';'db_teams.name';'db_federations.name'" + Environment.NewLine;

            if (root != null)
            {
                athletesCSV = athletesCSV +
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
            }
            else
            {
                athletesCSV = athletesCSV + ISonenParticipations.Where(y => !string.IsNullOrEmpty(y.FirstName)).Select(x =>
                    $"'{x.FirstName + x.LastName + x.Team}';'{x.FirstName}';'{x.LastName}';'{(NorwegianToUKGender(x.Gender))}';'';'{(NorwegianToUKDateFormat(x.BirthDate))}';'Norway';'{x.Team}';'Norwegian Athletic Federation'{Environment.NewLine}"
                ).Distinct()
                 .Aggregate(
                        new StringBuilder(),
                        (sb, s) => sb.Append(s),
                        sb => sb.ToString()
                        );
            }
            string filename = $"{_path}\\create\\athletes.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(athletesCSV));
        }

        private string NorwegianToUKGender(string gender)
        {
            return gender == "K" ? "W" : "M";
        }

        private string NorwegianToUKDateFormat(string datestring)
        {
            if (string.IsNullOrEmpty(datestring))
            {
                return datestring;
            }
            var parts = datestring.Split(".");
            return parts[2] + "-" + parts[1] + "-" + parts[0];
        }

        /*
        private string EventCategoryToClassCode(string eventCategory)
        {
            string code = null;
            var eventCategorySplit = eventCategory.Split(" ").ToArray();
            if (eventCategorySplit.Length==2)
            {
                code = eventCategorySplit[0].Take(1) + eventCategorySplit[1];
            }
            return code;
        }
        */

        public void CreateLicense(string _path)
        {
            string licensesCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate';'db_teams.name';'db_federations.name';'db_licenses.licensenumber*';'db_licenses.bib'" + Environment.NewLine;

            if (root != null)
            {
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
            }
            else
            {
                licensesCSV = licensesCSV + ISonenParticipations.Where(y => !string.IsNullOrEmpty(y.FirstName)).Select(x =>
                    $"'{x.FirstName}';'{x.LastName}';'{(NorwegianToUKDateFormat(x.BirthDate))}';'{x.Team}';'Norwegian Athletic Federation';'{x.FirstName + x.LastName + x.Team}';''{Environment.NewLine}"
                ).Distinct()
                 .Aggregate(
                        new StringBuilder(),
                        (sb, s) => sb.Append(s),
                        sb => sb.ToString()
                        );
            }
            string filename = $"{_path}\\create\\licenses.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(licensesCSV));
        }

        public void CreateParticipation(string _path, TimeSchedule timeSchedule)
        {
            string participationCSV = "'db_athletes.firstname';'db_athletes.lastname';'db_licenses.licensenumber';'db_events.name';'db_rounds.name';'db_competitions.name';'db_categories.name';'db_federations.name'" + Environment.NewLine;

            if (root != null)
            {
                participationCSV = participationCSV +
                    (from el in root.Descendants("Competitor")
                     let name = (string?)el.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                     let person = el.Element("Person")
                     let entry = el.Element("Entry")
                     let givenname = (string?)person.Element("Name")?.Element("Given").Value.TrimEnd(' ')
                     let familyname = (string?)person.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                     let amEventName = timeSchedule.GetAMEventName((string)entry.Element("Exercise").Attribute("name"), (string)entry.Element("EntryClass").Attribute("classCode"))
                     where amEventName != null
                     orderby name
                     select String.Format("'{0}';'{1}';'{2}';'{3}';'*';'COOP Jærcup 8';'{4}';'Norwegian Athletic Federation'{5}",
                 givenname,
                 familyname,
                 givenname + familyname + (string?)person.Attribute("clubName").Value.TrimEnd(' '),
                 amEventName,
                 (string)entry.Element("EntryClass").Attribute("shortName"),
                 Environment.NewLine))
                    .Distinct()
                    .Aggregate(
                        new StringBuilder(),
                        (sb, s) => sb.Append(s),
                        sb => sb.ToString()
                        );
            }
            else
            {
                participationCSV = participationCSV + ISonenParticipations.Where(y => !string.IsNullOrEmpty(y.FirstName) && !string.IsNullOrEmpty(y.EventCategory)).Select(x => {
                    var ageCode = amCategory.GetAMAbbreviation(x.EventCategory);
                    var amEventName = timeSchedule.GetAMEventName(x.Event, ageCode);

                    return $"'{x.FirstName}';'{x.LastName}';'{x.FirstName + x.LastName + x.Team}';'{amEventName}';'*';'COOP Jærcup 1';'{ageCode}';'Norwegian Athletic Federation'{Environment.NewLine}";
                }).Distinct()
                 .Aggregate(
                        new StringBuilder(),
                        (sb, s) => sb.Append(s),
                        sb => sb.ToString()
                        );
            }

            string filename = $"{_path}\\create\\participations.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(participationCSV));
        }

        public void CreateParticipationWithoutEvent(string _path, TimeSchedule timeSchedule)
        {

            string participationCSV = "'db_athletes.firstname';'db_athletes.lastname';'db_licenses.licensenumber';'db_events.name';'db_rounds.name';'db_competitions.name';'db_categories.name';'db_federations.name'" + Environment.NewLine;
            if (root != null)
            {
                participationCSV = participationCSV +
                    (from el in root.Descendants("Competitor")
                     let name = (string?)el.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                     let person = el.Element("Person")
                     let entry = el.Element("Entry")
                     let givenname = (string?)person.Element("Name")?.Element("Given").Value.TrimEnd(' ')
                     let familyname = (string?)person.Element("Name")?.Element("Family").Value.TrimEnd(' ')
                     let amEventName = timeSchedule.GetAMEventName((string)entry.Element("Exercise").Attribute("name"), (string)entry.Element("EntryClass").Attribute("classCode"))
                     where amEventName == null
                     orderby name
                     select String.Format("'{0}';'{1}';'{2}';'{3}';'*';'COOP Jærcup 8';'{4}';'Norwegian Athletic Federation'{5}",
                 givenname,
                 familyname,
                 givenname + familyname + (string?)person.Attribute("clubName").Value.TrimEnd(' '),
                 (string)entry.Element("Exercise").Attribute("name") + " " + (string)entry.Element("EntryClass").Attribute("classCode"),
                 (string)entry.Element("EntryClass").Attribute("shortName"),
                 Environment.NewLine))
                    .Distinct()
                    .Aggregate(
                        new StringBuilder(),
                        (sb, s) => sb.Append(s),
                        sb => sb.ToString()
                        );
            }
            else
            {
                participationCSV = participationCSV + ISonenParticipations.Where(y => string.IsNullOrEmpty(timeSchedule.GetAMEventName(y.Event, y.EventCategory)) && !string.IsNullOrEmpty(y.FirstName) && !string.IsNullOrEmpty(y.EventCategory)).Select(x =>
                     $"'{x.FirstName}';'{x.LastName}';'{x.FirstName + x.LastName + x.Team}';'{timeSchedule.GetAMEventName(x.Event, x.EventCategory)}';'*';'COOP Jærcup 1';'{(amCategory.GetAMAbbreviation(x.EventCategory))}';'Norwegian Athletic Federation'{Environment.NewLine}"
                 ).Distinct()
                  .Aggregate(
                         new StringBuilder(),
                         (sb, s) => sb.Append(s),
                         sb => sb.ToString()
                         );
            }

            string filename = $"{_path}\\create\\participations_not_found.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(participationCSV));
        }

        public void CreateCompetitor(string _path)
        {
            string competitorCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate*';'db_licenses.licensenumber*';'db_competitions.name*';'db_federations.name*';'db_competitors.bib';'db_competitors.displayname'" + Environment.NewLine;
            if (root != null)
            {
                competitorCSV = competitorCSV +
                    (from el in root.Descendants("Person")
                     let name = (string?)el.Element("Name")?.Element("Family")
                     orderby name
                     select String.Format("'{0}';'{1}';'{2}-{3}-{4}';'{5}';'COOP Jærcup 8';'Norwegian Athletic Federation';'';'{6}'{7}",
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
            }
            else
            {
                competitorCSV = competitorCSV + ISonenParticipations.Where(y => !string.IsNullOrEmpty(y.FirstName)).Select(x =>
                     $"'{x.FirstName}';'{x.LastName}';'{(NorwegianToUKDateFormat(x.BirthDate))}';'{x.FirstName + x.LastName + x.Team}';'COOP Jærcup 1';'Norwegian Athletic Federation';'';'{x.LastName}, {x.FirstName}'{Environment.NewLine}"
                 ).Distinct()
                  .Aggregate(
                         new StringBuilder(),
                         (sb, s) => sb.Append(s),
                         sb => sb.ToString()
                         );
            }

            string filename = $"{_path}\\create\\competitors.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(competitorCSV));
        }

        public void CreateRecord(string _path, TimeSchedule timeSchedule)
        {
            string recordsCSV = "'db_athletes.firstname*';'db_athletes.lastname*';'db_athletes.birthdate';'db_eventtypes.standardname*';'db_records.date';'db_records.value';'db_records.wind';'db_records.seasonflag';'db_records.alltimeflag'" + Environment.NewLine;

            if (root != null)
            {
                recordsCSV = recordsCSV +
                    (from el in root.Descendants("Competitor")
                     let name = (string?)el.Element("Name")?.Element("Family")
                     let person = el.Element("Person")
                     let entry = el.Element("Entry")
                     let ageCode = (string)entry.Element("EntryClass").Attribute("classCode")
                     let eventCategory = (string)entry.Element("Exercise").Attribute("name")
                     let givenname = (string?)person.Element("Name")?.Element("Given")
                     let familyname = (string?)person.Element("Name")?.Element("Family")
                     let athleteSB = RecordImporter.GetAthleteSB(givenname, familyname, timeSchedule.GetAMEvent(eventCategory, ageCode)?.SAEventName ?? eventCategory, ageCode)
                     orderby name
                     select String.Format("'{0}';'{1}';'{2}-{3}-{4}';'{5}';'{6} 00:00:00';'{7}';'{8}';'1';'0'{9}",
                 givenname.TrimEnd(' '),
                 familyname.TrimEnd(' '),
                 (string?)person.Element("BirthDate")?.Attribute("year"),
                 (string)((int?)person.Element("BirthDate")?.Attribute("month") ?? 1).ToString("D2"),
                 (string)((int?)person.Element("BirthDate")?.Attribute("day") ?? 1).ToString("D2"),
                 timeSchedule.GetAMEvent(eventCategory, ageCode)?.EventTypeStandardName,
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
            }
            else
            {
                recordsCSV = recordsCSV + ISonenParticipations.Where(y => !string.IsNullOrEmpty(y.FirstName) && !string.IsNullOrEmpty(y.EventCategory)).Select(x => {
                    var ageCode = amCategory.GetAMAbbreviation(x.EventCategory);
                    var amEvent = timeSchedule.GetAMEvent(x.EventCategory, ageCode );
                    var standardName = amEvent?.EventTypeStandardName;
                    var athleteSB = RecordImporter.GetAthleteSB(x.FirstName, x.LastName, amEvent?.SAEventName ?? x.Event, ageCode);
                    return $"'{x.FirstName}';'{x.LastName}';'{(NorwegianToUKDateFormat(x.BirthDate))}';'{x.FirstName + x.LastName + x.Team}';'{timeSchedule.GetAMEvent(x.EventCategory, amCategory.GetAMAbbreviation(x.EventCategory))?.EventTypeStandardName}';'{athleteSB.Date}  00:00:00';'{athleteSB.Time}';'{athleteSB.Wind}';'1';'0'{Environment.NewLine}";
                 })
                  .Distinct()
                  .Aggregate(
                         new StringBuilder(),
                         (sb, s) => sb.Append(s),
                         sb => sb.ToString()
                         );
            }

            string filename = $"{_path}\\create\\records.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(recordsCSV));
        }
    }
}
