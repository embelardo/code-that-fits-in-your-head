/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Seating
    {
        public Seating(TimeSpan seatingDuration, Reservation reservation)
        {
            SeatingDuration = seatingDuration;
            Reservation = reservation;
        }

        public TimeSpan SeatingDuration { get; }
        public Reservation Reservation { get; }

        public DateTime Start
        {
            get { return Reservation.At; }
        }

        public DateTime End
        {
            get { return Start + SeatingDuration; }
        }

        public bool Overlaps(Reservation other)
        {
            var otherSeating = new Seating(SeatingDuration, other);
            return Overlaps(otherSeating);
        }

        public bool Overlaps(Seating otherSeating)
        {
            if (otherSeating is null)
                throw new ArgumentNullException(nameof(otherSeating));

            return Start < otherSeating.End && otherSeating.Start < End;
        }

        public override bool Equals(object? obj)
        {
            return obj is Seating seating &&
                   SeatingDuration.Equals(seating.SeatingDuration) &&
                   EqualityComparer<Reservation>.Default.Equals(Reservation, seating.Reservation);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SeatingDuration, Reservation);
        }
    }
}
