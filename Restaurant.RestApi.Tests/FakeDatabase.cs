/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    [SuppressMessage(
        "Naming",
        "CA1710:Identifiers should have correct suffix",
        Justification = "The role of the class is a Test Double.")]
    public class FakeDatabase :
        Collection<Reservation>, IReservationsRepository
    {
        public Task Create(Reservation reservation)
        {
            Add(reservation);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime dateTime)
        {
            var min = dateTime.Date;
            var max = min.AddDays(1).AddTicks(-1);

            return Task.FromResult<IReadOnlyCollection<Reservation>>(
                this.Where(r => min <= r.At && r.At <= max).ToList());
        }

        public Task<Reservation?> ReadReservation(Guid id)
        {
            var reservation = this.FirstOrDefault(r => r.Id == id);
            return Task.FromResult((Reservation?)reservation);
        }

        public Task Delete(Guid id)
        {
            var reservation = this.SingleOrDefault(r => r.Id == id);
            if (reservation is { })
                Remove(reservation);

            return Task.CompletedTask;
        }
    }
}