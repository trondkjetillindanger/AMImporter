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
        public Dictionary<string, string> AMEventTypes { get; set; }
        public AMEventType(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            AMEventTypes = File.ReadLines(fullFileName).Select(line => line.Split(';')).Where(line => line[2]=="'I'" && line[17]=="'en'").ToDictionary(line => line[13].Replace("'",""), line => line[3].Replace("'", "")); 
            var AMEventCombinedTypes = File.ReadLines(fullFileName).Select(line => line.Split(';')).Where(line => line[2] == "'I'" && line[17] == "'en'" && line[11] != "'0'").ToDictionary(line => line[14].Replace("'", ""), line => line[3].Replace("'", ""));
            var AMEventTypesUnion = AMEventTypes.Union(AMEventCombinedTypes);
            AMEventTypes = AMEventTypesUnion.ToDictionary(x => x.Key, x => x.Value);
            AMEventTypes.Remove("db_eventtypes.standardname*");
        }

        public string GetAMEventTypeAbbreviation(string AMStandardName)
        {
            return AMEventTypes[AMStandardName];
        }
    }
}
