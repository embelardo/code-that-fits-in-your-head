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
    public sealed class ScheduleController
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
        public ActionResult Get(int year, int month, int day)
        {
            return new RedirectToActionResult(
                nameof(Get),
                null,
                new { restaurantId = Grandfather.Id, year, month, day },
                permanent: true);
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

        private static CalendarDto MakeCalendar(
            DateTime date,
            IEnumerable<TimeSlot> schedule)
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

        private static TimeDto MakeEntry(TimeSlot timeSlot)
        {
            return new TimeDto
            {
                Time = timeSlot.At.TimeOfDay.ToIso8601TimeString(),
                Reservations = timeSlot.Tables
                    .SelectMany(t => t.Accept(ReservationsVisitor.Instance))
                    .Select(r => r.ToDto())
                    .ToArray()
            };
        }
    }
}
