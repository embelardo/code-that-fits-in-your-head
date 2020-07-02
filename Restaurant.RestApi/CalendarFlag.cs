/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    // Delete this class once the calendar feature is ready for production.
    public class CalendarFlag
    {
        public CalendarFlag(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }

        public override bool Equals(object? obj)
        {
            return obj is CalendarFlag flag &&
                   Enabled == flag.Enabled;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Enabled);
        }
    }
}
