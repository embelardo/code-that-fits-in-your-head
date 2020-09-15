/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [Authorize(Roles = "MaitreD")]
    public class ScheduleController
    {
        public ScheduleController(
            IRestaurantDatabase restaurantDatabase,
            IReservationsRepository repository,
            AccessControlList accessControlList)
        {
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
            AccessControlList = accessControlList;
        }

        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }
        public AccessControlList AccessControlList { get; }

        [Obsolete("Use Get method with restaurant ID.")]
        [HttpGet("schedule/{year}/{month}/{day}")]
        public Task<ActionResult> Get(int year, int month, int day)
        {
            var result = new RedirectToActionResult(
                nameof(Get),
                null,
                new { restaurantId = Grandfather.Id, year, month, day },
                permanent: true);
            return Task.FromResult<ActionResult>(result);
        }

        [HttpGet("restaurants/{restaurantId}/schedule/{year}/{month}/{day}")]
        public async Task<ActionResult> Get(
            int restaurantId,
            int year,
            int month,
            int day)
        {
            if (!AccessControlList.Authorize(restaurantId))
                return new ForbidResult();

            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            var reservations = await Repository
                .ReadReservations(restaurantId, Period.Day(year, month, day))
                .ConfigureAwait(false);
            var schedule = restaurant.MaitreD.Schedule(reservations);

            var dto = MakeCalendar(new DateTime(year, month, day), schedule);
            dto.Name = restaurant.Name;
            return new OkObjectResult(dto);
        }

        private CalendarDto MakeCalendar(
            DateTime date,
            IEnumerable<Occurrence<IEnumerable<Table>>> schedule)
        {
            var entries = schedule.Select(MakeEntry).ToArray();

            return new CalendarDto
            {
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                Days = new[]
                {
                    new DayDto
                    {
                        Date = date.ToIso8601DateString(),
                        Entries = entries
                    }
                }
            };
        }

        private TimeDto MakeEntry(Occurrence<IEnumerable<Table>> occurrence)
        {
            return new TimeDto
            {
                Time = occurrence.At.TimeOfDay.ToIso8601TimeString(),
                Reservations = occurrence.Value
                    .SelectMany(t => t.Accept(ReservationsVisitor.Instance))
                    .Select(r => r.ToDto())
                    .ToArray()
            };
        }
    }
}
