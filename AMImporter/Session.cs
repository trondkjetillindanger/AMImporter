using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class Session
    {
        public static string GetName(string ageCategory)
        {
            if (ageCategory == "G10" || ageCategory == "G11" || ageCategory == "G12" ||
                ageCategory == "J10" || ageCategory == "J11" || ageCategory == "J12")
            {
                return "10-12";
            }
            else if (ageCategory == "G13" || ageCategory == "G14" ||
                     ageCategory == "J13" || ageCategory == "J14")
            {
                return "13-14";
            }
            else if (ageCategory == "G15" || ageCategory == "G16" || ageCategory == "G17" ||
                     ageCategory == "G18/19" || ageCategory == "MS" ||
                     ageCategory == "J15" || ageCategory == "J16" || ageCategory == "J17" ||
                     ageCategory == "J18/19" || ageCategory == "KS")
            {
                return "15+";
            }
            else
            {
                return "Unknown Age Category";
            }
        }
    }
}
