using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class Competition
    {
        string filename = "competitions.csv";
        public AMCompetitionDTO CompetitionDTO { get; set; }
        public Competition(string _path)
        {
            string fullFileName = _path + "\\create\\" + filename;
            CompetitionDTO = File.ReadLines(fullFileName, Encoding.GetEncoding("iso-8859-1")).Select(line => line.Split(';')).Where(line => line[0]!= "'db_federations.name'").Select(line => new AMCompetitionDTO { Name = line[2].Replace("'",""), FederationName = line[0].Replace("'", ""), Location = line[4], StartDate = line[6], EndDate = line[7] }).FirstOrDefault();
        }
    }
}
