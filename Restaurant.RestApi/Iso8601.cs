/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public static class Iso8601
    {
        public static string ToIso8601DateString(this DateTime date)
        {
            return date.ToString(
                "yyyy'-'MM'-'dd",
                CultureInfo.InvariantCulture);
        }
    }
}
