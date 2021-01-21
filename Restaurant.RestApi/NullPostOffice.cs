/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class NullPostOffice : IPostOffice
    {
        public static readonly NullPostOffice Instance = new NullPostOffice();

        private NullPostOffice()
        {
        }

        public Task EmailReservationCreated(
            int restaurantId,
            Reservation reservation)
        {
            return Task.CompletedTask;
        }

        public Task EmailReservationDeleted(
            int restaurantId,
            Reservation reservation)
        {
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdating(
            int restaurantId,
            Reservation reservation)
        {
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdated(
            int restaurantId,
            Reservation reservation)
        {
            return Task.CompletedTask;
        }
    }
}