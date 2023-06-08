using System;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;

namespace StyexFleetManagement.Salus
{
    public class Utils
    {
        private static readonly DateTime Jan1St1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }

        public static string Timestamp()
        {
            return CurrentTimeMillis().ToString();
        }

        public static string GetMd5Key(string input)
        {
            try
            {
                var bytesOfMessage = Encoding.ASCII.GetBytes(input);

                var md5Bytes = MD5.Create().ComputeHash(bytesOfMessage);

                var hexStringBuilder = new StringBuilder();
                foreach (var value in md5Bytes)
                {
                    var hexValue = $"{Convert.ToInt32(0xff & value):x}";
                    if (hexValue.Length == 1)
                    {
                        hexValue = $"0{hexValue}";
                    }

                    hexStringBuilder.Append(hexValue);
                }

                return hexStringBuilder.ToString();
            }
            catch (Exception exception)
            {
                Serilog.Log.Error(exception, exception.Message);
                throw;
            }
        }

        public static string DeviceType()
        {
            return Device.RuntimePlatform == Device.Android ? Constants.DeviceTypeAndroid : Constants.DeviceTypeIos;
        }
    }
}