using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSharpVersion
{
    class Program
    {
        struct Record
        {
            public int Label;
            public int[] Pixels;
        }

        struct TestRecord
        {
            public int ExpectedLabel;
            public int FoundLabel;
            public double CorrectPercent;
        }

        static void Main()
        {
            var timer = Stopwatch.StartNew();

            var knownRecords = ParseRecords("../../../trainingsample.csv");

            var distanceBetweenArrays = new Func<int[], int[], IEnumerable<int>>((a1, a2) => a1.Zip(a2, (i1, i2) => (i1 - i2)*(i1 - i2)));

            var aggDistanceBetweenArrays = new Func<int[], int[], int>((a1, a2) => distanceBetweenArrays(a1, a2).Sum());

            var classify = new Func<Record, Record>
                (unknown => knownRecords.OrderBy(
                    known => aggDistanceBetweenArrays(unknown.Pixels, known.Pixels)).First());


            //// Now on to validating our classifier..

            //                                  // from http://brandewinder.blob.core.windows.net/public/validationsample.csv
            var unknownRecords = ParseRecords("../../../validationsample.csv");

            var testedRecords = unknownRecords.Select(r =>
                {
                    var foundLabel = classify(r).Label;
                    return new TestRecord
                        {
                            FoundLabel = foundLabel,
                            ExpectedLabel = r.Label,
                            CorrectPercent = foundLabel.Equals(r.Label) ? 1.0 : 0.0
                        };
                }).ToList();

            foreach (TestRecord r in testedRecords)
            {
                Console.WriteLine("Expected: {0}   Found: {1}    Match: {2}",
                                    r.ExpectedLabel,
                                    r.FoundLabel,
                                    r.ExpectedLabel.Equals(r.FoundLabel));
            }

            var percentCorrect = testedRecords.Average(r => r.CorrectPercent);

            Console.WriteLine("Percent Correct: {0} in {1} seconds", percentCorrect, timer.Elapsed.TotalSeconds);
        }

        private static IEnumerable<Record> ParseRecords(string path)
        {
            return File.ReadAllLines(path)
                        .Select(s => s.Split(','))
                        .Skip(1)
                        .Select(sa => sa.Select(s => Convert.ToInt32(s)))
                        .Select(intArray => new Record
                            {
                                Label = intArray.First(),
                                Pixels = intArray.Skip(1).ToArray()
                            }).ToList();
        }

    }
}
