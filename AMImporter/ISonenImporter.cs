using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class ISonenImporter
    {
        public static List<iSonenParticipation> import(string filename, string filenameRelays, bool isIsonen = true)
        {
            List<iSonenParticipation> ISonenParticipations = null;

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8, // Our file uses UTF-8 encoding.
                Delimiter = "," // The delimiter is a comma.
            };

            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var textReader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(textReader, configuration))
                {
                    if (isIsonen)
                    {
                        csv.Context.RegisterClassMap<iSonenParticipationMap>();
                        ISonenParticipations = csv.GetRecords<iSonenParticipation>().ToList();
                    }
                    else
                    {
                        csv.Context.RegisterClassMap<GSheetParticipationMap>();
                        var GSheetParticipations = csv.GetRecords<iSonenParticipation>().ToList();

                        GSheetParticipations = GSheetParticipations
                            .GroupBy(p => new
                            {
                                FirstName = p.FirstName.ToLowerInvariant().Trim().Replace("-",""),
                                LastName = p.LastName.ToLowerInvariant().Trim().Replace("-",""),
                                Team = p.Team.ToLowerInvariant(),
                                EventCategory = p.EventCategory.ToLowerInvariant()
                            })
                            .SelectMany(g => g.OrderByDescending(p => p.Tidsmerke).Take(1))
                            .ToList();

                        ISonenParticipations = GSheetParticipations.SelectMany(x =>
                        {
                            return x.Event.Split(',').Select(y =>
                            {
                                y = y.Replace("Liten ball", "Litenball");
                                string[] parts = y.Trim().Split(' ');
                                parts[0] = parts[0].Replace("Litenball", "Liten ball");
                                parts[0] = parts[0].Replace("60m", "60 meter");
                                parts[0] = parts[0].Replace("600m", "600 meter");
                                string gender = x.EventCategory.Contains("gutt") ? "M" :
                                                x.EventCategory.Contains("jente") ? "K" : "";
                                if (x.IsOlderThan13() || x.IsYoungerThan10())
                                {
                                    x.BirthDate = MapEventCategoryToBirthDate(parts[1]);
                                }

                                return new iSonenParticipation()
                                {
                                    FirstName = x.FirstName.Trim(),
                                    LastName = x.LastName.Trim(),
                                    BirthDate = x.BirthDate,
                                    Team = x.Team,
                                    Event = parts[0],
                                    EventCategory = MapEventCategory(parts[1]),
                                    Gender = gender,
                                    License = x.FirstName.Replace(" ","")+x.LastName.Replace(" ", "") + x.BirthDate
                                };
                            });
                        }).ToList();

                        //ISonenParticipations = rawParticipations
                        //    .GroupBy(p => new { p.FirstName, p.LastName, p.BirthDate, p.Gender})
                        //    .SelectMany(g => g.OrderByDescending(p => p.Tidsmerke).Take(2))
                        //    .Distinct()
                        //    .ToList();

                        var duplicates = ISonenParticipations
                            .GroupBy(p => p)
                            .Where(g => g.Count() > 1)
                            .SelectMany(g => g)
                            .ToList();

                        if (duplicates.Any())
                        {
                            Console.WriteLine("There are duplicates.");
                            foreach (var dup in duplicates)
                            {
                                Console.WriteLine($"{dup.FirstName} {dup.LastName}, {dup.BirthDate}, {dup.Gender}, {dup.Event}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No duplicates found.");
                        }

                    }
                }
            }

            if (!string.IsNullOrEmpty(filenameRelays) ){
                using (var fs = File.Open(filenameRelays, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var textReader = new StreamReader(fs, Encoding.UTF8))
                    using (var csv = new CsvReader(textReader, configuration))
                    {
                        csv.Context.RegisterClassMap<iSonenParticipationRelaysMap>();
                        ISonenParticipations.AddRange(csv.GetRecords<iSonenParticipation>().ToList());
                        //ISonenParticipations = csv.GetRecords<iSonenParticipation>().ToList();
                    }
                }
            }
            return ISonenParticipations;
        }


        static string MapEventCategory(string eventCategory)
        {
            var mappings = new Dictionary<string, string>
            {
                { "J11", "Jenter 11" },
                { "G11", "Gutter 11" },
                { "J12", "Jenter 12" },
                { "G12", "Gutter 12" },
                { "J13", "Jenter 13" },
                { "G13", "Gutter 13" }
            };

            if (mappings.TryGetValue(eventCategory, out var mappedValue))
            {
                return mappedValue;
            }
            else
            {
                return eventCategory;
            }
        }


        static string MapEventCategoryToBirthDate(string eventCategory)
        {
            var mappings = new Dictionary<string, string>
            {
                { "J11", "01.01.2014" },
                { "G11", "01.01.2014" },
                { "J12", "01.01.2013" },
                { "G12", "01.01.2013" },
                { "J13", "01.01.2012" },
                { "G13", "01.01.2012" }
            };

            if (mappings.TryGetValue(eventCategory, out var mappedValue))
            {
                return mappedValue;
            }
            else
            {
                return eventCategory;
            }
        }


        public static List<iSonenParticipation> FixRelays(List<iSonenParticipation> iSonenParticipations)
        {
            List<iSonenParticipation> iSonenParticipationsClone = new List<iSonenParticipation>(iSonenParticipations);
            iSonenParticipations.Where(x => x.Event.Contains("stafett")).ToList().ForEach(y =>
            {
                iSonenParticipation FirstParticipationInTeam = iSonenParticipations.Where(z =>  (z.Event.Contains("stafett")) && (z.GroupId == y.TeamId || z.TeamId == y.TeamId) && z.EventCategory == y.EventCategory).First();
                y.FirstName = FirstParticipationInTeam.FirstName;
                y.LastName = FirstParticipationInTeam.LastName;
                y.BirthDate = FirstParticipationInTeam.BirthDate;
                y.Gender = FirstParticipationInTeam.Gender;
                y.Team = FirstParticipationInTeam.Team;
                y.TeamId = FirstParticipationInTeam.TeamId;
                y.GroupId = FirstParticipationInTeam.GroupId;
            });

            return iSonenParticipations;
        }
    }
}
