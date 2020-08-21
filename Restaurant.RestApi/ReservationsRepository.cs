/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    internal static class ReservationsRepository
    {
        internal static Task<IReadOnlyCollection<Reservation>> ReadReservations(
            this IReservationsRepository repository,
            DateTime date)
        {
            var min = date.Date;
            var max = min.AddDays(1).AddTicks(-1);
            return repository.ReadReservations(min, max);
        }
    }
}
