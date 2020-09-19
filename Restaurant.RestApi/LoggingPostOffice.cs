/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class LoggingPostOffice : IPostOffice
    {
        public LoggingPostOffice(
            ILogger<LoggingPostOffice> logger,
            IPostOffice inner)
        {
            Logger = logger;
            Inner = inner;
        }

        public ILogger<LoggingPostOffice> Logger { get; }
        public IPostOffice Inner { get; }

        public async Task EmailReservationCreated(
            int restaurantId,
            Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, reservation: {reservation})",
                nameof(EmailReservationCreated),
                restaurantId,
                JsonSerializer.Serialize(reservation.ToDto()));
            await Inner.EmailReservationCreated(restaurantId, reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationDeleted(
            int restaurantId,
            Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, reservation: {reservation})",
                nameof(EmailReservationDeleted),
                restaurantId,
                JsonSerializer.Serialize(reservation.ToDto()));
            await Inner.EmailReservationDeleted(restaurantId, reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationUpdated(
            int restaurantId,
            Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, reservation: {reservation})",
                nameof(EmailReservationUpdated),
                restaurantId,
                JsonSerializer.Serialize(reservation.ToDto()));
            await Inner.EmailReservationUpdated(restaurantId, reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationUpdating(
            int restaurantId,
            Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(restaurantId: {restaurantId}, reservation: {reservation})",
                nameof(EmailReservationUpdating),
                restaurantId,
                JsonSerializer.Serialize(reservation.ToDto()));
            await Inner.EmailReservationUpdating(restaurantId, reservation)
                .ConfigureAwait(false);
        }
    }
}
