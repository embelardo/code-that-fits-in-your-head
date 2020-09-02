/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.Generic;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class ReservationsVisitor :
        ITableVisitor<IEnumerable<Reservation>>
    {
        public static readonly ReservationsVisitor Instance =
            new ReservationsVisitor();

        private ReservationsVisitor() { }

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
