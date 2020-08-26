/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.Generic;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class ReservationsVisitor :
        ITableVisitor<IEnumerable<Reservation>>
    {
        public IEnumerable<Reservation> VisitCommunal(
            int seats,
            IReadOnlyCollection<Reservation> reservations)
        {
            return reservations;
        }

        public IEnumerable<Reservation> VisitStandard(
            int seats,
            Reservation? reservation)
        {
            if (reservation is { })
                yield return reservation;
        }
    }
}
