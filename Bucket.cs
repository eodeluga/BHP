using System.Linq;
using System.Text;
using System.Collections.Generic;
namespace Bitmex {
using Newtonsoft.Json;

    public class Bucket {
        
        private List<Bucket> _bucketList;

        private Bucket _last;
                
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        
        [JsonProperty(PropertyName = "open")]
        public string Open { get; set; }
        
        [JsonProperty(PropertyName = "high")]
        public string High { get; set; }
        
        [JsonProperty(PropertyName = "low")]
        public string Low { get; set; }
        
        [JsonProperty(PropertyName = "close")]
        public string Close { get; set; }
        
        [JsonProperty(PropertyName = "volume")]
        public string Volume { get; set; }

        // Property to set and retrieve the bucketed JSON data
        public List<Bucket> List { get => _bucketList; }

        // Property to retrieve the last element of a bucketed JSON list
        public Bucket Last { get => _last; }

        internal void AddFromJson(string json) {
            
             _bucketList = new List<Bucket>();
            
            // Convert JSON string from array object 
            // to collection of singletons
            json = json.Replace("[", string.Empty);
            json = json.Replace("]", string.Empty);
            
            // Split JSON string into bucket array keeping delimiter
            string[] jsonStringArray = json.Split("},");
            // Remove array last element closing curly brace 
            // to avoid JSON parse error
            int i = jsonStringArray.Length -1;
            string lastElement = (jsonStringArray[i])
                .Replace("}", string.Empty);
            jsonStringArray[i] = lastElement;
            
            // Add each JSON item to bucket list
            StringBuilder sb = new StringBuilder();
            string item;
            foreach (string element in jsonStringArray) {
                item = $"{element.Substring(0, element.Length)}}}";
                // Deserialise reconstructed element and add to list
                _bucketList.Add(JsonConvert.DeserializeObject<Bucket>(item));
            }
            
            _last = _bucketList.Last();
        }
    }
}