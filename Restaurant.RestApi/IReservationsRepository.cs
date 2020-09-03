/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public interface IReservationsRepository
    {
        Task Create(Reservation reservation);
        Task Create(int restaurantId, Reservation reservation);

        Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime min, DateTime max);

        Task<Reservation?> ReadReservation(Guid id);

        Task Update(Reservation reservation);

        Task Delete(Guid id);
    }
}