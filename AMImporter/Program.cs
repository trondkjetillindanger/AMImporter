using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AMImporter;
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
        eventCategory.Create(path, timeSchedule);

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
            athletes.CreateParticipation(path, timeSchedule);
            athletes.CreateParticipationWithoutEvent(path, timeSchedule);
            athletes.CreateCompetitor(path);
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
            athletes.CreateParticipation(path, timeSchedule);
            athletes.CreateParticipationWithoutEvent(path, timeSchedule);
            athletes.CreateCompetitor(path);
            athletes.CreateRecord(path, timeSchedule);
            string zipfilename = $"{path}\\create.zip";
            File.Delete(zipfilename);
            ZipFile.CreateFromDirectory(path + "\\create", zipfilename);
        }
    }
}