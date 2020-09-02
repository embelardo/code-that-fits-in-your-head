/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class Year : IPeriod
    {
        private readonly int year;

        public Year(int year)
        {
            this.year = year;
        }

        public T Accept<T>(IPeriodVisitor<T> visitor)
        {
            return visitor.VisitYear(year);
        }

        public override bool Equals(object? obj)
        {
            return obj is Year year &&
                   this.year == year.year;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(year);
        }
    }
}
