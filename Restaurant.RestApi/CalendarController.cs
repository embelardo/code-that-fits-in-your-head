/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("")]
    public class CalendarController
    {
        public CalendarController(
            IRestaurantDatabase restaurantDatabase,
            IReservationsRepository repository,
            MaitreD maitreD)
        {
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
            MaitreD = maitreD;
        }

        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }
        public MaitreD MaitreD { get; }

        [HttpGet("calendar/{year}"), ResponseCache(Duration = 60)]
        public Task<ActionResult> GetYear(int year)
        {
            return GetYear(Grandfather.Id, year);
        }

        /* This method loads a year's worth of reservation in order to segment
         * them all. In a realistic system, this could be quite stressful for
         * both the database and the web server. Some of that concern can be
         * addressed with an appropriate HTTP cache header and a reverse proxy,
         * but a better solution would be a CQRS-style architecture where the
         * calendars get re-rendered as materialised views in a background
         * process. That's beyond the scope of this example code base, though.
         */
        [ResponseCache(Duration = 60)]
        [HttpGet("restaurants/{restaurantId}/calendar/{year}")]
        public async Task<ActionResult> GetYear(int restaurantId, int year)
        {
            var name = await RestaurantDatabase.GetName(restaurantId)
                .ConfigureAwait(false);

            var period = Period.Year(year);
            var days = await MakeDays(restaurantId, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = name,
                    Year = year,
                    Days = days
                });
        }

        [HttpGet("calendar/{year}/{month}")]
        public Task<ActionResult> GetMonth(int year, int month)
        {
            return GetMonth(Grandfather.Id, year, month);
        }

        /* See comment about Get(int year). */
        [HttpGet("restaurants/{restaurantId}/calendar/{year}/{month}")]
        public async Task<ActionResult> GetMonth(
            int restaurantId,
            int year,
            int month)
        {
            var name = await RestaurantDatabase.GetName(restaurantId)
                .ConfigureAwait(false);

            var period = Period.Month(year, month);
            var days = await MakeDays(restaurantId, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = name,
                    Year = year,
                    Month = month,
                    Days = days
                });
        }

        [HttpGet("calendar/{year}/{month}/{day}")]
        public Task<ActionResult> GetDay(int year, int month, int day)
        {
            return GetDay(Grandfather.Id, year, month, day);
        }

        [HttpGet("restaurants/{restaurantId}/calendar/{year}/{month}/{day}")]
        public async Task<ActionResult> GetDay(
            int restaurantId,
            int year,
            int month,
            int day)
        {
            var name = await RestaurantDatabase.GetName(restaurantId)
                .ConfigureAwait(false);

            var period = Period.Day(year, month, day);
            var days = await MakeDays(restaurantId, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = name,
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = days
                });
        }

        private async Task<DayDto[]> MakeDays(int restaurantId, IPeriod period)
        {
            var firstTick = period.Accept(new FirstTickVisitor());
            var lastTick = period.Accept(new LastTickVisitor());
            var reservations = await Repository
                .ReadReservations(restaurantId, firstTick, lastTick)
                .ConfigureAwait(false);

            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);

            var days = period.Accept(new DaysVisitor())
                .Select(d => MakeDay(d, reservations, maitreD!))
                .ToArray();
            return days;
        }

        private static DayDto MakeDay(
            DateTime date,
            IReadOnlyCollection<Reservation> reservations,
            MaitreD maitreD)
        {
            var segments = maitreD
                .Segment(date, reservations)
                .Select(o => new TimeDto
                {
                    Time = o.At.TimeOfDay.ToIso8601TimeString(),
                    MaximumPartySize = o.Value.Max(t => t.RemainingSeats)
                })
                .ToArray();

            return new DayDto
            {
                Date = date.ToIso8601DateString(),
                Entries = segments
            };
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

        private sealed class DaysVisitor :
            IPeriodVisitor<IEnumerable<DateTime>>
        {
            public IEnumerable<DateTime> VisitDay(int year, int month, int day)
            {
                return new[] { new DateTime(year, month, day) };
            }

            public IEnumerable<DateTime> VisitMonth(int year, int month)
            {
                var daysInMonth =
                    new GregorianCalendar().GetDaysInMonth(year, month);
                var firstDay = new DateTime(year, month, 1);
                return Enumerable.Range(0, daysInMonth)
                    .Select(i => firstDay.AddDays(i));
            }

            public IEnumerable<DateTime> VisitYear(int year)
            {
                var daysInYear = new GregorianCalendar().GetDaysInYear(year);
                var firstDay = new DateTime(year, 1, 1);
                return Enumerable.Range(0, daysInYear)
                    .Select(i => firstDay.AddDays(i));
            }
        }
    }
}
