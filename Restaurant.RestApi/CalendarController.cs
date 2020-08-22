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
            var daysInYear = new GregorianCalendar().GetDaysInYear(year);
            var firstDay = new DateTime(year, 1, 1);
            var lastDay = firstDay.AddYears(1).AddTicks(-1);
            var reservations = await Repository
                .ReadReservations(firstDay, lastDay).ConfigureAwait(false);
            var days = Enumerable.Range(0, daysInYear)
                .Select(i => MakeDay(firstDay, i, reservations))
                .ToArray();
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
            var daysInMonth =
                new GregorianCalendar().GetDaysInMonth(year, month);
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddTicks(-1);
            var reservations = await Repository
                .ReadReservations(firstDay, lastDay).ConfigureAwait(false);
            var days = Enumerable.Range(0, daysInMonth)
                .Select(i => MakeDay(firstDay, i, reservations))
                .ToArray();
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
            var firstDay = new DateTime(year, month, day);
            var reservations = await Repository.ReadReservations(firstDay)
                .ConfigureAwait(false);
            var days = new[] { MakeDay(new DateTime(year, month, day), 0, reservations) };
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = days
                });
        }

        private DayDto MakeDay(
            DateTime origin,
            int offset,
            IEnumerable<Reservation> reservations)
        {
            var entries = MaitreD
                .Segment(origin.AddDays(offset), reservations)
                .Select(o => new TimeDto
                {
                    Time = o.At.TimeOfDay.ToIso8601TimeString(),
                    MaximumPartySize = o.Value.Max(t => t.RemainingSeats)
                })
                .ToArray();
            return new DayDto
            {
                Date = origin.AddDays(offset).ToIso8601DateString(),
                Entries = entries
            };
        }
    }
}
