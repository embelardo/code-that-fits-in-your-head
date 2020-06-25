/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class SpyPostOffice : Collection<Reservation>, IPostOffice
    {
        public Task EmailReservationCreated(Reservation reservation)
        {
            Add(reservation);
            return Task.CompletedTask;
        }
    }
}