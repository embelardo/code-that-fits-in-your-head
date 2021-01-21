/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class EmailingReservationsRepository :
        IReservationsRepository
    {
        public EmailingReservationsRepository(
            IPostOffice postOffice,
            IReservationsRepository inner)
        {
            PostOffice = postOffice;
            Inner = inner;
        }

        public IPostOffice PostOffice { get; }
        public IReservationsRepository Inner { get; }

        public async Task Create(int restaurantId, Reservation reservation)
        {
            await Inner.Create(restaurantId, reservation)
                .ConfigureAwait(false);
            await PostOffice.EmailReservationCreated(restaurantId, reservation)
                .ConfigureAwait(false);
        }

        public async Task Delete(int restaurantId, Guid id)
        {
            var existing = await Inner.ReadReservation(restaurantId, id)
                .ConfigureAwait(false);

            await Inner.Delete(restaurantId, id).ConfigureAwait(false);

            if (existing is { })
                await PostOffice
                    .EmailReservationDeleted(restaurantId, existing)
                    .ConfigureAwait(false);
        }

        public Task<Reservation?> ReadReservation(int restaurantId, Guid id)
        {
            return Inner.ReadReservation(restaurantId, id);
        }

        public Task<IReadOnlyCollection<Reservation>> ReadReservations(
            int restaurantId,
            DateTime min,
            DateTime max)
        {
            return Inner.ReadReservations(restaurantId, min, max);
        }

        public async Task Update(int restaurantId, Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            var existing =
                await Inner.ReadReservation(restaurantId, reservation.Id)
                    .ConfigureAwait(false);
            if (existing is { } && existing.Email != reservation.Email)
                await PostOffice
                    .EmailReservationUpdating(restaurantId, existing)
                    .ConfigureAwait(false);

            await Inner.Update(restaurantId, reservation)
                .ConfigureAwait(false);

            await PostOffice.EmailReservationUpdated(restaurantId, reservation)
                .ConfigureAwait(false);
        }
    }
}
