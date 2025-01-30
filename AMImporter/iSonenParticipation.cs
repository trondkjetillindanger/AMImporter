using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class iSonenParticipation
    {
        private string _eventCategory = null;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string Team { get; set; }
        public string TeamId { get; set; }
        public string License { get; set; }
        public string GroupId { get; set; }
        public string Event { get; set; }

        //public int EstimatedEventTime => AthleticEventTimeCalculator.EstimateEventTime()
        public string EventCategory { 
            get
            {
                if (_eventCategory == "Jenter 18-19" || _eventCategory == "Gutter 18-19")
                {
                    _eventCategory = _eventCategory.Replace('-', '/');
                }
                return _eventCategory;
            } 
            set 
            { 
                _eventCategory = value;  
            } 
        }

        public string EventDate { get; set; }
        public string EmailRegistrant { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Bib { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            iSonenParticipation otherPerson = (iSonenParticipation)obj;

            return FirstName == otherPerson.FirstName &&
                   LastName == otherPerson.LastName &&
                   BirthDate == otherPerson.BirthDate &&
                   Gender == otherPerson.Gender;
        }
    }
}
