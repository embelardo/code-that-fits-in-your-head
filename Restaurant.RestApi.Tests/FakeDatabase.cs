/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
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
            int restaurantId,
            DateTime min,
            DateTime max)
        {
            return Task.FromResult<IReadOnlyCollection<Reservation>>(
                GetOrAdd(restaurantId, new Collection<Reservation>())
                    .Where(r => min <= r.At && r.At <= max).ToList());
        }

        public Task<Reservation?> ReadReservation(int restaurantId, Guid id)
        {
            var reservation = Values
                .SelectMany(rs => rs)
                .FirstOrDefault(r => r.Id == id);
            return Task.FromResult((Reservation?)reservation);
        }

        public Task Update(int restaurantId, Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var restaurant =
                Values.Single(rs => rs.Any(r => r.Id == reservation.Id));

            var existing =
                restaurant.SingleOrDefault(r => r.Id == reservation.Id);
            if (existing is { })
                restaurant.Remove(existing);

            restaurant.Add(reservation);

            return Task.CompletedTask;
        }

        public Task Delete(Guid id)
        {
            var restaurant =
                Values.SingleOrDefault(rs => rs.Any(r => r.Id == id));
            if (restaurant is null)
                return Task.CompletedTask;

            var reservation = restaurant.SingleOrDefault(r => r.Id == id);
            if (reservation is { })
                restaurant.Remove(reservation);

            return Task.CompletedTask;
        }
    }
}