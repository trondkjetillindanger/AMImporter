namespace AMImporter
{
    public class Participation
    {
        public int Id { get; set; }
        public string Event { get; set; }
        public string EventCategory { get; set; }
        public DateTime EventDate { get; set; }
        public List<iSonenParticipation> Participants { get; set; }
        public string FieldType { get; set; }
        public string EventAbbreviation { get; set; }
        public int? Distance { get; set; }
        public double EstimatedDuration { get; set; }
        public string EventCategoryAbbreviation { get; set; }
    }

}
