namespace Bitmex {
    
    using System.Security.Cryptography;
    using System.Text;
    using System;
    class Utils {

        
        internal static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        internal long GetExpires()
            // set expires one hour in the future
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;

        internal byte[] HmacSha256(byte[] keyByte, byte[] messageBytes){
            using (var hash = new HMACSHA256(keyByte)) {
                return hash.ComputeHash(messageBytes);
            }
        }
    }
}