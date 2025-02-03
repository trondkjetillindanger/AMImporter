using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AMImporter;
using AMImporter.Callroom;


class EventImporter
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        Console.WriteLine(args[0]);
        if (args.Length > 1)
        {
            Console.WriteLine(args[1]);
            var filenameRelays = args[1];
        }
        var filename = args[0];
        FileInfo fileInfo = new FileInfo(filename);
        bool isCSV = fileInfo.Extension==".csv"?true:false;

        string currentDirectory = Path.GetDirectoryName(filename);
        List<iSonenParticipation> ISonenParticipations = null;

        string path = Path.GetFullPath(currentDirectory);
        Competition competition = new Competition(path);
        TimeSchedule timeSchedule = new TimeSchedule(path);
        if (timeSchedule.AutoGenerate)
        {
            ISonenParticipations = ISonenImporter.import(filename, null);
            ISonenParticipations = ISonenImporter.FixRelays(ISonenParticipations);
            ISonenImporter.FixRelays(ISonenParticipations);

            AMCategory amCategory = new AMCategory(path);
            List<EventTypeCategory> categories = CsvImporter.ImportEventTypeCategory(path);
            List<EventType> eventTypes = CsvImporter.ImportEventTypes(path);
            var englishEventTypes = eventTypes.Where(x => x.Language == "en").ToList<EventType>();
            int id = 0;

            var groupedParticipations = ISonenParticipations
                .Where(p => !string.IsNullOrEmpty(p.Event))
                .GroupBy(p => new { p.Event, p.EventCategory, EventDate = DateTime.Parse(p.EventDate) })
                .OrderBy(g => g.Key.EventDate)
                .ThenBy(g => g.Key.Event)  // Secondary ordering by Event
                .ThenBy(g => g.Key.EventCategory)  // Tertiary ordering by EventCategory
                .Select(g =>
                {
                    var eventType = englishEventTypes.Where(y => y.StandardName == (categories.Where(x => x.SAEventName == g.Key.Event).FirstOrDefault()?.AMEventName ?? "NA")).FirstOrDefault();

                    return new
                    {
                        Id = id++,
                        Event = g.Key.Event,
                        EventCategory = g.Key.EventCategory,
                        EventDate = g.Key.EventDate,
                        Participants = g.ToList(), // List of participants in this group
                        FieldType = eventType?.FieldType.Trim(),
                        EventAbbreviation = eventType?.StandardAbbreviation,
                        Distance = eventType?.Distance,
                        EstimatedDuration = AthleticEventTimeCalculator.EstimateEventTime(eventType?.StandardAbbreviation, eventType?.FieldType, g.Count(), eventType?.Distance)
                    };
                })
                .ToList();

            int takeCount = 75;
            var numSlots = 200;

            var events = groupedParticipations.Take(takeCount).Select(x =>
            {
                return new MeetScheduler.Event()
                {
                    Id = x.Id,
                    Name = $"{x.Event} {x.EventCategory}",
                    Area = $"{x.FieldType??"Running"}",
                    DurationInSlots = 2,
                    EventType = x.Event
                };
            }).ToList();
 
            var participantNames = groupedParticipations.Take(takeCount).SelectMany(x => x.Participants).Select(y => $"{y.FirstName} {y.LastName}").ToList();

            var participants = participantNames.Select(x =>
            {
                return new MeetScheduler.Participant()
                {
                    EventIds = groupedParticipations.Take(takeCount).Where(y => y.Participants.Select(z => $"{z.FirstName} {z.LastName}").Contains(x)).Select(q => q.Id).ToList(),
                    Name = x
                };
            }).ToList();

            var (eventTimeSlots, solver, status) = MeetScheduler.AthleticMeetScheduler.GetMeetSchedule(events, participants, numSlots);
            MeetScheduler.AthleticMeetScheduler.PrintResult(eventTimeSlots, solver, status, events, participants, numSlots);

            string timescheduleCSV = string.Join(
                        Environment.NewLine, // Delimiter to join each line
                        groupedParticipations.Select(x =>
                        {
                            var abbreviation = amCategory.GetAMAbbreviation(x.EventCategory);
                            var category = categories
                                .Where(y => y.SAEventName == x.Event && y.AMCategoryAbbreviation == abbreviation)
                                .FirstOrDefault();
                            var amEventName = category?.AMEventName;
                            var saEventCategoryName = category?.SAEventCategoryName;

                            var sessionName = Session.GetName(abbreviation);
                            var eventName = $"{x.Event} {abbreviation}".Replace(" meter", "m");

                            return $"{sessionName};00:00:00|finale|1;{eventName};{x.Event};{abbreviation};{amEventName};{saEventCategoryName}";
                        }));


            string fullFileName = path + "\\create\\timeschedule.csv";
            //CSVUtil.CreateNewCSV(fullFileName, timescheduleCSV);
            //timeSchedule = new TimeSchedule(path);
        }
        AMImporter.Event eventImporter = new AMImporter.Event();
        eventImporter.Create(path, competition, timeSchedule);
        eventImporter.CreateRound(path, competition, timeSchedule);
        EventCategory eventCategory = new EventCategory();
        eventCategory.Create(path, timeSchedule, competition);

        XElement root = null;

        if (!isCSV)
        {
            root = XElement.Load(filename);
            Team teams = new Team(root, path);
            Athlete athletes = new Athlete(root, teams);
            var teamNames = athletes.GetTeams();
            var missingTeams = teams.FindMissing(teamNames);
            teams.CreateMissing(missingTeams, path);
            athletes.Create(path);
            athletes.CreateLicense(path);
            athletes.CreateParticipation(path, timeSchedule, competition);
            athletes.CreateParticipationWithoutEvent(path, timeSchedule, competition);
            athletes.CreateCompetitor(path, competition);
            athletes.CreateRecord(path, timeSchedule);
            string zipfilename = $"{path}\\create.zip";
            File.Delete(zipfilename);
            ZipFile.CreateFromDirectory(path + "\\create", zipfilename);
        }
        else
        {
            ISonenParticipations = ISonenImporter.import(filename, null);
            ISonenParticipations = ISonenImporter.FixRelays(ISonenParticipations);
            ISonenImporter.FixRelays(ISonenParticipations);
            Team teams = new Team(ISonenParticipations, path);
            Athlete athletes = new Athlete(ISonenParticipations, teams, eventCategory.amCategory);
            var teamNames = athletes.GetTeams();
            var missingTeams = teams.FindMissing(teamNames);
            teams.CreateMissing(missingTeams, path);
            //athletes.AssignBib();
            athletes.Create(path);
            athletes.CreateLicense(path);
            athletes.CreateParticipation(path, timeSchedule, competition);
            athletes.CreateParticipationWithoutEvent(path, timeSchedule, competition);
            athletes.CreateCompetitor(path, competition);
            //athletes.CreateRecord(path, timeSchedule);
            string zipfilename = $"{path}\\create.zip";
            File.Delete(zipfilename);
            ZipFile.CreateFromDirectory(path + "\\create", zipfilename);
        }

        //CallroomData callroomData = new CallroomData();
        //callroomData.Create(path, timeSchedule, ISonenParticipations, eventCategory);
    }
}