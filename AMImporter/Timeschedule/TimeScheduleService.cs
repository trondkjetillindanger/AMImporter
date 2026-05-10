using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if MEET_SCHEDULER_AVAILABLE
using MeetScheduler;
#endif

namespace AMImporter.Timeschedule
{
    public class TimeScheduleService
    {
        private List<Participation> _groupedParticipations = null;
        private AMCategory _amCategory = null;
        private List<EventTypeCategory> _categories = null;
        private string _filename = null;
        private string _path = null;

        public TimeScheduleService(string filename, string path)
        {
            _filename = filename;
            _path = path;
        }

        public void Create()
        {
#if !MEET_SCHEDULER_AVAILABLE
            throw new InvalidOperationException("Automatic time schedule generation requires the MeetScheduler project at '..\\..\\MeetScheduler\\MeetScheduler\\MeetScheduler.csproj'. Add that sibling repository or provide create\\timeschedule.csv before running AMImporter.");
#else
            var ISonenParticipations = ISonenImporter.import(_filename, null);
            ISonenParticipations = ISonenImporter.FixRelays(ISonenParticipations);
            ISonenImporter.FixRelays(ISonenParticipations);

            _amCategory = new AMCategory(_path);
            _categories = CsvImporter.ImportEventTypeCategory(_path);
            List<EventType> eventTypes = CsvImporter.ImportEventTypes(_path);
            var englishEventTypes = eventTypes.Where(x => x.Language == "en").ToList<EventType>();
            int id = 0;

            _groupedParticipations = ISonenParticipations
                .Where(p => !string.IsNullOrEmpty(p.Event))
                .Where(p => p.EventCategory.Contains("Gutter 14") || p.EventCategory.Contains("Gutter 13"))
                .GroupBy(p => new { p.Event, p.EventCategory, EventDate = DateTime.Parse(p.EventDate) })
                .OrderBy(g => g.Key.EventDate)
                .ThenBy(g => g.Key.Event)  // Secondary ordering by Event
                .ThenBy(g => g.Key.EventCategory)  // Tertiary ordering by EventCategory
                .Select(g =>
                {
                    var eventType = englishEventTypes.Where(y => y.StandardName == (_categories.Where(x => x.SAEventName == g.Key.Event).FirstOrDefault()?.AMEventName ?? "NA")).FirstOrDefault();

                    return new Participation()
                    {
                        Id = id++,
                        Event = g.Key.Event,
                        EventCategory = g.Key.EventCategory,
                        EventCategoryAbbreviation = _amCategory.GetAMAbbreviation(g.Key.EventCategory),
                        EventDate = g.Key.EventDate,
                        Participants = g.ToList<iSonenParticipation>(), // List of participants in this group
                        FieldType = eventType?.FieldType.Trim(),
                        EventAbbreviation = eventType?.StandardAbbreviation,
                        Distance = eventType?.Distance,
                        EstimatedDuration = AthleticEventTimeCalculator.EstimateEventTime(eventType?.StandardAbbreviation, eventType?.FieldType, g.Count(), eventType?.Distance)
                    };
                })
                .ToList<Participation>();

            int i = 0;
            _groupedParticipations = EventGrouper.MergeEvents(_groupedParticipations);
            _groupedParticipations.ForEach(x =>
            {
                x.Id = i++;
            });

            int takeCount = 20;
            //var numSlots = 48;
            var numSlots = 88;

            var filteredGroupedParticipations = _groupedParticipations.Take(takeCount);

            var events = filteredGroupedParticipations.Select(x =>
            {
                string area = $"{(string.IsNullOrEmpty(x.FieldType) ? "Running" : x.EventAbbreviation)}";
                if (x.EventAbbreviation == "LJ" || x.EventAbbreviation == "TJ")
                {
                    // Distribute evenly between the two long jump pits.
                    Random random = new Random();
                    string[] options = { "Lengdegrop 1", "Lengdegrop 2" };
                    area = options[random.Next(options.Length)];
                }
                return new MeetScheduler.Event()
                {
                    Id = x.Id,
                    Name = $"{x.Event} {x.EventCategory}",
                    Area = area,
                    DurationInSlots = AthleticMeetScheduler.MinutesToSlotNo(x.EstimatedDuration),
                    EventType = x.Event
                };
            }).ToList();

            var participantNames = filteredGroupedParticipations.SelectMany(x => x.Participants).Select(y => $"{y.FirstName} {y.LastName}").Distinct().ToList();

            var participants = participantNames.Select(x =>
            {
                return new MeetScheduler.Participant()
                {
                    EventIds = filteredGroupedParticipations.Where(y => y.Participants.Select(z => $"{z.FirstName} {z.LastName}").Contains(x)).Select(q => q.Id).ToList(),
                    Name = x
                };
            }).ToList();

            // Identify how events can be grouped together by category.
            // Implement rules for how to group together. E.g. 60m G15-MS. Hekk: Høgde og ledig bane mellom.
            // Include time for innhopping.

            var (eventTimeSlots, solver, status) = MeetScheduler.AthleticMeetScheduler.GetMeetSchedule(events, participants, numSlots);
            MeetScheduler.AthleticMeetScheduler.PrintResult(eventTimeSlots, solver, status, events, participants, numSlots);
#endif

        }

        public void Save()
        {
            string timescheduleCSV = string.Join(
                        Environment.NewLine, // Delimiter to join each line
                        _groupedParticipations.Select(x =>
                        {
                            var abbreviation = _amCategory.GetAMAbbreviation(x.EventCategory);
                            var category = _categories
                                .Where(y => y.SAEventName == x.Event && y.AMCategoryAbbreviation == abbreviation)
                                .FirstOrDefault();
                            var amEventName = category?.AMEventName;
                            var saEventCategoryName = category?.SAEventCategoryName;

                            var sessionName = Session.GetName(abbreviation);
                            var eventName = $"{x.Event} {abbreviation}".Replace(" meter", "m");

                            return $"{sessionName};00:00:00|finale|1;{eventName};{x.Event};{abbreviation};{amEventName};{saEventCategoryName}";
                        }));


            string fullFileName = _path + "\\create\\timeschedule.csv";
        }
    }
}
