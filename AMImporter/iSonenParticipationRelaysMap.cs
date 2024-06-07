using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class iSonenParticipationRelaysMap : ClassMap<iSonenParticipation>
    {
        public iSonenParticipationRelaysMap()
        {
            Map(p => p.FirstName).Name("Fornavn");
            Map(p => p.LastName).Name("Etternavn");
            Map(p => p.BirthDate).Name("Fødselsdato");
            Map(p => p.Gender).Name("Kjønn");
            Map(p => p.Team).Name("Lag");
            Map(p => p.TeamId).Name("Lag");
            Map(p => p.GroupId).Name("Lag");
            Map(p => p.Event).Name("Øvelse");
            Map(p => p.EventCategory).Name("Klasse");
            Map(p => p.EmailRegistrant).Name("Påmelder epost");
            Map(p => p.Email).Name("E-post");
            Map(p => p.Id).Name("Person ID");
        }
    }
}
