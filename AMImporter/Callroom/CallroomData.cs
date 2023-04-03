using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMImporter.Callroom
{
    public class CallroomData
    {
        public List<AMImporter.Callroom.@Event> events  { get; set; }
        public List<AMImporter.Callroom.Participant> participants { get; set; }

        public void Create(string path, TimeSchedule timeSchedule, List<iSonenParticipation> iSonenParticipations, EventCategory eventCategory)
        {
            AMImporter.Callroom.EventList eventList = new AMImporter.Callroom.EventList();
            eventList.CreateCallroomEvents(timeSchedule, iSonenParticipations, eventCategory);

            AMImporter.Callroom.ParticipantList participantList = new AMImporter.Callroom.ParticipantList();

            participantList.CreateCallroomParticipants(timeSchedule, iSonenParticipations, eventCategory);
            events = eventList.events;
            participants = participantList.participants;
            string json = JsonSerializer.Serialize(this);

            File.WriteAllText(path + "\\create\\callroom.json", json);
        }
    }
}
