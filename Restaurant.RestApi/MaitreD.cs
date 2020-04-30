/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class MaitreD
    {
        private readonly Table table;

        public MaitreD(Table table)
        {
            this.table = table;
        }

        public bool WillAccept(
            IEnumerable<Reservation> existingReservations,
            Reservation candidate)
        {
            if (candidate is null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            int reservedSeats = existingReservations.Sum(r => r.Quantity);
            return reservedSeats + candidate.Quantity <= table.Seats;
        }
    }
}
