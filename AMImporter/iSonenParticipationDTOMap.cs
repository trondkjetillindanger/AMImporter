using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace AMImporter
{
    public class iSonenParticipationDTOMap : ClassMap<iSonenParticipationDTO>
    {
        public iSonenParticipationDTOMap()
        {
            Map(p => p.FirstName).Name("Fornavn");
            Map(p => p.LastName).Name("Etternavn");
            Map(p => p.BirthDate).Name("Fødselsdato");
            Map(p => p.Gender).Name("Kjønn");
            Map(p => p.Team).Name("Klubb");
            Map(p => p.Event).Name("Øvelse");
            Map(p => p.EventCategory).Name("Klasse");

        }
    }
}
