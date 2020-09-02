/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal static class Period
    {
        internal static IPeriod Year(int year)
        {
            return new Year(year);
        }

        internal static IPeriod Month(int year, int month)
        {
            return new Month(year, month);
        }

        internal static IPeriod Day(int year, int month, int day)
        {
            return new Day(year, month, day);
        }
    }
}
