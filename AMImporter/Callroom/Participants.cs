using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class ParticipantList
    {
        public List<AMImporter.Callroom.Participant> participants { get; set;}
        public void CreateCallroomParticipants(TimeSchedule timeSchedule, List<iSonenParticipation> ISonenParticipations, EventCategory eventCategory) 
        {
            participants = ISonenParticipations.Where(x => !string.IsNullOrWhiteSpace(x.Event)).GroupBy(x => $"{x.Team}{x.LastName}{x.FirstName}")
                                .Select(y => {
                                    var eventCategoryName = y.First().EventCategory;
                                    var amCategoryAbbreviation = eventCategory.amCategory.GetAMAbbreviation(y.First().EventCategory);
                                    return new AMImporter.Callroom.Participant()
                                    {
                                        participant_name = $"{y.First().FirstName} {y.First().LastName}",
                                        participant_bib = y.First().Bib + "",
                                        participant_id = y.First().Bib + "",
                                        participant_team = y.First().Team,
                                        competition_id = "1",
                                        email = y.First().Email,
                                        email2 = y.First().EmailRegistrant,
                                        events = y.Select(q =>
                                        {
                                            var eventsByNameAndCategory = timeSchedule.AMEvents.Values.Where(w => w.SAEventCategoryName == q.Event && w.AgeCategory.Contains(amCategoryAbbreviation));
                                            return new AMImporter.Callroom.Participation()
                                            {
                                            //event_id = q.Event,
                                                event_id = eventsByNameAndCategory.Any() ? eventsByNameAndCategory.First()?.Id + "" : "-1",
                                                callroomstatus = false,
                                                callroomstatus_modificationtime = "null",
                                                year_best = "null",
                                                personal_best = "null"
                                            };
                                        }).ToList()
                                    };
                                }).ToList();

        }
    }
}
