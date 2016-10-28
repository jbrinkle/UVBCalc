using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UVBCalc
{
    class Program
    {
        static void Main(string[] args)
        {
            var city = "Mesa";
            var state = "AZ";
            var year = 2016;

            var outStream = Console.OpenStandardOutput();
            var tracer = new TraceWriter(outStream) {Level = TraceWriter.TraceLevel.Information};
            var requestHelper = new WebRequestHelper(tracer);

            var service = new UsNavalSolarAltitudeService(tracer, requestHelper)
            {
                Throttle = .5
            };

            //var entries = new [] { service.GetOneDay(DateTime.Today, city, state) };
            var entries = service.GetOneYear(year, city, state);

            var fileName = $"{state}_{city}_{year}.csv";
            tracer.WriteInformation($"Writing output to {fileName}...");
            WriteOutputFile(fileName, entries);
        }

        private static void WriteOutputFile(string fileName, IEnumerable<UsNavalSolarAltitudeGreaterThan50DegDailyEntry> entries)
        {
            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = new StreamWriter(fs)
                {
                    AutoFlush = true
                };

                var headerLine = $"Date,StartTime,EndTime,Duration";
                writer.WriteLine(headerLine);

                foreach (var entry in entries)
                {
                    writer.WriteLine(entry.ToCommaDelimitedString());
                }
            }
        }
    }
}
