/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Concurrent;
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
        ConcurrentDictionary<int, Collection<Reservation>>,
        IReservationsRepository
    {
        public FakeDatabase()
        {
            Grandfather = new Collection<Reservation>();
            AddOrUpdate(RestApi.Grandfather.Id, Grandfather, (_, rs) => rs);
        }

        /// <summary>
        /// The 'original' restaurant 'grandfathered' in.
        /// </summary>
        /// <seealso cref="RestApi.Grandfather" />
        public Collection<Reservation> Grandfather { get; }

        public Task Create(int restaurantId, Reservation reservation)
        {
            AddOrUpdate(
                restaurantId,
                new Collection<Reservation> { reservation },
                (_, rs) => { rs.Add(reservation); return rs; });
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime min,
            DateTime max)
        {
            return ReadReservations(RestApi.Grandfather.Id, min, max);
        }

        public Task<IReadOnlyCollection<Reservation>> ReadReservations(
            int restaurantId,
            DateTime min,
            DateTime max)
        {
            return Task.FromResult<IReadOnlyCollection<Reservation>>(
                GetOrAdd(restaurantId, new Collection<Reservation>())
                    .Where(r => min <= r.At && r.At <= max).ToList());
        }

        public Task<Reservation?> ReadReservation(Guid id)
        {
            var reservation =
                GetOrAdd(RestApi.Grandfather.Id, new Collection<Reservation>())
                .FirstOrDefault(r => r.Id == id);
            return Task.FromResult((Reservation?)reservation);
        }

        public async Task Update(Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            await Delete(reservation.Id);
            await Create(RestApi.Grandfather.Id, reservation);
        }

        public Task Delete(Guid id)
        {
            var reservations =
                GetOrAdd(RestApi.Grandfather.Id, new Collection<Reservation>());
            var reservation = reservations.SingleOrDefault(r => r.Id == id);
            if (reservation is { })
                reservations.Remove(reservation);

            return Task.CompletedTask;
        }
    }
}