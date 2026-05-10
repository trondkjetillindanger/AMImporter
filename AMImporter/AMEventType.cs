using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class AMEventType
    {
        string filename = "eventtypes.csv";
        string indoorOutdoorCode = "I";
        public Dictionary<string, string> AMEventTypes { get; set; }
        private Dictionary<string, string> AMEventTypeVenues { get; set; }
        public AMEventType(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            AMEventTypeVenues = File.ReadLines(fullFileName).Select(line => line.Split(';')).Where(line => line.Length > 17 && line[17] == "'en'").SelectMany(line => new[]
            {
                new { StandardName = line[13].Replace("'", ""), Venue = line[2].Replace("'", "") },
                new { StandardName = line[14].Replace("'", ""), Venue = line[2].Replace("'", "") }
            }).Where(x => !string.IsNullOrEmpty(x.StandardName) && x.StandardName != "db_eventtypes.standardname*" && x.StandardName != "db_eventtypes.standardabbreviation").GroupBy(x => x.StandardName).ToDictionary(x => x.Key, x => x.First().Venue);
            AMEventTypes = File.ReadLines(fullFileName).Select(line => line.Split(';')).Where(line => line[2]== $"'{indoorOutdoorCode}'" && line[17]=="'en'").ToDictionary(line => line[13].Replace("'",""), line => line[3].Replace("'", "")); 
            var AMEventCombinedTypes = File.ReadLines(fullFileName).Select(line => line.Split(';')).Where(line => line[2] == $"'{indoorOutdoorCode}'" && line[17] == "'en'" && line[11] != "'0'").ToDictionary(line => line[14].Replace("'", ""), line => line[3].Replace("'", ""));
            var AMEventTypesUnion = AMEventTypes.Union(AMEventCombinedTypes);
            AMEventTypes = AMEventTypesUnion.ToDictionary(x => x.Key, x => x.Value);
            AMEventTypes.Remove("db_eventtypes.standardname*");
        }

        public string GetAMEventTypeAbbreviation(string AMStandardName)
        {
            return AMEventTypes[AMStandardName];
        }

        public bool IsOutdoor(string? AMStandardName)
        {
            if (string.IsNullOrEmpty(AMStandardName))
            {
                return true;
            }

            return !AMEventTypeVenues.TryGetValue(AMStandardName, out var venue) || venue == "O";
        }
    }
}
