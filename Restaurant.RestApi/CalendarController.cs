/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class CalendarController
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

        [Obsolete("Use Get method with restaurant ID.")]
        [HttpGet("calendar/{year}"), ResponseCache(Duration = 60)]
        public ActionResult LegacyGet(int year)
        {
            return new RedirectToActionResult(
                nameof(Get),
                null,
                new { restaurantId = Grandfather.Id, year },
                permanent: true);
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
        public async Task<ActionResult> Get(int restaurantId, int year)
        {
            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            var period = Period.Year(year);
            var days = await MakeDays(restaurant, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = restaurant.Name,
                    Year = year,
                    Days = days
                });
        }

        [Obsolete("Use Get method with restaurant ID.")]
        [HttpGet("calendar/{year}/{month}")]
        public ActionResult LegacyGet(int year, int month)
        {
            return new RedirectToActionResult(
                nameof(Get),
                null,
                new { restaurantId = Grandfather.Id, year, month },
                permanent: true);
        }

        /* See comment about Get(int year). */
        [HttpGet("restaurants/{restaurantId}/calendar/{year}/{month}")]
        public async Task<ActionResult> Get(
            int restaurantId,
            int year,
            int month)
        {
            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            var period = Period.Month(year, month);
            var days = await MakeDays(restaurant, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = restaurant.Name,
                    Year = year,
                    Month = month,
                    Days = days
                });
        }

        [Obsolete("Use Get method with restaurant ID.")]
        [HttpGet("calendar/{year}/{month}/{day}")]
        public ActionResult LegacyGet(int year, int month, int day)
        {
            return new RedirectToActionResult(
                nameof(Get),
                null,
                new { restaurantId = Grandfather.Id, year, month, day },
                permanent: true);
        }

        [HttpGet("restaurants/{restaurantId}/calendar/{year}/{month}/{day}")]
        public async Task<ActionResult> Get(
            int restaurantId,
            int year,
            int month,
            int day)
        {
            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            var period = Period.Day(year, month, day);
            var days = await MakeDays(restaurant, period)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new CalendarDto
                {
                    Name = restaurant.Name,
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = days
                });
        }

        private async Task<DayDto[]> MakeDays(
            Restaurant restaurant,
            IPeriod period)
        {
            var reservations = await Repository
                .ReadReservations(restaurant.Id, period)
                .ConfigureAwait(false);

            var days = period.Accept(new DaysVisitor())
                .Select(d => MakeDay(d, reservations, restaurant.MaitreD!))
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
                .Select(ts => new TimeDto
                {
                    Time = ts.At.TimeOfDay.ToIso8601TimeString(),
                    MaximumPartySize = ts.Tables.Max(t => t.RemainingSeats)
                })
                .ToArray();

            return new DayDto
            {
                Date = date.ToIso8601DateString(),
                Entries = segments
            };
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
