/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal sealed class TimeDtoComparer : IEqualityComparer<TimeDto>
    {
        public bool Equals([AllowNull] TimeDto x, [AllowNull] TimeDto y)
        {
            return Equals(x?.Time, y?.Time)
                && Equals(x?.MaximumPartySize, y?.MaximumPartySize);
        }

        public int GetHashCode([DisallowNull] TimeDto obj)
        {
            return HashCode.Combine(obj.Time, obj.MaximumPartySize);
        }
    }
}