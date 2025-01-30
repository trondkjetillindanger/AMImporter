using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class EventType
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Venue { get; set; }
        public int Distance { get; set; }
        public string WindMode { get; set; }
        public string WindTime { get; set; }
        public int Precision { get; set; }
        public string FieldType { get; set; }
        public int HandTimeDifference { get; set; }
        public int AthletesQuantity { get; set; }
        public int Implement { get; set; }
        public int CombinedEventsQuantity { get; set; }
        public string CombinedEvents { get; set; }
        public string StandardName { get; set; }
        public string StandardAbbreviation { get; set; }
        public int LowToHigh { get; set; }
        public string NationalCode { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
    }
}
