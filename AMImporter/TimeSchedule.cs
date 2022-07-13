using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class TimeSchedule
    {
        string filename = "timeschedule.csv";
        public Dictionary<string, AMEvent> AMEvents { get; set; }
        public TimeSchedule(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            AMEvents = File.ReadLines(fullFileName).Select(line => line.Split(';')).ToDictionary(line => line[2], line => new AMEvent { Session = line[0], Name = line[2], Time = line[1], SAEventCategoryName = line[3], AgeCategory = line[4].Split(',').ToList(), EventTypeStandardName = line[5] });
        }

        public string GetAMEventName(string SAEventCategory, string SAAgeCategoryCode)
        {
            return AMEvents.Where(x => x.Value.SAEventCategoryName == SAEventCategory && x.Value.AgeCategory.Contains(SAAgeCategoryCode)).FirstOrDefault().Value.Name;
        }
    }
}
