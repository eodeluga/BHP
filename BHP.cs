using System.Globalization;
using System.Text.RegularExpressions;
using System;

namespace Bitmex {
    internal class BHP {
        public static string[] Args { get; set; }

        private static string domain = "https://www.bitmex.com", 
            symbol = "xbt", timeFrame = "1m",
            // Dates are in US format
            start = "01/01/2017", 
            end = DateTime.Now.ToString("MM/dd/yyyy HH:mm"),
            bitmexKey = "", // Your Bitmex key
            bitmexSecret = ""; // Your Bitmex secret

        public static void Main(string[] args){
                     
            InfluxDB influx = new InfluxDB (
                // Database server endpoint address
                "",  // Your InfluxDB server endpoint
                // Database name
                "bitmex",
                // Save trade data to disk
                @"" // Your local storage to save trade data as CSV
            );
            
            // If database exists set start time to next after last bucket
            if (influx.DatabaseExists()) {
                start = GetNewStartTime();                
            }
            
            BitmexApi bitmex = new BitmexApi(
                domain, symbol, start, end, timeFrame,
                bitmexKey, bitmexSecret);
          
            // Insert bucket into database
            do {
                influx.InsertFromBucket(bitmex.GetBucket());
            } while (bitmex.HasMoreBuckets());
            // Finished so send stop
            Environment.Exit(0);
            
            # region local methods
            string GetNewStartTime(){
                string json = influx.GetLastRecord();
                // Use regex to get timestamp from string
                string match = @"[\d]{4}-[\d]{2}-[\d]{2}T[\d]{2}:[\d]{2}:[\d]{2}Z";
                string timestamp = Regex.Match(json, match).Value;
                // Add next time unit
                string dateFormat = "yyyy-MM-ddTHH:mm:ssZ";
                CultureInfo culture = new CultureInfo("en-US");
                DateTime dateTime = DateTime.ParseExact(
                    timestamp, dateFormat, culture).ToUniversalTime();
                // Add the respective time frame minutes to timestamp to get new
                // new start time for pulling bucket data from
                TimeUtils time = new TimeUtils(timeFrame);
                DateTime newStart = dateTime.AddMinutes(time.GetTimeframeInMins());
                return newStart.ToString("MM/dd/yyyy HH:mm");
            }
            # endregion local methods
        }   
    }
}
