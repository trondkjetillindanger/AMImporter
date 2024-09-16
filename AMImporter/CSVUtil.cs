using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public static class CSVUtil
    {
        public static void CreateNewCSV(string filename, string CSV, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            File.Delete(filename);
            using (StreamWriter sw = new StreamWriter(File.Open(filename, FileMode.CreateNew), encoding))
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
