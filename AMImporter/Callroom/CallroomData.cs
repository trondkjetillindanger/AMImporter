using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AMImporter.Timeschedule;

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
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            var json = JsonSerializer.Serialize(this, jso);

            File.WriteAllText(path + "\\create\\callroom.json", json, Encoding.UTF8);
        }
    }
}
