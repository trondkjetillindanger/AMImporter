using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AMImporter
{
    public static class RecordImporter
    {
        public static AMRecordDTO GetAthleteSB(string firstname, string lastname, string eventName, string ageCode)
        {
            if (new string[] { "G10", "J10" }.Contains(ageCode))
            {
                return new AMRecordDTO();
            }
            //Thread.Sleep(500);
            string[] names = firstname.Split(' ');

            string athleteId = GetAthleteId(firstname, lastname);

            if (athleteId == null && names[0]!=firstname)
            {
                athleteId = GetAthleteId(names[0], lastname); // All names might not have been included in stats.
                if (athleteId == null)
                {
                    athleteId = GetAthleteId(names[0], names[1]); // Different order of last names might have been used.
                }
                if (athleteId == null && names.Length>=3)
                {
                    athleteId = GetAthleteId(names[0], names[2]); // Different order of last names might have been used.
                }
            }

            if (athleteId == null)
            {
                Console.WriteLine($"No stats found for {firstname} {lastname}. Athlete is not found.");
                return new AMRecordDTO();
            }
            var athleteSB = GetAthleteSBById(athleteId, eventName, true);
            if (athleteSB.IllegalWind != null && athleteSB.IllegalWind.Value)
            {
                athleteSB = GetAthleteSBById(athleteId, eventName, false);
            }
            if (athleteSB.IllegalWind != null && athleteSB.IllegalWind.Value)
            {
                return new AMRecordDTO();
            }
            return athleteSB;
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
                        if (att.OwnerNode.InnerHtml.Replace(" ", "") == fullname.Replace(" ", "") || att.OwnerNode.InnerHtml.Replace(" ", "") == fullname.Substring(0, fullname.LastIndexOf(" ") + 2).Replace(" ",""))
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

        private static AMRecordDTO Parse(string result)
        {
            var record = new AMRecordDTO();
            int windStartIndex = result.IndexOf('(');
            int windEndIndex = result.IndexOf(')');

            if (windStartIndex > -1)
            {
                record.Time = result.Substring(0, windStartIndex);
                string Wind = result.Substring(windStartIndex + 1, windEndIndex - windStartIndex - 1);
                if (double.TryParse(Wind, out double number))
                {
                    if (number <= 2.0)
                    {
                        record.Wind = Wind;
                        record.IllegalWind = false;
                    }
                    else
                    {
                        record.Wind = Wind;
                        record.IllegalWind = true;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                record.Time = result;
            }
            return record;
        }

        private static AMRecordDTO GetAthleteSBById(string athleteId, string eventName, bool bestOnly = true)
        {
            AMRecordDTO record = null;
            using (WebClient web1 = new WebClient())
            {
                string myParameters = "listtype=All&outdoor=Y&showseason=2022&showevent=0&showathl=" + athleteId;
                if (bestOnly)
                {
                    myParameters = "listtype=Best&outdoor=Y&showseason=2022&showevent=0&showathl=" + athleteId;
                }
                //web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = null;

                bool isLoaded = false;
                do
                {
                    try
                    {
                        Html = web1.DownloadString(baseUrl + "/UtoverStatistikk.php?" + myParameters);
                        isLoaded = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Can not contact stats server. Waiting a bit and trying again...");
                        isLoaded = false;
                        Thread.Sleep(5000);
                    }                   
                } while (!isLoaded);


                HtmlDocument htmlSnippet = new HtmlDocument();
                htmlSnippet.LoadHtml(Html);
                var eventResultNode = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table")?.FirstOrDefault();
                int? numberOfResultsForEvent = eventResultNode?.SelectNodes(".//tr")?.Count();
                string result = null;
                for (int i=2;i<= numberOfResultsForEvent;i++)
                {
                    result = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table/tr[{i}]/td[2]/text()")?.FirstOrDefault()?.GetDirectInnerText();
                    record = Parse(result);
                    if (record != null && (record.IllegalWind==null || !record.IllegalWind.Value))
                    {
                        string dateValue = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table/tr[{i}]/td[5]/text()").FirstOrDefault().GetDirectInnerText();
                        string datePattern = "dd.MM.yy";
                        string date = null;
                        DateTime parsedDate;
                        if (DateTime.TryParseExact(dateValue, datePattern, null,
                                                              DateTimeStyles.None, out parsedDate))
                        {
                            record.Date = parsedDate.ToString("yyyy-MM-dd");
                        }
                        break;
                    }
                }
              
                if (record == null)
                {
                    Console.WriteLine($"No result found for athlete id {athleteId}  in event {eventName}.");
                    return new AMRecordDTO();
                }
            }
            return record;
        }
    }
}
