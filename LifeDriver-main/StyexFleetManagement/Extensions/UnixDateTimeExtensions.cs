using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Extensions
{
    public static class UnixDateTimeExtensions
    {
        private static readonly DateTime UtcDate_1970_01_01 = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);


        /// <summary>
        /// Converts UTC DateTime to unix seconds (since 1970-01-01 00:00:00 UTC).
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        public static ulong ToUnixSeconds(this DateTime dateTime)
        {
            return (ulong)dateTime.ToUniversalTime().Subtract(UtcDate_1970_01_01).TotalSeconds;
        }
    }
}
