using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class ISonenImporter
    {
        public static List<iSonenParticipation> import(string filename)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8, // Our file uses UTF-8 encoding.
                Delimiter = "," // The delimiter is a comma.
            };

            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var textReader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(textReader, configuration))
                {
                    csv.Context.RegisterClassMap<iSonenParticipationMap>();
                    return csv.GetRecords<iSonenParticipation>().ToList();
                }
            }
        }
    }
}
