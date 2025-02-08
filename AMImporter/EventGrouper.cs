using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    public class EventGrouper
    {
        public static List<Participation> MergeEvents(List<Participation> groupedParticipations)
        {
            var mergedEvents = new List<Participation>();
            var currentMergedEvent = new List<Participation>();  // To collect participants of the merged events

            // Order events by Event and EventCategory
            groupedParticipations.OrderBy(x => x.Event)
                .ThenBy(x => x.EventCategory)
                .ToList()
                .ForEach(y =>
                {
            // If currentMergedEvent is empty, add the first event
                    if (!currentMergedEvent.Any())
                    {
                        currentMergedEvent.Add(y);
                    }
                    else
                    {
                // Get the last merged event to compare
                        var lastMergedEvent = currentMergedEvent.Last();

                // Check if the current event can be merged with the last merged event
                        if ((lastMergedEvent.FieldType == y.FieldType) && (lastMergedEvent.Distance == y.Distance) && (lastMergedEvent.EventAbbreviation == y.EventAbbreviation) && (ShouldMerge(lastMergedEvent, y)))
                        {
                    // Merge: update the event name and add participants
                            lastMergedEvent.EventCategoryAbbreviation += "/" + y.EventCategoryAbbreviation;
                            lastMergedEvent.EventCategory += "/" + y.EventCategory;
                            lastMergedEvent.Participants.AddRange(y.Participants);
                            lastMergedEvent.EstimatedDuration = AthleticEventTimeCalculator.EstimateEventTime(y.EventAbbreviation, y.FieldType, lastMergedEvent.Participants.Count(), y.Distance);
                        }
                        else
                        {
                    // If they can't be merged, save the current merged event and start a new merge group
                            mergedEvents.Add(lastMergedEvent);
                            currentMergedEvent = new List<Participation> { y }; // Start new group
                        }
                    }
                });

            // Add the last merged event if any
            if (currentMergedEvent.Any())
            {
                mergedEvents.Add(currentMergedEvent.Last());
            }

            return mergedEvents;
        }



        public static bool ShouldMerge(Participation lastMergedEvent, Participation currentEvent)
        {
            if ((currentEvent.Participants.Count()<5) && (string.IsNullOrEmpty(currentEvent.FieldType)) && (currentEvent.Distance < 200))
            {
                return true;
            }
            else if ((currentEvent.Participants.Count() <= 2) && (string.IsNullOrEmpty(currentEvent.FieldType)) && (currentEvent.Distance == 200))
            {
                return true;
            }
            else if ((currentEvent.Participants.Count() <= 4) && (string.IsNullOrEmpty(currentEvent.FieldType)) && (currentEvent.Distance == 400))
            {
                return true;
            }
            else if ((currentEvent.Participants.Count() <= 6) && (string.IsNullOrEmpty(currentEvent.FieldType)) && (currentEvent.Distance > 400) && (currentEvent.Distance <= 5000))
            {
                return true;
            }
            else if ((currentEvent.Participants.Count() < 5) && (currentEvent.FieldType == "V")) {
                return true;
            }
            else if ((currentEvent.Participants.Count() < 8) && (currentEvent.FieldType == "H"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
