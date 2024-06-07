using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMImporter
{
    using System;

    public class StringSimilarityChecker
    {
        // Method to calculate the Levenshtein distance between two strings
        public static int CalculateLevenshteinDistance(string str1, string str2)
        {
            int[,] distance = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= str2.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(Math.Min(
                        distance[i - 1, j] + 1,
                        distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[str1.Length, str2.Length];
        }

        // Method to check for similarity between two strings
        public static bool AreSimilar(string str1, string str2, int threshold)
        {
            int distance = CalculateLevenshteinDistance(str1, str2);
            return distance <= threshold;
        }
    }

    //class Program
    //{
    //    static void Main()
    //    {
    //        string str1 = "kitten";
    //        string str2 = "sitting";

    //        int threshold = 3; // Set a threshold for similarity

    //        bool similar = StringSimilarityChecker.AreSimilar(str1, str2, threshold);

    //        Console.WriteLine($"Are '{str1}' and '{str2}' similar? {similar}");
    //    }
    //}

}
