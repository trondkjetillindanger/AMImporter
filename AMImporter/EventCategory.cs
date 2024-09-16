using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AMImporter
{
    public class EventCategory
    {
        public virtual AMCategory amCategory { get; set; }
        public void Create(string _path, TimeSchedule timeSchedule, Competition competition)
        {
            amCategory = new AMCategory(_path);
            int seqno = 1;
            string eventCategoriesCSV = "'db_events.name*';'db_competitions.name*';'db_categories.name*';'db_federations.name*'" + Environment.NewLine;
            timeSchedule.AMEvents.Values.ToList().ForEach( amEvent => {
                amEvent.AgeCategory.ForEach(ageCategoryAbbreviation =>
                {
                    string newLine = String.Format("'{0}';'{1}';'{2}';'{3}'",
                        amEvent.Name,
                        competition.CompetitionDTO.Name,
                        amCategory.GetAMName(ageCategoryAbbreviation),
                        competition.CompetitionDTO.FederationName);
                    eventCategoriesCSV = eventCategoriesCSV + newLine + Environment.NewLine;
                });
            });

            string filename = $"{_path}\\create\\eventcategories.csv";
            CSVUtil.CreateNewCSV(filename, CSVUtil.RemoveLastNewline(eventCategoriesCSV), Encoding.GetEncoding("iso-8859-1"));
        }
    }
}
