using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class GSheetParticipationMap : ClassMap<iSonenParticipation>
    {
        public GSheetParticipationMap()
        {
            Map(p => p.Tidsmerke).Name("Tidsmerke");
            Map(p => p.Email).Name("E-postadresse");
            Map(p => p.FirstName).Name("Fornamn");
            Map(p => p.LastName).Name("Etternamn");
            Map(p => p.BirthDate).Name("Fødselsdag", "Fødselsdag (dd.mm.yyyy)");
            //Map(p => p.Gender).Name("Kjønn");
            Map(p => p.Team).Name("Velg skole");
            //Map(p => p.TeamId).Name("Velg skole");
            //Map(p => p.GroupId).Name("Gruppe-ID");
            //Map(p => p.License).Name("Person ID");
            //Map(p => p.Event).Name("Ønsker å delta i (maks to øvingar)").NameIndex(2);
            Map(p => p.EventCategory).Name("Velg klasse og kjønn");
            //Map(p => p.EventDate).Name("Dato");
            //Map(p => p.EmailRegistrant).Name("E-post påmelder");
            //Map(p => p.Email).Name("E-post");
            //Map(p => p.Id).Name("Person ID");
            Map(p => p.Event).Convert(row =>
            {
                var csvRow = row.Row;
                if (csvRow == null)
                {
                    return string.Empty;
                }

                var categoryAndGender = csvRow.GetField("Velg klasse og kjønn") ?? string.Empty;

                // Decide index based on substring in the event
                int index = categoryAndGender.Contains("5.klasse (jente)") ? 0 :
                            categoryAndGender.Contains("6.klasse (jente)") ? 2 :
                            categoryAndGender.Contains("7.klasse (jente)") ? 4 :
                            categoryAndGender.Contains("5.klasse (gutt)") ? 1 :
                            categoryAndGender.Contains("6.klasse (gutt)") ? 3 :
                            categoryAndGender.Contains("7.klasse (gutt)") ? 5 : 0; // fallback

                // Get the correct "Velg skole" column based on index
                return csvRow.GetField("Ønsker å delta i (maks to øvingar)", index) ?? string.Empty;
            });

        }
    }
}
