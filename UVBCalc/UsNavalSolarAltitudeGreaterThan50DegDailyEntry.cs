using System;

namespace UVBCalc
{
    static class Ext
    {
        public static int Round(this double original)
        {
            return (original % 1 >= .5)
                ? (int)original + 1
                : (int)original;
        }
    }

    internal class UsNavalSolarAltitudeGreaterThan50DegDailyEntry
    {
        /// <summary>
        /// Offset of when sun begins to be 50* above horizon
        /// </summary>
        public TimeSpan BeginOffset { get; set; }

        /// <summary>
        /// Offset of when sun no longer 50* above horizon
        /// </summary>
        public TimeSpan EndOffset { get; set; }

        /// <summary>
        /// Date for this entry
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Approx number of minutes the sun is 50*+ above horizon 
        /// </summary>
        public int Duration => (EndOffset - BeginOffset).TotalMinutes.Round();

        public override string ToString()
        {
            return $"{EntryDate} - {Duration} min";
        }

        public string ToCommaDelimitedString()
        {
            var date = EntryDate.ToShortDateString();
            var startTime = (EntryDate + BeginOffset).ToShortTimeString();
            var endTime = (EntryDate + EndOffset).ToShortTimeString();

            return $"{date},{startTime},{endTime},{Duration}";
        }
    }
}
