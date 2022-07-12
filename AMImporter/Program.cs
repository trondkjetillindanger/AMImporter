using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AMImporter;

class Program
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        Console.WriteLine(args[0]);
        var filename = args[0];

        string currentDirectory = Path.GetDirectoryName(filename);

        string path = Path.GetFullPath(currentDirectory);
        TimeSchedule timeSchedule = new TimeSchedule(path);

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
        string zipfilename = $"{path}\\create.zip";
        File.Delete(zipfilename);
        ZipFile.CreateFromDirectory(path+"\\create", zipfilename);
    }
}