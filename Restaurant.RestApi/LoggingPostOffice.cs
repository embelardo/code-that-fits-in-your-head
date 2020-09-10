/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
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

        public async Task EmailReservationCreated(Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(reservation: {reservation})",
                nameof(EmailReservationCreated),
                reservation.ToDto());
            await Inner.EmailReservationCreated(reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationDeleted(Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(reservation: {reservation})",
                nameof(EmailReservationDeleted),
                reservation.ToDto());
            await Inner.EmailReservationDeleted(reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationUpdated(Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(reservation: {reservation})",
                nameof(EmailReservationUpdated),
                reservation.ToDto());
            await Inner.EmailReservationUpdated(reservation)
                .ConfigureAwait(false);
        }

        public async Task EmailReservationUpdating(Reservation reservation)
        {
            Logger.LogInformation(
                "{method}(reservation: {reservation})",
                nameof(EmailReservationUpdating),
                reservation.ToDto());
            await Inner.EmailReservationUpdating(reservation)
                .ConfigureAwait(false);
        }
    }
}
