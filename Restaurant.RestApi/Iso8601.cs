/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public static class Iso8601
    {
        public static string ToIso8601DateString(this DateTime date)
        {
            return date.ToString(
                "yyyy'-'MM'-'dd",
                CultureInfo.InvariantCulture);
        }

        public static string ToIso8601TimeString(this TimeSpan time)
        {
            return time.ToString("T", CultureInfo.InvariantCulture);
        }

        public static string ToIso8601DateTimeString(this DateTime date)
        {
            return date.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
