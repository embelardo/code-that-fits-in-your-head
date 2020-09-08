/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class CalendarController
    {
        public CalendarController(
            IRestaurantDatabase restaurantDatabase,
            IReservationsRepository repository)
        {
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
        }

        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }

        [HttpGet("calendar/{year}"), ResponseCache(Duration = 60)]
        public Task<ActionResult> GetYear(int year)
        {
            var result = new RedirectToActionResult(
                nameof(GetYear),
                null,
                new { restaurantId = Grandfather.Id, year },
                permanent: true);
            return Task.FromResult<ActionResult>(result);
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
            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            var period = Period.Year(year);
            var days = await MakeDays(restaurantId, maitreD, period)
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
            var result = new RedirectToActionResult(
                nameof(GetMonth),
                null,
                new { restaurantId = Grandfather.Id, year, month },
                permanent: true);
            return Task.FromResult<ActionResult>(result);
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
            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            var period = Period.Month(year, month);
            var days = await MakeDays(restaurantId, maitreD, period)
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
            var result = new RedirectToActionResult(
                nameof(GetDay),
                null,
                new { restaurantId = Grandfather.Id, year, month, day },
                permanent: true);
            return Task.FromResult<ActionResult>(result);
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
            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            var period = Period.Day(year, month, day);
            var days = await MakeDays(restaurantId, maitreD, period)
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

        private async Task<DayDto[]> MakeDays(
            int restaurantId,
            MaitreD maitreD,
            IPeriod period)
        {
            var firstTick = period.Accept(new FirstTickVisitor());
            var lastTick = period.Accept(new LastTickVisitor());
            var reservations = await Repository
                .ReadReservations(restaurantId, firstTick, lastTick)
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
