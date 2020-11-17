/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Seating
    {
        public Seating(TimeSpan seatingDuration, DateTime at)
        {
            SeatingDuration = seatingDuration;
            At = at;
        }

        public TimeSpan SeatingDuration { get; }
        public DateTime At { get; }

        public DateTime Start
        {
            get { return At; }
        }

        public DateTime End
        {
            get { return Start + SeatingDuration; }
        }

        public bool Overlaps(Reservation otherRestaurant)
        {
            if (otherRestaurant is null)
                throw new ArgumentNullException(nameof(otherRestaurant));

            var other = new Seating(SeatingDuration, otherRestaurant.At);
            return Overlaps(other);
        }

        public bool Overlaps(Seating other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            return Start < other.End && other.Start < End;
        }

        public override bool Equals(object? obj)
        {
            return obj is Seating seating &&
                   SeatingDuration.Equals(seating.SeatingDuration) &&
                   At == seating.At;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SeatingDuration, At);
        }
    }
}
