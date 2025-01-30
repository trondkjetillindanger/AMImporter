using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class CsvImporter
    {
        public static List<EventTypeCategory> ImportEventTypeCategory(string _path)
        {
            string filename = $"{_path}\\create\\eventtypecategories.csv";
            var eventTypeCategories = new List<EventTypeCategory>();

            try
            {
                // Open the file using ANSI encoding (ISO-8859-1)
                using (var reader = new StreamReader(filename, Encoding.GetEncoding("iso-8859-1")))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Split the line by semicolon delimiter
                        var fields = line.Split(';');

                        if (fields.Length == 4)
                        {
                            // Create a new EventTypeCategory object and populate its properties
                            var eventTypeCategory = new EventTypeCategory
                            {
                                SAEventName = fields[0],
                                AMCategoryAbbreviation = fields[1],
                                AMEventName = fields[2],
                                SAEventCategoryName = fields[3]
                            };

                            // Add the object to the list
                            eventTypeCategories.Add(eventTypeCategory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading CSV file: " + ex.Message);
            }

            return eventTypeCategories;
        }


        public static List<EventType> ImportEventTypes(string _path)
        {
            string filePath = $"{_path}\\create\\eventtypes.csv";

            var eventTypes = new List<EventType>();
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.GetEncoding("iso-8859-1")))
                {
                    string line;
                    bool firstLine = true;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (firstLine)
                        {
                            firstLine = false; // Skip header
                            continue;
                        }

                        var fields = line.Split(';').Select(f => f.Trim('\'')).ToArray();
                        if (fields.Length == 19)
                        {
                            var eventType = new EventType
                            {
                                Name = fields[0].Trim('"'),
                                Abbreviation = fields[1].Trim('"'),
                                Venue = fields[2].Trim('"'),
                                Distance = int.TryParse(fields[3], out int d) ? d : 0,
                                WindMode = fields[4].Trim('"'),
                                WindTime = fields[5].Trim('"'),
                                Precision = int.TryParse(fields[6], out int p) ? p : 0,
                                FieldType = fields[7].Trim('"'),
                                HandTimeDifference = int.TryParse(fields[8], out int h) ? h : 0,
                                AthletesQuantity = int.TryParse(fields[9], out int aq) ? aq : 0,
                                Implement = int.TryParse(fields[10], out int imp) ? imp : 0,
                                CombinedEventsQuantity = int.TryParse(fields[11], out int ceq) ? ceq : 0,
                                CombinedEvents = fields[12].Trim('"'),
                                StandardName = fields[13].Trim('"'),
                                StandardAbbreviation = fields[14].Trim('"'),
                                LowToHigh = int.TryParse(fields[15], out int lth) ? lth : 0,
                                NationalCode = fields[16].Trim('"'),
                                Language = fields[17].Trim('"'),
                                Value = fields[18].Trim('"')
                            };
                            eventTypes.Add(eventType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading CSV file: " + ex.Message);
            }
            return eventTypes;
        }
    }

}
