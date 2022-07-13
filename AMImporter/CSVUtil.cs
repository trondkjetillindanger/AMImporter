using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public static class CSVUtil
    {
        public static void CreateNewCSV(string filename, string CSV)
        {
            File.Delete(filename);
            using (StreamWriter sw = new StreamWriter(File.Open(filename, FileMode.CreateNew), Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(CSV);
            }
        }

        public static string RemoveLastNewline(string CSV)
        {
            int lastNewLine = CSV.LastIndexOf(Environment.NewLine);
            return CSV.Substring(0, lastNewLine);
        }
    }
}
