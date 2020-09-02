/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class SystemClock : IClock
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
