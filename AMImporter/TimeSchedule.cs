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
        public bool AutoGenerate { get; set; } = false;
        public Dictionary<string, AMEvent> AMEvents { get; set; }
        public TimeSchedule(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            if (!File.Exists(fullFileName))
            {
                AutoGenerate = true;
                return;
            }

            AMEvents = File.ReadLines(fullFileName)
            .Where(line => !line.TrimStart().StartsWith("//") && line != "")
            .Select(line => line.Split(';'))
            .ToDictionary(
                line => $"{line[2]} {line[1]}",
                line => new AMEvent
                {
                    Session = line[0],
                    Name = line[2],
                    Time = line[1],
                    SAEventCategoryName = line[3],
                    AgeCategory = line[4].Split(',').ToList(),
                    EventTypeStandardName = line[5],
                    SAEventName = (line.Length == 7) ? line[6] : null
                }
            );
         
            int eventId = 0;
            var AMEventGroups = AMEvents.Values.GroupBy(x => x.Name);
            AMEventGroups.ToList().ForEach(x =>
            {
                eventId++;
                x.ToList().ForEach(y => y.Id = eventId);
            });           
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
            return AMEvents.Where(x => x.Value.SAEventCategoryName == SAEventCategory.Trim() && x.Value.AgeCategory.Contains(SAAgeCategoryCode)).FirstOrDefault().Value;
        }
    }
}
