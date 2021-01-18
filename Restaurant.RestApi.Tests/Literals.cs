using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class Literals
    {
        internal static TimeSpan Days(this int days)
        {
            return TimeSpan.FromDays(days);
        }
    }
}
