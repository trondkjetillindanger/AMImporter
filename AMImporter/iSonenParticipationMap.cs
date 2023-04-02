using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class iSonenParticipationMap : ClassMap<iSonenParticipation>
    {
        public iSonenParticipationMap()
        {
            Map(p => p.FirstName).Name("Fornavn");
            Map(p => p.LastName).Name("Etternavn");
            Map(p => p.BirthDate).Name("Fødselsdato");
            Map(p => p.Gender).Name("Kjønn");
            Map(p => p.Team).Name("Klubb");
            Map(p => p.Event).Name("Øvelse");
            Map(p => p.EventCategory).Name("Klasse");
            Map(p => p.EventDate).Name("Dato");
            Map(p => p.EmailRegistrant).Name("E-post påmelder");
            Map(p => p.Email).Name("E-post");
            Map(p => p.Id).Name("Person ID");
        }
    }
}
