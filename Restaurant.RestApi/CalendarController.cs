/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("calendar")]
    public class CalendarController
    {
        public CalendarController(
            IReservationsRepository repository,
            MaitreD maitreD)
        {
            Repository = repository;
            MaitreD = maitreD;
        }

        public IReservationsRepository Repository { get; }
        public MaitreD MaitreD { get; }

        /* This method loads a year's worth of reservation in order to segment
         * them all. In a realistic system, this could be quite stressful for
         * both the database and the web server. Some of that concern can be
         * addressed with an appropriate HTTP cache header and a reverse proxy,
         * but a better solution would be a CQRS-style architecture where the
         * calendars get re-rendered as materialised views in a background
         * process. That's beyond the scope of this example code base, though.
         */
        [HttpGet("{year}")]
        public async Task<ActionResult> Get(int year)
        {
            var period = Period.Year(year);
            var days = await MakeDays(period).ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Days = days
                });
        }

        /* See comment about Get(int year). */
        [HttpGet("{year}/{month}")]
        public async Task<ActionResult> Get(int year, int month)
        {
            var period = Period.Month(year, month);
            var days = await MakeDays(period).ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Days = days
                });
        }

        [HttpGet("{year}/{month}/{day}")]
        public async Task<ActionResult> Get(int year, int month, int day)
        {
            var period = Period.Day(year, month, day);
            var days = await MakeDays(period).ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = days
                });
        }

        private async Task<DayDto[]> MakeDays(IPeriod period)
        {
            var firstTick = period.Accept(new FirstTickVisitor());
            var lastTick = period.Accept(new LastTickVisitor());
            var reservations = await Repository
                .ReadReservations(firstTick, lastTick).ConfigureAwait(false);

            var days = period.Accept(new DaysVisitor())
                .Select(d => MakeDay(d, reservations))
                .ToArray();
            return days;
        }

        private DayDto MakeDay(
            DateTime date,
            IReadOnlyCollection<Reservation> reservations)
        {
            var segments = MaitreD
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
