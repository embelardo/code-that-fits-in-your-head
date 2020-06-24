/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public interface IReservationsRepository
    {
        Task Create(Reservation reservation);

        Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime dateTime);

        Task<Reservation?> ReadReservation(Guid id);

        Task Delete(Guid id);
    }
}