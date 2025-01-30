using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AMImporter
{
    public static class RecordImporter
    {
        private static readonly HttpClient httpClient = new HttpClient();

        class RecordItem
        {
            public string Result { get; set; }
            public string Date { get; set; }
        }
        class Records
        {
            public RecordItem PB { get; set; }
            public RecordItem SB { get; set; }
        }

        class Athlete
        {
            public string? Athlete_Id { get; set; }
            public string? OtAthleteId { get; set; }
            public string? FirstName { get; set; }
            public string? MiddleName { get; set; }
            public string? LastName { get; set; }
            public string? DateOfBirth { get; set; }
            public string? Gender { get; set; }
            public string? Nationality { get; set; }
        }


        public async static Task<string?> GetAthleteIdAsync(string firstname, string lastname, string birthDate)
        {
            var values = new Dictionary<string, string>
                          {
                              { "FirstName", firstname },
                              { "LastName", lastname },
                              { "DateOfBirth", birthDate },
                          };

            string json = JsonSerializer.Serialize(values);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await httpClient.PostAsync("http://www.minfriidrettsstatistikk.info/php/sokutover.php", content);

            var jsonString = await response.Content.ReadAsStringAsync();
            Athlete athlete = JsonSerializer.Deserialize<List<Athlete>>(jsonString).FirstOrDefault();

            if (string.IsNullOrEmpty(athlete.Athlete_Id))
            {
                return null;
            }
            return athlete.Athlete_Id;
        }



        public async static Task<AMRecordDTO> GetAthleteRecordAsync(string athleteId, string eventName, bool? isOutdoor)
        {
            var events = new Dictionary<string, string>
                          {
                              { "60 meter", "2" },
                              { "100 meter", "4" },
                              { "200 meter", "5" },
                              { "400 meter", "7" },
                              { "600 meter", "8" },
                              { "800 meter", "9" },
                              { "1500 meter", "11" },
                              { "3000 meter", "13" },
                              { "5000 meter", "14" },
                              { "10000 meter", "15" },
                              { "60 meter hekk (68,0cm)","19" },
                              { "60 meter hekk (76,2cm)","20" },
                              { "60 meter hekk (84,0cm)","21" },
                              { "60 meter hekk (91,4cm)","22" },
                              { "60 meter hekk (100cm)","23" },
                              { "60 meter hekk (106,7 cm)","24" },
                              { "Kappgang 1500 meter","151" },
                              { "Kappgang 3000 meter", "122" },
                              { "100 meter hekk (84,0cm)", "35" },
                              { "110 meter hekk (100cm)", "41" },
                              { "110 meter hekk (106,7cm)", "42" },
                              { "400 meter hekk (76,2cm)", "57" },
                              { "400 meter hekk (91,4cm)", "59" },
                              { "3000 meter hinder (76,2cm)", "120" },
                              { "3000 meter hinder (91,4cm)", "121" },
                              { "Høyde", "68" },
                              { "Stav", "70" },
                              { "Lengde", "71" },
                              { "Lengde (Sone 0,5m)", "72" },
                              { "Tresteg", "75" },
                              { "Kule 2,0Kg", "81" },
                              { "Kule 3,0Kg", "82" },
                              { "Kule 4,0Kg", "83" },
                              { "Kule 5,0Kg", "84" },
                              { "Kule 6,0Kg", "85" },
                              { "Kule 7,26Kg", "86" },
                              { "Diskos 600gram", "88" },
                              { "Diskos 750gram", "89" },
                              { "Diskos 1,0Kg", "90" },
                              { "Diskos 1,5Kg", "91" },
                              { "Diskos 1,75Kg", "92" },
                              { "Diskos 2,0Kg", "93" },
                              { "Slegge 2,0Kg", "101" },
                              { "Slegge 4,0Kg", "103" },
                              { "Slegge 5,0Kg", "104" },
                              { "Slegge 6,0Kg", "105" },
                              { "Slegge 7,26Kg", "106" },
                              { "Spyd 400gram", "95" },
                              { "Spyd 500gram", "139" },
                              { "Spyd 600gram", "96" },
                              { "Spyd 700gram", "97" },
                              { "Spyd 800gram", "98" },
                              { "Liten Ball 150gram", "109" }
                          };

            var eventId = events.GetValueOrDefault(eventName);
            if (eventId == null)
            {
                return new AMRecordDTO();
            }

            var values = new Dictionary<string, string>
                          {
                              { "Athlete_Id", athleteId },
                              { "Event_Id", eventId }
                          };

            if (isOutdoor != null)
            {
                values = new Dictionary<string, string>
                              {
                                  { "Athlete_Id", athleteId },
                                  { "Event_Id", eventId },
                                  { "Outdoor", isOutdoor.Value?"Y":"N" }
                              };
            }

            string json = JsonSerializer.Serialize(values);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await httpClient.PostAsync("http://www.minfriidrettsstatistikk.info/php/hentresultater.php", content);

            var jsonString = await response.Content.ReadAsStringAsync();
            Records records = JsonSerializer.Deserialize<Records>(jsonString);

            if (records == null || (records.PB == null && records.SB == null))
            {
                Console.WriteLine($"No stats found for {athleteId} in {eventName}.");
                return new AMRecordDTO();
                //if (records.PB != null && new string[] { "4" }.Contains(eventId))
                //{
                //    records.SB = records.PB;
                //}
                //else
                //{
                //    Console.WriteLine($"No stats found for {athleteId} in {eventName}.");
                //    return new AMRecordDTO();
                //}
            }
            else
            {
                DateTime pbDate = DateTime.ParseExact(records.PB.Date, "dd.MM.yyyy", null);
                DateTime? sbDate = null;
                if (records.SB != null)
                {
                    sbDate = DateTime.ParseExact(records.SB.Date, "dd.MM.yyyy", null);
                }
                int currentYear = DateTime.Now.Year;

                if ((pbDate > sbDate || sbDate == null) && pbDate.Year == currentYear)
                {
                    records.SB = records.PB;
                }
            }
            return new AMRecordDTO() {                
                SB = {
                    Date = records.SB?.Date,
                    Time = records.SB?.Result,
                    Wind = "0.0"
                },
                PB =
                {
                    Date = records.PB?.Date,
                    Time = records.PB?.Result,
                    Wind = "0.0"
                }
            };
        }

        public static AMRecordDTO GetAthleteRecord(string firstname, string lastname, string eventName, string ageCode, string birthDate, bool useApi = true)
        {
            if (new string[] { "G10", "J10" }.Contains(ageCode))
            {
                return new AMRecordDTO();
            }
            Thread.Sleep(750);
            string[] names = firstname.Split(' ');
            string? athleteId = null;

            if (useApi)
            {
                athleteId = GetAthleteIdAsync(firstname, lastname, birthDate).Result;
                if (athleteId == null && names[0] != firstname)
                {
                    athleteId = GetAthleteIdAsync(names[0], lastname, birthDate).Result; // All names might not have been included in stats.
                    if (athleteId == null)
                    {
                        athleteId = GetAthleteIdAsync(names[0], names[1], birthDate).Result; // Different order of last names might have been used.
                    }
                    if (athleteId == null && names.Length >= 3)
                    {
                        athleteId = GetAthleteIdAsync(names[0], names[2], birthDate).Result; // Different order of last names might have been used.
                    }
                }
            }
            else {
                athleteId = GetAthleteId(firstname, lastname);

                if (athleteId == null && names[0] != firstname)
                {
                    athleteId = GetAthleteId(names[0], lastname); // All names might not have been included in stats.
                    if (athleteId == null)
                    {
                        athleteId = GetAthleteId(names[0], names[1]); // Different order of last names might have been used.
                    }
                    if (athleteId == null && names.Length >= 3)
                    {
                        athleteId = GetAthleteId(names[0], names[2]); // Different order of last names might have been used.
                    }
                }
            }

            if (athleteId == null)
            {
                Console.WriteLine($"No stats found for {firstname} {lastname}. Athlete is not found.");
                return new AMRecordDTO();
            }
            //var athleteSB = GetValidAthleteSBPrioritized(athleteId, eventName, false);
            var athleteRecord = GetAthleteRecordAsync(athleteId, eventName, false).Result;
            //if (eventName == "Lengde") {
            //    var athleteSBLengdeSone = GetAthleteRecordAsync(athleteId, "Lengde(Sone 0, 5m)", null).Result;
            //    if (athleteSB.SB.Time==null)
            //    {
            //        return athleteSBLengdeSone;
            //    }
            //    CultureInfo culture = CultureInfo.InvariantCulture;
            //    if (athleteSBLengdeSone.SB.Time != null && double.Parse(athleteSBLengdeSone.SB.Time, culture)>double.Parse(athleteSB.SB.Time, culture))
            //    {
            //        return athleteSBLengdeSone;
            //    }
            //}

            return athleteRecord;
        }

        private static string GetAthleteId(string firstname, string lastname)
        {
            string fullname = lastname.TrimEnd() + ", " + firstname.TrimEnd();
            using (WebClient web1 = new WebClient())
            {
                string myParameters = $"cmd=SearchAthlete&showathlete={lastname}";
                web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = null;
                try { 
                    Html = web1.UploadString(baseUrl+"/UtoverSok.php", myParameters);
                }
                catch (Exception e)
                {
                    Thread.Sleep(5000); // Wait and try again.
                    Html = web1.UploadString(baseUrl + "/UtoverSok.php", myParameters);
                }

                HtmlDocument htmlSnippet = new HtmlDocument();
                htmlSnippet.LoadHtml(Html);
                var links = htmlSnippet.DocumentNode.SelectNodes("//a[@href]");

                if (links != null)
                {
                    foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        var fullnameFromStat = att.OwnerNode.InnerHtml.Replace(" ", "");
                        var names = att.OwnerNode.InnerHtml.Split(' ').Select(x => x.TrimEnd(',')).ToArray<string>();
                        string alt1FullnameFromStat = $"{names[0]},{names[1]}";
                        string alt2FullnameFromStat = $"{names[2]},{names[1]}";

                        if (fullnameFromStat.ToLower() == fullname.Replace(" ", "").ToLower() || fullnameFromStat.ToLower() == fullname.Substring(0, fullname.LastIndexOf(" ") + 2).Replace(" ","").ToLower())
                        {
                            Uri uri = new Uri(new Uri(baseUrl), att.Value);
                            string queryString = uri.Query;
                            var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                            return queryDictionary["showathl"];
                        }
                        if (alt1FullnameFromStat.ToLower() == fullname.Replace(" ", "").ToLower() || alt1FullnameFromStat.ToLower() == fullname.Substring(0, fullname.LastIndexOf(" ") + 2).Replace(" ", "").ToLower())
                        {
                            Uri uri = new Uri(new Uri(baseUrl), att.Value);
                            string queryString = uri.Query;
                            var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                            return queryDictionary["showathl"];
                        }
                        if (alt2FullnameFromStat.ToLower() == fullname.Replace(" ", "").ToLower() || alt2FullnameFromStat.ToLower() == fullname.Substring(0, fullname.LastIndexOf(" ") + 2).Replace(" ", "").ToLower())
                        {
                            Uri uri = new Uri(new Uri(baseUrl), att.Value);
                            string queryString = uri.Query;
                            var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                            return queryDictionary["showathl"];
                        }
                    }
                }
            }
            return null;
        }
    }
}
