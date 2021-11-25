using System.Numerics;
using System;
using System.Globalization;
namespace Bitmex{

    internal class TimeUtils{

        private string timeframe;
        
        private CultureInfo provider = new CultureInfo("en-US");
        private static readonly DateTime epochStart = DateTime.UnixEpoch.ToUniversalTime();

        internal TimeUtils(string timeframe) {
            this.timeframe = timeframe;
        }

        internal DateTime GetNextStartTime(string previousTimestamp) {
            string dateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            DateTime newStartTime = DateTime.ParseExact(previousTimestamp, dateFormat, provider)
                .AddMinutes(GetTimeframeInMins());
            return newStartTime;
        }

        internal decimal TimeDeltaInMins(DateTime startTime, DateTime endTime)
            => (decimal)endTime.Subtract(startTime).TotalMinutes;

        internal DateTime USDateParse(string date) {
            string[] dateFormat = {"MM/dd/yyyy", "MM/dd/yyyy HH:mm"};
            return DateTime.ParseExact(
                date,
                dateFormat,
                provider);
        }

        internal string TimeInBMexFormat (DateTime dateTime)
            // Ha, I like these functional methods
            => dateTime.ToString("MM/dd/yyyy HH:mm:ss");


        internal int GetTimeframeInMins() {
            switch (timeframe) {
                // Select the timeframe seconds
                case "1m": 
                    return (int)BINSIZES.MIN;
                case "5m": 
                    return (int)BINSIZES.MIN5;
                case "1h": 
                    return (int)BINSIZES.HR;
                case "1d": 
                    return (int)BINSIZES.DAY;
                default:
                    return (int)BINSIZES.DAY;
            }
        }

        internal static string UnixTimeToTickSeconds(string time) {
            /// <summary>
            /// Converts a string representation of a Unix timestamp 
            /// to seconds since Unix epoch start time.
            /// </summary>
            DateTimeOffset timeOffset = DateTimeOffset.Parse(time).UtcDateTime;
            TimeSpan timeSpan = timeOffset.Subtract(epochStart);
            return ((BigInteger) timeSpan.TotalSeconds).ToString();
        }


        internal enum BINSIZES {
            MIN = 1, MIN5 = 5, HR = 60, DAY = 1440
        }
    }
}