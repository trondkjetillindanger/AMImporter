using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AMImporter
{
    public static class RecordImporter
    {
        public static (string, string) GetAthleteSB(string firstname, string lastname, string eventName)
        {
            string[] names = firstname.Split(' ');

            string athleteId = GetAthleteId(firstname, lastname);

            if (athleteId == null && names[0]!=firstname)
            {
                athleteId = GetAthleteId(names[0], lastname); // All names might not have been included in stats.
            }

            if (athleteId == null)
            {
                Console.WriteLine($"No stats found for {firstname} {lastname}");
                return ("", "");
            }
            return GetAthleteSBById(athleteId, eventName);
        }

        private static string GetAthleteId(string firstname, string lastname)
        {
            string fullname = lastname + ", " + firstname;
            using (WebClient web1 = new WebClient())
            {
                string myParameters = $"cmd=SearchAthlete&showathlete={lastname}";
                web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = web1.UploadString(baseUrl+"/UtoverSok.php", myParameters);


                HtmlDocument htmlSnippet = new HtmlDocument();
                htmlSnippet.LoadHtml(Html);

                foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.OwnerNode.InnerHtml.Replace(" ", "") == fullname.Replace(" ", ""))
                    {
                        Uri uri = new Uri(new Uri(baseUrl), att.Value);
                        string queryString = uri.Query;
                        var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                        return queryDictionary["showathl"];
                    }
                }
            }
            return null;
        }

        private static (string,string) GetAthleteSBById(string athleteId, string eventName)
        {
            using (WebClient web1 = new WebClient())
            {
                string myParameters = "listtype=Best&outdoor=Y&showseason=2022&showevent=0&showathl=" + athleteId;
                //web1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string baseUrl = "https://www.minfriidrettsstatistikk.info/php";
                string Html = web1.DownloadString(baseUrl + "/UtoverStatistikk.php?"+myParameters);


                HtmlDocument htmlSnippet = new HtmlDocument();
                htmlSnippet.LoadHtml(Html);
                string result = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table//td[2]/text()").FirstOrDefault().GetDirectInnerText();
                string dateValue = htmlSnippet.DocumentNode.SelectNodes($"//div[@id='Øvelse' and h4='{eventName}']/table//td[5]/text()").FirstOrDefault().GetDirectInnerText();
                string pattern = "dd.MM.yy";
                string date = null;
                DateTime parsedDate;
                if (DateTime.TryParseExact(dateValue, pattern, null,
                                                      DateTimeStyles.None, out parsedDate))
                {
                    date = parsedDate.ToString("yyyy-MM-dd");
                }

                return (result, date);
            }
            return (null, null);
        }
    }
}
