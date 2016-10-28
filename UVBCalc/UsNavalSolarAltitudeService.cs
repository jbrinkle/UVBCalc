using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UVBCalc
{
    /// <summary>
    /// Uses the U.S. Navy's Solar Altitude/Azimuth calculator at http://aa.usno.navy.mil/data/docs/AltAz.php
    /// Computes a year's worth of daily entries indicating when solar altitude is > 50 degrees from the horizon
    /// Which is the lowest the sun can sit in the sky without the Earth's atmosphere filtering out UVB
    /// http://articles.mercola.com/sites/articles/archive/2012/03/26/maximizing-vitamin-d-exposure.aspx#!
    /// </summary>
    internal class UsNavalSolarAltitudeService
    {
        private const int Interval = 20;
        private const string UriPattern = @"http://aa.usno.navy.mil/cgi-bin/aa_altazw.pl?form=1&body=10&year={0}&month={1}&day={2}&intv_mag={3}&state={4}&place={5}";
        private readonly IWebRequestHelper _requestHelper;
        private readonly TraceWriter _tracer;
        private readonly Regex _lineParseRegex;
        private DateTime _lastRequestStarted;

        private readonly bool _testMode;

        private int SingleEntryRetryDelay => _testMode ? 0 : (int) (Throttle * 1000);

        /// <summary>
        /// Minimum number of seconds that must exist between initiating requests 
        /// </summary>
        public double Throttle { get; set; }

        public UsNavalSolarAltitudeService(TraceWriter tracer, IWebRequestHelper requestHelper, bool testMode = false)
        {
            Debug.Assert(requestHelper != null);

            _testMode = testMode;

            _tracer = tracer;
            _requestHelper = requestHelper;

            _lineParseRegex = new Regex(@"^\s*(?<time>\d\d\:\d\d)\s+(?<alt>\-?\d+(\.\d+)?)",
                                        RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            Throttle = testMode ? 0 : 1;
        }

        /// <summary>
        /// Get an entire year of solar altitude data entries
        /// </summary>
        /// <param name="year">The year of entries desired</param>
        /// <param name="city">City for calculation</param>
        /// <param name="state">State for calculation</param>
        /// <returns></returns>
        public List<UsNavalSolarAltitudeGreaterThan50DegDailyEntry> GetOneYear(int year, string city, string state)
        {
            var informationFormat = "Processing {0:MMMM}";

            var dayOfYear = new DateTime(year, 1, 1);
            var entries = new List<UsNavalSolarAltitudeGreaterThan50DegDailyEntry>();

            _tracer.WriteInformation(string.Format(informationFormat, dayOfYear));
            while (dayOfYear.Year == year)
            {
                if (!_testMode && (DateTime.Now - _lastRequestStarted).TotalSeconds < Throttle)
                {
                    Thread.Sleep((int) (_lastRequestStarted.AddSeconds(Throttle) - DateTime.Now).TotalMilliseconds);
                }

                _lastRequestStarted = DateTime.Now;

                var entry = GetOneDay(dayOfYear, city, state);

                if (entry != null)
                {
                    entries.Add(entry);
                }

                var nextDay = dayOfYear.AddDays(1);

                if (nextDay.Month != dayOfYear.Month)
                {
                    _tracer.WriteInformation(string.Format(informationFormat, nextDay));
                }

                dayOfYear = nextDay;
            }

            return entries;
        }

        /// <summary>
        /// Get a single day of solar altitude data entries
        /// </summary>
        /// <param name="date">The date for which data is desired</param>
        /// <param name="city">City for calculation</param>
        /// <param name="state">State for calculation</param>
        /// <returns></returns>
        public UsNavalSolarAltitudeGreaterThan50DegDailyEntry GetOneDay(DateTime date, string city, string state)
        {
            var requestUri = string.Format(UriPattern, date.Year, date.Month, date.Day, Interval, state, city);

            var retriesLeft = 3;
            string body = null;

            // try a few times (if necessary) to get a response body
            while (retriesLeft > 0)
            {
                body = _requestHelper.GetResponseBodyForUri(new Uri(requestUri));

                if (body != null)
                {
                    break;
                }

                retriesLeft--;
                Thread.Sleep(SingleEntryRetryDelay);
            }

            // If no body was ever found, time to move on
            if (body == null)
            {
                return null;
            }

            var entry = new UsNavalSolarAltitudeGreaterThan50DegDailyEntry
            {
                EntryDate = date
            };

            // dig through the result for data lines
            foreach (Match match in _lineParseRegex.Matches(body))
            {
                var timeOffset = ParseTimeOffset(match.Groups["time"].Value);
                var altitude = ParseAltitude(match.Groups["alt"].Value);

                // for when no begin time set
                if (altitude >= 50 && entry.BeginOffset == TimeSpan.Zero)
                {
                    entry.BeginOffset = timeOffset;
                    continue;
                }

                // for when no end time set
                if (altitude < 50 && entry.BeginOffset > TimeSpan.Zero && entry.EndOffset == TimeSpan.Zero)
                {
                    entry.EndOffset = timeOffset;
                }
            }

            return entry;
        }

        private TimeSpan ParseTimeOffset(string offset)
        {
            var parts = offset.Split(':');
            var hours = double.Parse(parts[0]);
            var minutes = double.Parse(parts[1])/60;

            return TimeSpan.FromHours(hours + minutes);
        }

        private double ParseAltitude(string alt)
        {
            return double.Parse(alt);
        }
    }
}
