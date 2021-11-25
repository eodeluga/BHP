using System.Threading;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Net.Http;
using System.Collections.Generic;

namespace Bitmex {

    internal class Query {
        private string apiKey, apiSecret;
        private string domain;
        private const string BASE_URL = "api/v1";
        private Utils utils = new Utils();
        private static int apiRateLimit, apiRateRemaining = 0;
        
        internal Query (string domain, string apiKey, string apiSecret) {
            this.domain = domain;
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        private string HttpResponse { get; set; }

        internal string BuildQueryData(Dictionary<string, string> param){
            if (param == null)
                return string.Empty;
            
            StringBuilder b = new StringBuilder();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, item.Value));

            try { return b.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }

        internal string ExecuteQuery(string operation, Dictionary<string, string> query){
            HttpWebResponse response;
            string queryData = BuildQueryData(query);
            string url = $"{domain}/{BASE_URL}/{operation}?{queryData}";
         
            response = MakeHttpRequest(url, queryData, operation);

            // Read the response into a string using a stream
            using (Stream responseStream = response.GetResponseStream()){
                using (StreamReader myStreamReader 
                    = new StreamReader(responseStream, Encoding.UTF8)){
                        // Return stream content as string object
                        return myStreamReader.ReadToEnd();
                }
            }
        }

        private void SetRateLimitStats(HttpWebResponse response) {
            // Store current rate limit stats taken from response headers
            apiRateLimit = 
                Int32.Parse(response.GetResponseHeader("X-RateLimit-Limit"));
            apiRateRemaining = 
                Int32.Parse(response.GetResponseHeader("X-RateLimit-Remaining"));
        }
        
        private HttpWebResponse MakeHttpRequest(string url, string queryData, string operation){
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = HttpMethod.Get.ToString();
            
            // Calculate request signature for authentication headers
            // if an API key and secret are supplied 
            if (!(string.IsNullOrEmpty(apiKey) && string.IsNullOrEmpty(apiSecret))) {
                // Create Http headers
                string expires = utils.GetExpires().ToString();
                string message = 
                    $"{HttpMethod.Get.ToString()}/{BASE_URL}/{operation}?{queryData}{expires}";
                byte[] signatureBytes = 
                    utils.HmacSha256(Encoding.UTF8.GetBytes(apiSecret), Encoding.UTF8.GetBytes(message));
                string signatureString = Utils.ByteArrayToString(signatureBytes);
                // Add HTTP headers
                request.Headers.Add("api-expires", expires);
                request.Headers.Add("api-key", apiKey);
                request.Headers.Add("api-signature", signatureString);
            }

            // Get the http request response
            var response = (HttpWebResponse)request.GetResponse();
            
            // Perform request rate limiting if necessary
            SetRateLimitStats(response);
            MitigateRateLimiting();
            return response;
        }

        /// <summary>
        /// Returns the current rate limit of the remote server API in requests per minute
        /// </summary>
        public static int RateLimit { get => apiRateLimit; }
        private static int RateRemaining { get => apiRateRemaining; }

        private void MitigateRateLimiting() {
            // Given that the remote server imposes a rate limit of 
            // 60 RPM for authenticated requests and 30 for non, this method
            // delays the next request if remaining allowed requests begins
            // reducing i.e rate limiting in effect
            if (RateRemaining < 30 | RateRemaining < 60)
                Thread.Sleep(1000);
        }
    }
}