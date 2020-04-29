/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    internal static class MaitreD
    {
        internal static bool WillAccept(
            IEnumerable<Reservation> existingReservations,
            Reservation candidate)
        {
            int reservedSeats = existingReservations.Sum(r => r.Quantity);
            return reservedSeats + candidate.Quantity <= 10;
        }
    }
}
