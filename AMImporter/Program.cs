using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AMImporter;

class EventImporter
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        Console.WriteLine(args[0]);
        var filename = args[0];

        string currentDirectory = Path.GetDirectoryName(filename);

        string path = Path.GetFullPath(currentDirectory);
        Competition competition = new Competition(path);
        TimeSchedule timeSchedule = new TimeSchedule(path);
        AMImporter.Event eventImporter = new AMImporter.Event();
        eventImporter.Create(path, competition, timeSchedule);
        eventImporter.CreateRound(path, competition, timeSchedule);
        EventCategory eventCategory = new EventCategory();
        eventCategory.Create(path, timeSchedule);

        XElement root = XElement.Load(filename);
        Team teams = new Team(root, path);
        Athlete athletes = new Athlete(root, teams);
        var teamNames = athletes.GetTeams();
        var missingTeams = teams.FindMissing(teamNames);
        teams.CreateMissing(missingTeams, path);
        athletes.Create(path);
        athletes.CreateLicense(path);
        athletes.CreateParticipation(path, timeSchedule);
        athletes.CreateCompetitor(path);
        athletes.CreateRecord(path, timeSchedule);
        string zipfilename = $"{path}\\create.zip";
        File.Delete(zipfilename);
        ZipFile.CreateFromDirectory(path+"\\create", zipfilename);
    }
}