/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class NullPostOffice : IPostOffice
    {
        public static readonly NullPostOffice Instance = new NullPostOffice();

        private NullPostOffice()
        {
        }

        public Task EmailReservationCreated(Reservation reservation)
        {
            return Task.CompletedTask;
        }

        public Task EmailReservationDeleted(Reservation reservation)
        {
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdated(Reservation reservation)
        {
            return Task.CompletedTask;
        }
    }
}