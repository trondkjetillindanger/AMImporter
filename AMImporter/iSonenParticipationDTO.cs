using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class iSonenParticipationDTO
    {
        private string _eventCategory = null;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string Team { get; set; }
        public string Event { get; set; }
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
    }
}
