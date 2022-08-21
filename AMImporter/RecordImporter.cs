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
            return GetAthleteSBById(athleteId, eventName);
        }

        private static string GetAthleteId(string firstname, string lastname)
        {
            string fullname = lastname.TrimEnd() + ", " + firstname.TrimEnd();
            using (WebClient web1 = new WebClient())
            {
                string myParameters = $"cmd=SearchAthlete&showathlete={lastname}";
                web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = web1.UploadString(baseUrl+"/UtoverSok.php", myParameters);


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

        private static AMRecordDTO GetAthleteSBById(string athleteId, string eventName)
        {
            AMRecordDTO record = new AMRecordDTO();
            using (WebClient web1 = new WebClient())
            {
                string myParameters = "listtype=Best&outdoor=Y&showseason=2022&showevent=0&showathl=" + athleteId;
                //web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = null;

                try
                {
                    Html = web1.DownloadString(baseUrl + "/UtoverStatistikk.php?" + myParameters);
                }
                catch (Exception e)
                {
                    Thread.Sleep(5000); // Wait and try again.
                    Html = web1.DownloadString(baseUrl + "/UtoverStatistikk.php?" + myParameters);
                }

                HtmlDocument htmlSnippet = new HtmlDocument();
                htmlSnippet.LoadHtml(Html);
                string result = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table//td[2]/text()")?.FirstOrDefault()?.GetDirectInnerText();

                if (result != null)
                {
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
                            }
                            else
                            {
                                return new AMRecordDTO();
                            }
                        }
                        else
                        {
                            return new AMRecordDTO();
                        }
                    }
                    else
                    {
                        record.Time = result;
                    }

                    string dateValue = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table//td[5]/text()").FirstOrDefault().GetDirectInnerText();
                    string datePattern = "dd.MM.yy";
                    string date = null;
                    DateTime parsedDate;
                    if (DateTime.TryParseExact(dateValue, datePattern, null,
                                                          DateTimeStyles.None, out parsedDate))
                    {
                        record.Date = parsedDate.ToString("yyyy-MM-dd");
                    }
                }
                else
                {
                    Console.WriteLine($"No result found for athlete id {athleteId}  in event {eventName}.");
                }
            }
            return record;
        }
    }
}
