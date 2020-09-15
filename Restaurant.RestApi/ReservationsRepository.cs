/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal static class ReservationsRepository
    {
        internal static Task<IReadOnlyCollection<Reservation>> ReadReservations(
            this IReservationsRepository repository,
            int restaurantId,
            DateTime date)
        {
            var min = date.Date;
            var max = min.AddDays(1).AddTicks(-1);
            return repository.ReadReservations(restaurantId, min, max);
        }

        internal static async Task<IReadOnlyCollection<Reservation>> ReadReservations(
            this IReservationsRepository repository,
            int restaurantId,
            IPeriod period)
        {
            var firstTick = period.Accept(new FirstTickVisitor());
            var lastTick = period.Accept(new LastTickVisitor());
            return await repository
                .ReadReservations(restaurantId, firstTick, lastTick)
                .ConfigureAwait(false);
        }

        private sealed class FirstTickVisitor : IPeriodVisitor<DateTime>
        {
            public DateTime VisitDay(int year, int month, int day)
            {
                return new DateTime(year, month, day);
            }

            public DateTime VisitMonth(int year, int month)
            {
                return new DateTime(year, month, 1);
            }

            public DateTime VisitYear(int year)
            {
                return new DateTime(year, 1, 1);
            }
        }

        private sealed class LastTickVisitor : IPeriodVisitor<DateTime>
        {
            public DateTime VisitDay(int year, int month, int day)
            {
                return new DateTime(year, month, day).AddDays(1).AddTicks(-1);
            }

            public DateTime VisitMonth(int year, int month)
            {
                return new DateTime(year, month, 1).AddMonths(1).AddTicks(-1);
            }

            public DateTime VisitYear(int year)
            {
                return new DateTime(year, 1, 1).AddYears(1).AddTicks(-1);
            }
        }
    }
}
