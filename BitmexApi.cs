using System.Net;
using System;
using System.Collections.Generic;

namespace Bitmex {
    public class BitmexApi {
        
        private string apiKey;
        private string apiSecret;

        private Query query;
        private TimeUtils time;
        private decimal deltaOfStartEndMins;
 
        private string symbol, timeframe;
        
        private int timeframeInMins;
        
        DateTime startTime, endTime;

        private Bucket bucket = new Bucket();
        
        private string lastTimestamp;

        const int BATCH_SIZE = 1000;

        public BitmexApi(string domain, string symbol, 
            string startTime, string endTime, string timeframe,
            string apiKey, string apiSecret){
            this.symbol = symbol;
            this.timeframe = timeframe;
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;

            query = new Query(domain, this.apiKey, this.apiSecret);
            
            // Convert string time into DateTime
            time = new TimeUtils(timeframe);
            this.startTime = time.USDateParse(startTime);
            this.endTime = time.USDateParse(endTime);
            
            timeframeInMins = time.GetTimeframeInMins();

            // Difference between start and end times
            deltaOfStartEndMins = time.TimeDeltaInMins(this.startTime, this.endTime);
        }
        
        public Bucket GetBucket() {
            // Set all parameters for trade data query
            var param = new Dictionary<string, string>();
            param["binSize"] = timeframe;
            param["partial"] = false.ToString();
            param["symbol"] = symbol;
            param["columns"] = "open,high,low,close,volume";
            param["count"] = BATCH_SIZE.ToString();
            param["start"] = 0.ToString();
            param["reverse"] = "false";
            param["startTime"] = WebUtility.UrlEncode(time.TimeInBMexFormat(startTime));
            param["endTime"] = WebUtility.UrlEncode(time.TimeInBMexFormat(endTime));
            
            // Get query result as JSON string
            string json = query.ExecuteQuery("trade/bucketed", param);
            
            // Convert returned JSON into a Bucket object
            bucket.AddFromJson(json);
            // Store timestamp of last item in bucket
            lastTimestamp = bucket.Last.Timestamp;

            // To get the next available bucket, increment the timestamp of the last item in the 
            // current bucket by that of the selected binsize time frame e.g 1d, 1m etc
            startTime = time.GetNextStartTime(lastTimestamp);
            return bucket;
        }

        public bool HasMoreBuckets() {
            DateTime lastBucketDate = DateTime.Parse(lastTimestamp);
            int result = DateTime.Compare(lastBucketDate, endTime);
            return (result < 0);
        }   
    }
}