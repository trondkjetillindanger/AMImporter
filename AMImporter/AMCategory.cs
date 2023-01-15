using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class AMCategory
    {
        string filename = "categories.csv";
        public Dictionary<string, string> AMCategories { get; set; }
        public AMCategory(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            AMCategories = File.ReadLines(fullFileName).Select(line => line.Split(';')).ToDictionary(line => line[2].Replace("'",""), line => line[1].Replace("'", ""));
            AMCategories.Remove("db_categories.abbreviation");
        }

        public string GetAMName(string AMAbbreviation)
        {
            return AMCategories[AMAbbreviation];
        }

        public string GetAMAbbreviation(string AMName)
        {
            return AMCategories.Where(x => x.Value.ToLower() == AMName.ToLower()).First().Key;
        }
    }
}
