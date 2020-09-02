/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class Month : IPeriod
    {
        private readonly int year;
        private readonly int month;

        public Month(int year, int month)
        {
            this.year = year;
            this.month = month;
        }

        public T Accept<T>(IPeriodVisitor<T> visitor)
        {
            return visitor.VisitMonth(year, month);
        }

        public override bool Equals(object? obj)
        {
            return obj is Month month &&
                   year == month.year &&
                   this.month == month.month;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(year, month);
        }
    }
}
