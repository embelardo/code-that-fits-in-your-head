/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class Day : IPeriod
    {
        private readonly int year;
        private readonly int month;
        private readonly int day;

        public Day(int year, int month, int day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public T Accept<T>(IPeriodVisitor<T> visitor)
        {
            return visitor.VisitDay(year, month, day);
        }

        public override bool Equals(object? obj)
        {
            return obj is Day day &&
                   month == day.month;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(month);
        }
    }
}
