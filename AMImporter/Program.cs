using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AMImporter;
using AMImporter.Callroom;
using CsvHelper;
using CsvHelper.Configuration;

class EventImporter
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        Console.WriteLine(args[0]);
        var filename = args[0];
        FileInfo fileInfo = new FileInfo(filename);
        bool isCSV = fileInfo.Extension==".csv"?true:false;

        string currentDirectory = Path.GetDirectoryName(filename);

        string path = Path.GetFullPath(currentDirectory);
        Competition competition = new Competition(path);
        TimeSchedule timeSchedule = new TimeSchedule(path);
        AMImporter.Event eventImporter = new AMImporter.Event();
        eventImporter.Create(path, competition, timeSchedule);
        eventImporter.CreateRound(path, competition, timeSchedule);
        EventCategory eventCategory = new EventCategory();
        eventCategory.Create(path, timeSchedule, competition);

        XElement root = null;
        List<iSonenParticipationDTO> ISonenParticipations = null;

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
            ISonenParticipations = ISonenImporter.import(filename);
            Team teams = new Team(ISonenParticipations, path);
            Athlete athletes = new Athlete(ISonenParticipations, teams, eventCategory.amCategory);
            var teamNames = athletes.GetTeams();
            var missingTeams = teams.FindMissing(teamNames);
            teams.CreateMissing(missingTeams, path);
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

        AMImporter.Callroom.EventList eventList = new AMImporter.Callroom.EventList();
        eventList.events = timeSchedule.AMEvents.Values.Select(x =>
        {
            var eventParticipations = ISonenParticipations.Where(y => y.Event == x.SAEventCategoryName && x.AgeCategory.Select(z => eventCategory.amCategory.GetAMName(z)).Contains(y.EventCategory)).ToList();
            var date = eventParticipations.Any() ? eventParticipations.Select(y => y.EventDate).First() : null;
            if (date == null)
            {
                return new AMImporter.Callroom.@Event()
                {
                    @event = x.Name,
                    event_time = "null"
                };
            };

            DateTime dateTime = DateTime.ParseExact(date, "dd.mm.yyyy", CultureInfo.InvariantCulture).Add(TimeSpan.ParseExact(x.Time, "hh\\:mm\\:ss", CultureInfo.InvariantCulture));
            //string dateTimeString = $"{date}T{x.Time}";mm:
            //DateTime dateTime = DateTime.Parse(dateTimeString);
            dateTime.AddHours(2);
            return new AMImporter.Callroom.@Event()
            {
                 @event = x.Name,
                agegroup = String.Join(", ", x.AgeCategory),
                callroomregistration_closed_time = dateTime.AddHours(2).AddMinutes(5).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                callroomregistration_open_time = dateTime.AddDays(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                callroomregistration_time = dateTime.AddHours(2).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                competition_id = "1",
                enterStadium_time = "null",
                event_id = "1",
                event_time = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
            };
        }).ToList<AMImporter.Callroom.@Event>();

        AMImporter.Callroom.ParticipantList participantList = new AMImporter.Callroom.ParticipantList();

        participantList.participants = ISonenParticipations.GroupBy(x => $"{x.FirstName} {x.LastName}")
                            .Select(y => {
                                return new AMImporter.Callroom.Participant() {
                                    participant_name = y.Key,
                                    participant_bib = "1",
                                    participant_id = "1",
                                    participant_team = y.First().Team,
                                    competition_id = "1",
                                    email = y.First().Email,
                                    email2 = y.First().EmailRegistrant,
                                    events = y.Select(q =>
                                    {
                                        return new AMImporter.Callroom.Participation()
                                        {
                                            event_id = q.Event,
                                            callroomstatus = false,
                                            callroomstatus_modificationtime = "null",
                                            year_best = "null",
                                            personal_best = "null"
                                        };
                                    }).ToList()                              
                                };
                             }).ToList();

        CallroomData callroomData = new CallroomData()
        {
            events = eventList.events,
            participants = participantList.participants
        };

        string json = JsonSerializer.Serialize(callroomData);

        File.WriteAllText(path + "\\create\\callroom.json", json);
    }
}