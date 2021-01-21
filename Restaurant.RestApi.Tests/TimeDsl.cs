using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class TimeDsl
    {
        internal static DateTime At(
            this DateTime dateTime,
            int hours,
            int minutes)
        {
            return dateTime.Date.Add(new TimeSpan(hours, minutes, 0));
        }
    }
}
