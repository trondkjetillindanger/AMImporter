using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class CsvImporter
    {
        public static List<EventTypeCategory> ImportFromCsv(string _path)
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
    }
}
