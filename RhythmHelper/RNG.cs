using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GooBot
{
    public static class RNG
    {
        private readonly static RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        public static int Roll(int sides)
        {
            if (sides <= 0) return -1;

            int[] results = new int[sides];

            byte roll = RollDice((byte)results.Length);

            _rng.Dispose();

            return roll;
        }
        private static byte RollDice(byte numberSides)
        {
            byte[] randomNumber = new byte[1];
            do
            {
                _rng.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberSides));
            return (byte)((randomNumber[0] % numberSides) + 1);
        }
        private static bool IsFairRoll(byte roll, byte numSides)
        {
            int fullSetsOfValues = Byte.MaxValue / numSides;
            return roll < numSides * fullSetsOfValues;
        }
        public static string Distribution(int sides, int iterations)
        {
            int totalRolls = iterations;
            int[] results = new int[sides];
            double[] pc = new double[sides];

            var sb = new StringBuilder($"For {iterations} rolls, of a {sides}-sided die:");

            for (int x = 0; x < totalRolls; x++)
            {
                byte roll = RollDice((byte)results.Length);
                results[roll - 1]++;
            }
            for (int i = 0; i < results.Length; ++i)
            {
                var value = Math.Round((double)results[i] / (double)(iterations / 100), 3);
                pc[0] = value;
                sb.Append($"\n{i + 1}: {value}%");

            }
            _rng.Dispose();

            return sb.ToString();
        }
        //private static double CalculateStandardDeviation(IEnumerable<double> values)
        //{
        //    double standardDeviation = 0;

        //    if (values.Any())
        //    { 
        //        double avg = values.Average();
     
        //        double sum = values.Sum(d => Math.Pow(d - avg, 2));

        //        standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
        //    }

        //    return standardDeviation;
        //}
    }
}