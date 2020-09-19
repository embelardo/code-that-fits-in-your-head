/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class TimeSlot
    {
        public TimeSlot(DateTime at, params Table[] tables) :
            this(at, tables.ToList())
        {
        }

        public TimeSlot(DateTime at, IReadOnlyCollection<Table> tables)
        {
            At = at;
            Tables = tables;
        }

        public DateTime At { get; }
        public IReadOnlyCollection<Table> Tables { get; }

        public override bool Equals(object? obj)
        {
            return obj is TimeSlot other &&
                   At == other.At &&
                   Tables.SequenceEqual(other.Tables);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(At, Tables);
        }
    }
}
