/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Options
{
    public sealed class RestaurantOptions
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan OpensAt { get; set; }
        public TimeSpan LastSeating { get; set; }
        public TimeSpan SeatingDuration { get; set; }

        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "With the .NET configuration system, it seems like it's either this, or some collection object with a public setter, which causes other code analysis warnings.")]
        public TableOptions[]? Tables { get; set; }

        internal Restaurant? ToRestaurant()
        {
            if (Name is null)
                return null;
            return new Restaurant(Id, Name, ToMaitreD());
        }

        private MaitreD ToMaitreD()
        {
            return new MaitreD(
                OpensAt,
                LastSeating,
                SeatingDuration,
                Tables.Select(ts => ts.ToTable()));
        }
    }
}
