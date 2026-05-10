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

        class ResultItem
        {
            public string Result { get; set; }
            public string Date { get; set; }
            public string Wind { get; set; } = "0.0";
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


        public async static Task<string?> GetAthleteIdAsync(string firstname, string lastname, string? birthDate)
        {
            var values = new Dictionary<string, string>
                          {
                              { "FirstName", firstname },
                              { "LastName", lastname },
                              { "DateOfBirth", birthDate ?? string.Empty },
                          };

            string json = JsonSerializer.Serialize(values);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await httpClient.PostAsync("http://www.minfriidrettsstatistikk.info/php/sokutover.php", content);

            var jsonString = await response.Content.ReadAsStringAsync();
            Athlete? athlete = JsonSerializer.Deserialize<List<Athlete>>(jsonString)?.FirstOrDefault();

            if (string.IsNullOrEmpty(athlete?.Athlete_Id))
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
            else if (records.PB != null)
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

        private async static Task<AMRecordDTO> GetPreviousYearAthleteRecordAsync(string athleteId, string eventName, bool isOutdoor)
        {
            var previousYear = DateTime.Now.Year - 1;
            var values = new Dictionary<string, string>
            {
                { "athlete", athleteId },
                { "type", "RES" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync("https://www.minfriidrettsstatistikk.info/php/UtoverStatistikk.php", content);
            var html = await response.Content.ReadAsStringAsync();
            var result = GetPreviousYearResultFromHtml(html, eventName, isOutdoor, previousYear);

            if (result == null)
            {
                Console.WriteLine($"No previous year stats found for {athleteId} in {eventName}.");
                return new AMRecordDTO();
            }

            return new AMRecordDTO
            {
                SB =
                {
                    Date = result.Date,
                    Time = result.Result,
                    Wind = result.Wind
                }
            };
        }

        private static ResultItem? GetPreviousYearResultFromHtml(string html, string eventName, bool isOutdoor, int year)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var expectedHeader = isOutdoor ? "UTENDØRS" : "INNENDØRS";
            var isInExpectedVenue = false;
            var isInExpectedEvent = false;

            foreach (var node in document.DocumentNode.Descendants())
            {
                if (node.Name == "h2")
                {
                    isInExpectedVenue = NormalizeText(node.InnerText) == NormalizeText(expectedHeader);
                    isInExpectedEvent = false;
                    continue;
                }

                if (node.Id == "eventheader")
                {
                    var header = node.Descendants("h3").FirstOrDefault()?.InnerText;
                    isInExpectedEvent = isInExpectedVenue && NormalizeText(header) == NormalizeText(eventName);
                    continue;
                }

                if (isInExpectedEvent && node.Name == "h4" && NormalizeText(node.InnerText).Contains("IKKEGODKJENTERESULTATER"))
                {
                    isInExpectedEvent = false;
                    continue;
                }

                if (!isInExpectedEvent || node.Name != "tr")
                {
                    continue;
                }

                var cells = node.Elements("td").ToList();
                if (cells.Count < 5)
                {
                    continue;
                }

                var yearText = WebUtility.HtmlDecode(cells[0].InnerText).Trim();
                if (!yearText.StartsWith(year.ToString(), StringComparison.Ordinal))
                {
                    continue;
                }

                return CreateResultItem(cells[1].InnerText, cells[4].InnerText);
            }

            return null;
        }

        private static ResultItem CreateResultItem(string resultText, string dateText)
        {
            var result = WebUtility.HtmlDecode(resultText).Trim();
            var wind = "0.0";
            var windMatch = Regex.Match(result, @"^(?<result>[^()]+)\((?<wind>[+-]?\d+,\d+)\)$");
            if (windMatch.Success)
            {
                result = windMatch.Groups["result"].Value.Trim();
                wind = windMatch.Groups["wind"].Value.Replace(',', '.');
            }

            return new ResultItem
            {
                Result = result,
                Date = NormalizeStatDate(WebUtility.HtmlDecode(dateText).Trim()),
                Wind = wind
            };
        }

        private static string NormalizeStatDate(string dateText)
        {
            if (DateTime.TryParseExact(dateText, new[] { "dd.MM.yy", "dd.MM.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            return dateText;
        }

        private static string NormalizeText(string? text)
        {
            return WebUtility.HtmlDecode(text ?? string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();
        }

        private static AMRecordDetailDTO GetBestPersonalRecord(string eventName, params AMRecordDTO[] records)
        {
            var candidates = records.Select(record => record.PB.Time != null ? record.PB : record.SB).Where(record => record.Time != null).ToList();
            if (!candidates.Any())
            {
                return new AMRecordDetailDTO();
            }

            return candidates.Aggregate((best, candidate) => IsBetterResult(eventName, candidate.Time, best.Time) ? candidate : best);
        }

        private static bool IsBetterResult(string eventName, string candidate, string currentBest)
        {
            if (!TryParseResult(candidate, out var candidateValue))
            {
                return false;
            }
            if (!TryParseResult(currentBest, out var currentBestValue))
            {
                return true;
            }

            return IsHigherResultBetter(eventName) ? candidateValue > currentBestValue : candidateValue < currentBestValue;
        }

        private static bool IsHigherResultBetter(string eventName)
        {
            return new[] { "Høyde", "Stav", "Lengde", "Tresteg", "Kule", "Diskos", "Slegge", "Spyd", "Liten Ball" }
                .Any(fieldEvent => eventName.StartsWith(fieldEvent, StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryParseResult(string result, out double value)
        {
            value = 0;
            var resultWithoutWind = Regex.Replace(result, @"\([^)]*\)", string.Empty).Trim();
            var parts = resultWithoutWind.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            if (parts.Length == 1)
            {
                return double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            }

            if (parts.Length == 2)
            {
                return double.TryParse(resultWithoutWind.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            }

            for (var index = 0; index < parts.Length - 1; index++)
            {
                if (!double.TryParse(parts[index], NumberStyles.Number, CultureInfo.InvariantCulture, out var timePart))
                {
                    return false;
                }
                value = value * 60 + timePart;
            }

            var decimals = parts.Last();
            if (!double.TryParse(decimals, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalPart))
            {
                return false;
            }

            value += decimalPart / Math.Pow(10, decimals.Length);
            return true;
        }

        private static bool ShouldSkipStatsLookup(string ageCode)
        {
            var match = Regex.Match(ageCode ?? string.Empty, @"\d+");
            return match.Success && int.TryParse(match.Value, out var age) && age <= 10;
        }

        private static string? GetAthleteIdFromApi(string firstname, string lastname, string? birthDate)
        {
            foreach (var dateCandidate in GetBirthDateCandidates(birthDate))
            {
                foreach (var nameCandidate in GetNameCandidates(firstname, lastname))
                {
                    var athleteId = GetAthleteIdAsync(nameCandidate.FirstName, nameCandidate.LastName, dateCandidate).Result;
                    if (athleteId != null)
                    {
                        return athleteId;
                    }
                }
            }

            return null;
        }

        private static string? GetAthleteIdFromHtmlSearch(string firstname, string lastname, string? birthDate)
        {
            foreach (var nameCandidate in GetNameCandidates(firstname, lastname))
            {
                var athleteId = GetAthleteId(nameCandidate.FirstName, nameCandidate.LastName, birthDate);
                if (athleteId != null)
                {
                    return athleteId;
                }
            }

            return null;
        }

        private static bool IsBirthDateMatch(string? expectedBirthDate, string actualBirthDate)
        {
            if (string.IsNullOrWhiteSpace(expectedBirthDate) || string.IsNullOrWhiteSpace(actualBirthDate))
            {
                return true;
            }

            var normalizedActualBirthDate = actualBirthDate.Trim();
            if (DateTime.TryParseExact(expectedBirthDate.Trim(), new[] { "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var expectedDate))
            {
                return normalizedActualBirthDate == expectedDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                    || normalizedActualBirthDate == expectedDate.Year.ToString(CultureInfo.InvariantCulture);
            }

            return normalizedActualBirthDate == expectedBirthDate.Trim();
        }

        private static bool IsAthleteNameMatch(string actualName, string firstName, string lastName)
        {
            var expectedName = $"{lastName}, {firstName}";
            return NormalizeComparableName(actualName) == NormalizeComparableName(expectedName);
        }

        private static string NormalizeComparableName(string? name)
        {
            return Regex.Replace(WebUtility.HtmlDecode(name ?? string.Empty), @"[\s.]", string.Empty).Trim().ToUpperInvariant();
        }

        private static IEnumerable<string?> GetBirthDateCandidates(string? birthDate)
        {
            if (string.IsNullOrWhiteSpace(birthDate))
            {
                yield return string.Empty;
                yield break;
            }

            var trimmedBirthDate = birthDate.Trim();
            yield return trimmedBirthDate;

            if (DateTime.TryParseExact(trimmedBirthDate, new[] { "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedBirthDate))
            {
                var norwegianDate = parsedBirthDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                if (norwegianDate != trimmedBirthDate)
                {
                    yield return norwegianDate;
                }
            }
        }

        private static IEnumerable<(string FirstName, string LastName)> GetNameCandidates(string firstname, string lastname)
        {
            var candidates = new List<(string FirstName, string LastName)>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cleanFirstName = NormalizeName(firstname);
            var cleanLastName = NormalizeName(lastname);
            var firstNameParts = SplitName(cleanFirstName);
            var lastNameParts = SplitName(cleanLastName);

            AddNameCandidate(cleanFirstName, cleanLastName);

            if (firstNameParts.Length > 1)
            {
                AddNameCandidate(firstNameParts[0], cleanLastName);
            }

            if (lastNameParts.Length > 1)
            {
                AddNameCandidate(cleanFirstName, lastNameParts.Last());
                AddNameCandidate(cleanFirstName, lastNameParts.First());
                if (firstNameParts.Length > 0)
                {
                    AddNameCandidate(firstNameParts[0], lastNameParts.Last());
                    AddNameCandidate(firstNameParts[0], lastNameParts.First());
                }
            }

            var allNameParts = firstNameParts.Concat(lastNameParts).ToArray();
            if (allNameParts.Length > 2)
            {
                for (var index = 1; index < allNameParts.Length; index++)
                {
                    AddNameCandidate(allNameParts[0], allNameParts[index]);
                    AddNameCandidate(string.Join(" ", allNameParts.Take(index)), string.Join(" ", allNameParts.Skip(index)));
                }
            }

            return candidates;

            void AddNameCandidate(string firstNameCandidate, string lastNameCandidate)
            {
                firstNameCandidate = NormalizeName(firstNameCandidate);
                lastNameCandidate = NormalizeName(lastNameCandidate);
                if (string.IsNullOrEmpty(firstNameCandidate) || string.IsNullOrEmpty(lastNameCandidate))
                {
                    return;
                }

                var key = $"{firstNameCandidate}|{lastNameCandidate}";
                if (seen.Add(key))
                {
                    candidates.Add((firstNameCandidate, lastNameCandidate));
                }
            }
        }

        private static string[] SplitName(string name)
        {
            return name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string NormalizeName(string? name)
        {
            return Regex.Replace(name ?? string.Empty, @"\s+", " ").Trim();
        }

        public static AMRecordDTO GetAthleteRecord(string firstname, string lastname, string eventName, string ageCode, string birthDate, bool isOutdoor = true, bool useApi = true)
        {
            if (ShouldSkipStatsLookup(ageCode))
            {
                return new AMRecordDTO();
            }
            Thread.Sleep(1500);
            string? athleteId = null;

            if (useApi)
            {
                athleteId = GetAthleteIdFromApi(firstname, lastname, birthDate);
                if (athleteId == null)
                {
                    athleteId = GetAthleteIdFromHtmlSearch(firstname, lastname, birthDate);
                }
            }
            else {
                athleteId = GetAthleteIdFromHtmlSearch(firstname, lastname, birthDate);
            }

            if (athleteId == null)
            {
                Console.WriteLine($"No stats found for {firstname} {lastname}. Athlete is not found.");
                return new AMRecordDTO();
            }
            //var athleteSB = GetValidAthleteSBPrioritized(athleteId, eventName, false);
            var athleteRecord = GetAthleteRecordAsync(athleteId, eventName, isOutdoor).Result;
            var backupAthleteRecord = GetAthleteRecordAsync(athleteId, eventName, !isOutdoor).Result;
            athleteRecord.PB = GetBestPersonalRecord(eventName, athleteRecord, backupAthleteRecord);
            if (athleteRecord.SB.Time == null)
            {
                if (backupAthleteRecord.SB.Time != null)
                {
                    athleteRecord.SB = backupAthleteRecord.SB;
                }
            }
            if (athleteRecord.SB.Time == null)
            {
                athleteRecord.SB = GetPreviousYearAthleteRecordAsync(athleteId, eventName, isOutdoor).Result.SB;
            }
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

        private static string? GetAthleteId(string firstname, string lastname, string? birthDate = null)
        {
            using (WebClient web1 = new WebClient())
            {
                string myParameters = $"cmd=SearchAthlete&showathlete={lastname}";
                web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string? Html = null;
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
                        var actualName = WebUtility.HtmlDecode(att.OwnerNode.InnerText).Trim();
                        var actualBirthDate = WebUtility.HtmlDecode(link.ParentNode?.NextSibling?.InnerText ?? string.Empty).Trim();
                        if (IsAthleteNameMatch(actualName, firstname, lastname) && IsBirthDateMatch(birthDate, actualBirthDate))
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
