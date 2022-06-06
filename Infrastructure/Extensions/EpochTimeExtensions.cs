using System;
using System.Globalization;

namespace SecurePolicyBasedDataAccess.Infrastructure.Extensions
{
    public static class EpochTimeExtensions
    {
        public static string ToEpoch(this DateTime value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var elapsedTime = value - epoch;
            return ((long)elapsedTime.TotalSeconds).ToString(CultureInfo.CurrentCulture);
        }

        public static long ToEpochNumber(this DateTime value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var elapsedTime = value - epoch;
            return (long)elapsedTime.TotalSeconds;
        }
    }
}
