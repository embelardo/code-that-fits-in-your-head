/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
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

        public async Task Delete(int restaurantId, Guid id)
        {
            Logger.LogInformation(
                "{method}(id: {id})",
                nameof(Delete),
                id);
            await Inner.Delete(restaurantId, id).ConfigureAwait(false);
        }

        public async Task<Reservation?> ReadReservation(
            int restaurantId, Guid id)
        {
            var output = await Inner.ReadReservation(restaurantId, id)
                .ConfigureAwait(false);
            Logger.LogInformation(
                "{method}(id: {id}) => {output}",
                nameof(ReadReservation),
                id,
                JsonSerializer.Serialize(output?.ToDto()));
            return output;
        }

        public async Task<IReadOnlyCollection<Reservation>> ReadReservations(
            int restaurantId,
            DateTime min,
            DateTime max)
        {

            var output = await Inner.ReadReservations(restaurantId, min, max)
                .ConfigureAwait(false);
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, min: {min}, max: {max}) => {output}",
                nameof(ReadReservations),
                restaurantId,
                min,
                max,
                JsonSerializer.Serialize(
                    output.Select(r => r.ToDto()).ToArray()));
            return output;
        }

        public async Task Update(int restaurantId, Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(reservation: {reservation})",
                nameof(Update),
                JsonSerializer.Serialize(reservation.ToDto()));
            await
                Inner.Update(restaurantId, reservation).ConfigureAwait(false);
        }
    }
}
