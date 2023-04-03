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
            int eventId = 1;
            AMEvents = File.ReadLines(fullFileName).Select(
                line => line.Split(';')).ToDictionary(line => line[2], line => new AMEvent { Id = eventId++, Session = line[0], Name = line[2], Time = line[1], SAEventCategoryName = line[3], AgeCategory = line[4].Split(',').ToList(), EventTypeStandardName = line[5], SAEventName = (line.Length==7)?line[6]:null }
            );
        }

        public string GetAMEventName(string SAEventCategory, string SAAgeCategoryCode)
        {
            string AMEventName = null;
            try
            {
                var amEventNames = AMEvents.Where(x => x.Value.SAEventCategoryName == SAEventCategory && x.Value.AgeCategory.Contains(SAAgeCategoryCode));
                if (amEventNames.Any()) {
                    AMEventName = amEventNames.FirstOrDefault().Value.Name;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not find event {SAEventCategory} for age category {SAAgeCategoryCode}. Check if it is missing in {filename}");
                throw e;
            }
            return AMEventName;
        }

        public AMEvent GetAMEvent(string SAEventCategory, string SAAgeCategoryCode)
        {
            return AMEvents.Where(x => x.Value.SAEventCategoryName == SAEventCategory && x.Value.AgeCategory.Contains(SAAgeCategoryCode)).FirstOrDefault().Value;
        }
    }
}
