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
        public static List<iSonenParticipation> import(string filename, string filenameRelays)
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
                    csv.Context.RegisterClassMap<iSonenParticipationMap>();
                    ISonenParticipations = csv.GetRecords<iSonenParticipation>().ToList();
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
