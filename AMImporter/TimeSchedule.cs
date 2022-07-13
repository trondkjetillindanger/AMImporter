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
            string fullFileName = _path + "\\" + filename;
            AMEvents = File.ReadLines(fullFileName).Select(line => line.Split(';')).ToDictionary(line => line[1], line => new AMEvent { Name = line[1], Time = line[0], SAEventCategoryName = line[2], AgeCategory = line[3].Split(',').ToList(), EventTypeStandardName = line[4] });
        }

        public string GetAMEventName(string SAEventCategory, string SAAgeCategoryCode)
        {
            return AMEvents.Where(x => x.Value.SAEventCategoryName == SAEventCategory && x.Value.AgeCategory.Contains(SAAgeCategoryCode)).FirstOrDefault().Value.Name;
        }
    }
}
