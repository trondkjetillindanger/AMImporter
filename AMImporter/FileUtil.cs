using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public static class FileUtil
    {
        public static void CreateNewCSV(string filename, string CSV)
        {
            File.Delete(filename);
            using (StreamWriter sw = new StreamWriter(File.Open(filename, FileMode.CreateNew), Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(CSV);
            }
        }
    }
}
