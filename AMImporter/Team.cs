using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMImporter
{
    public class Team
    {
        private XElement root = null;
        private List<string> teamNames = null;
        private string fullFileName = null;
        string filename = "teams.csv";
        public Team(XElement _root, string _path)
        {
            root = _root;
            fullFileName = _path + "\\" + filename;
            var teamEnum = from l in File.ReadAllLines(fullFileName)
                              let x = l.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Skip(2).SkipLast(2)
                              select new
                              {
                                  teamName = (string)x.First().Replace("'","")
                              };
            teamNames = teamEnum.Select(x => (string)x.teamName).Distinct().ToList<string>();
        }

        public List<string> FindMissing(List<string> _teamNames)
        {
            return _teamNames.Where(p => !teamNames.Any(p2 => p2 == p)).ToList();
        }

        public void AddMissing(List<string> newTeamNames)
        {
            //'db_federations.name*'; 'db_teams.abbreviation'; 'db_teams.name'; 'db_teams.federationnumber*'; 'db_teams.place'
            string teamsCSV =
                (from el in newTeamNames
                 select String.Format("'Norwegian Athletic Federation';'{1}';'{1}';'';''{2}",
             "",
             (string?)el,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );
            if (!string.IsNullOrEmpty(teamsCSV))
            {
                File.AppendAllText(fullFileName, teamsCSV);
            }
        }

        public void CreateMissing(List<string> newTeamNames, string _path)
        {
            string teamsCSV = "'db_federations.name*'; 'db_teams.abbreviation'; 'db_teams.name*'; 'db_teams.federationnumber'; 'db_teams.place'" + Environment.NewLine;
            teamsCSV = teamsCSV +
                (from el in newTeamNames
                 select String.Format("'Norwegian Athletic Federation';'{1}';'{1}';'';''{2}",
             "",
             (string?)el,
             Environment.NewLine))
                .Distinct()
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                    );

            int lastNewLine = teamsCSV.LastIndexOf(Environment.NewLine);
            teamsCSV = teamsCSV.Substring(0, lastNewLine - 1);

            string filename = $"{_path}\\create\\teams.csv";
            FileUtil.CreateNewCSV(filename, teamsCSV);
        }
    }
}
