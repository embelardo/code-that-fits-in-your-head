/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class LoggingReservationsRepository : IReservationsRepository
    {
        public LoggingReservationsRepository(
            ILogger<LoggingReservationsRepository> logger,
            IReservationsRepository inner)
        {
            Logger = logger;
            Inner = inner;
        }

        public ILogger<LoggingReservationsRepository> Logger { get; }
        public IReservationsRepository Inner { get; }

        public async Task Create(int restaurantId, Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, reservation: {reservation})",
                nameof(Create),
                restaurantId,
                JsonSerializer.Serialize(reservation.ToDto()));
            await Inner.Create(restaurantId, reservation).ConfigureAwait(false);
        }

        public Task Delete(Guid id)
        {
            return Inner.Delete(id);
        }

        public Task<Reservation?> ReadReservation(Guid id)
        {
            return Inner.ReadReservation(id);
        }

        public Task<IReadOnlyCollection<Reservation>> ReadReservations(
            int restaurantId,
            DateTime min,
            DateTime max)
        {
            return Inner.ReadReservations(restaurantId, min, max);
        }

        public Task Update(Reservation reservation)
        {
            return Inner.Update(reservation);
        }
    }
}
