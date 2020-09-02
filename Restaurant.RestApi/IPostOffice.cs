/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public interface IPostOffice
    {
        Task EmailReservationCreated(
            int restaurantId,
            Reservation reservation);

        Task EmailReservationDeleted(
            int restaurantId,
            Reservation reservation);

        Task EmailReservationUpdating(
            int restaurantId,
            Reservation reservation);

        Task EmailReservationUpdated(
            int restaurantId,
            Reservation reservation);
    }
}
