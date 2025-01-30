using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    using System;

    public class AthleticEventTimeCalculator
    {
        public static double EstimateEventTime(string eventAbbreviation, string eventType, int numParticipants, double runningTime = 0)
        {
            double estimatedTime = 0;

            if (string.IsNullOrEmpty(eventType?.Trim()))
            {
                return numParticipants / 8.0 * 5; // sprint

                // Example: 1500m race → 5 min per heat + estimated running time
                // estimatedTime = 5 + runningTime;
            }

            if (eventType == "H")
            {
                // Example: 12 participants → 3 rounds for all (12x3=36 attempts), 3 final rounds (8x3=24 attempts)
                int totalAttempts = (numParticipants * 3) + (Math.Min(numParticipants, 8) * 3);
                return totalAttempts + 3; // 1 min per attempt + 3 min for reorder
            }

            if (eventType == "V")
            {
                if (eventAbbreviation == "HJ")
                {
                    // Example: 12 participants → 3 rounds for all (12x3=36 attempts), 3 final rounds (8x3=24 attempts)
                    int totalAttempts = (numParticipants * 3) + (Math.Min(numParticipants, 8) * 3);
                    return totalAttempts + 3; // 1 min per attempt + 3 min for reorder
                }

                if (eventAbbreviation == "PV")
                {
                    // Example: 12 participants → 8 attempts each (12x8=96 attempts)
                    estimatedTime = (numParticipants * 8 * 1.5) + 10; // 1.5 min per attempt + 10 min for winner
                }
            }
            return 0;
            //throw new ArgumentException($"Unknown event type/ abbreviation. {eventAbbreviation} - {eventType}");
        }

        //public static void Main()
        //{
        //    Console.WriteLine("Estimated time for Long Jump (12 participants): " + EstimateEventTime("horizontal jump", 12) + " min");
        //    Console.WriteLine("Estimated time for High Jump (12 participants): " + EstimateEventTime("high jump", 12) + " min");
        //    Console.WriteLine("Estimated time for Pole Vault (12 participants): " + EstimateEventTime("pole vault", 12) + " min");
        //    Console.WriteLine("Estimated time for 100m Sprint (24 participants): " + EstimateEventTime("sprint", 24) + " min");
        //    Console.WriteLine("Estimated time for 1500m Race (15 participants, 5 min run time): " + EstimateEventTime("middle distance", 15, 5) + " min");
        //}
    }

}
