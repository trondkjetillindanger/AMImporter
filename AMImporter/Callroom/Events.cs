using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class EventList
    {
        public List<AMImporter.Callroom.@Event> events { get; set;}
        public void CreateCallroomEvents(TimeSchedule timeSchedule, List<iSonenParticipation> iSonenParticipations, EventCategory eventCategory)
        {
            events = timeSchedule.AMEvents.Values.Select(x =>
            {
                var eventParticipations = iSonenParticipations.Where(y => y.Event == x.SAEventCategoryName && x.AgeCategory.Select(z => eventCategory.amCategory.GetAMName(z).ToLower()).Contains(y.EventCategory.ToLower())).ToList();
                var date = eventParticipations.Any() ? eventParticipations.Select(y => y.EventDate).First() : null;
                if (date == null)
                {
                    return new AMImporter.Callroom.@Event()
                    {
                        @event = x.Name,
                        event_id = x.Id.Value,
                        event_time = "null"
                    };
                };

                DateTime dateTime = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture).Add(TimeSpan.ParseExact(x.Time, "hh\\:mm\\:ss", CultureInfo.InvariantCulture));
                //string dateTimeString = $"{date}T{x.Time}";mm:
                //DateTime dateTime = DateTime.Parse(dateTimeString);
                dateTime.AddHours(2);
                return new AMImporter.Callroom.@Event()
                {
                    @event = x.Name,
                    agegroup = String.Join(", ", x.AgeCategory),
                    callroomregistration_closed_time = dateTime.AddHours(-1).AddMinutes(5).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    callroomregistration_open_time = dateTime.AddDays(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    callroomregistration_time = dateTime.AddHours(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    competition_id = 1,
                    enterStadium_time = "null",
                    event_id = x.Id.Value,
                    event_time = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                };
            }).ToList<AMImporter.Callroom.@Event>();
        }
    }
}
