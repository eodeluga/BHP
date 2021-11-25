using System.Net;
using System.IO;
using System.Text;

namespace Bitmex {
    class InfluxDB {

        private StringBuilder sb = new StringBuilder();
        private string dbName;
        private string filePath;
        private FileStream fs = null;
        private string endpoint;

        internal InfluxDB (string endpoint, string dbName, string filePath = "") {
            this.endpoint = endpoint;
            this.dbName = dbName;
            this.filePath = filePath;
            // Check whether write to disk was specified and setup if so
            SetupWriteToDisk();
        }
        
        public bool DatabaseExists(){
            string operation = $"query?db=";
            string url = $"{endpoint}/{operation}{dbName}&q=SHOW+DATABASES";
            var response = MakeHttpRequest(url);
            // Get the HTTP response string read from a stream
            return response.Contains(dbName);        
        }
        
        public string GetLastRecord(){
            /// <summary>
            /// Queries the database and returns the last measurement in influxDB.
            /// </summary>
            string operation = $"query?db=";
            string url = $"{endpoint}/{operation}{dbName}&q=SELECT+LAST(close)+FROM+bucket";
            return MakeHttpRequest(url);
        }

        public void InsertFromBucket(Bucket bucket) { 
            // Iterate over the bucket to create the InfluxDB line protocol batch string
            foreach (Bucket item in bucket.List){
                sb.Append("bucket").Append(",")
                    .Append($"symbol={item.Symbol} ")
                    .Append($"open={item.Open}").Append(",")
                    .Append($"high={item.High}").Append(",")
                    .Append($"low={item.Low}").Append(",")
                    .Append($"close={item.Close}").Append(",")
                    .Append($"volume={item.Volume} ")
                    .Append($"{TimeUtils.UnixTimeToTickSeconds(item.Timestamp)}")
                    // Append new line for next line protocol string
                    .AppendLine();
            }
            
            // Insert the line protocol into the database as a string
            InsertRecord(sb.ToString());
            // Also write output to disk
            WriteRecordToDisk(sb.ToString());
            // Wipe string builder clear ready for next run
            sb.Clear();
        }

        private void InsertRecord(string record){
            string operation = $"write?db=";
            string url = $"{endpoint}/{operation}{dbName}&precision=s";
            MakeHttpRequest(url, record);
        }

        private async void WriteRecordToDisk(string record){
            if (fs != null) {
                byte[] buffer = Encoding.UTF8.GetBytes(record);
                await fs.WriteAsync(buffer);
            }
        }

        private void SetupWriteToDisk(){
            // Check whether writing to disk was specified
            if (!string.IsNullOrEmpty(filePath)){
                // Initialise buffer and the file stream for writing
                try {
                    fs = new FileStream(filePath, FileMode.Append, FileAccess.Write,
                        FileShare.None, 4096, FileOptions.Asynchronous);
                } catch (IOException) {
                    // Cannot open file for writing
                }
            }
        }

        public string MakeHttpRequest(string url){
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            // Get the HTTP response string read from a stream
            using (var stream = new StreamReader(request.GetResponse().GetResponseStream()))
                // Return JSON response string from server
                return stream.ReadToEnd();
        }
        
        private void MakeHttpRequest(string url, string data) {
                       
            WebRequest request = WebRequest.Create(url);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            // Stream the POST data 
            var stream = request.GetRequestStream();
            stream.Write(bytes);
            var response = request.GetResponse();
        }
    }
}