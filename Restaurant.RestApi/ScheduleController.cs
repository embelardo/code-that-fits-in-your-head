/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("schedule")]
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

        [HttpGet("{year}/{month}/{day}"), Authorize(Roles = "MaitreD")]
        public Task<ActionResult> Get(int year, int month, int day)
        {
            return Get(Grandfather.Id, year, month, day);
        }

        [Authorize(Roles = "MaitreD")]
        [HttpGet("{restaurantId}/{year}/{month}/{day}")]
        public async Task<ActionResult> Get(
            int restaurantId,
            int year,
            int month,
            int day)
        {
            if (!AccessControlList.Authorize(restaurantId))
                return new UnauthorizedResult();

            var name = await RestaurantDatabase.GetName(restaurantId)
                .ConfigureAwait(false);
            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            var date = new DateTime(year, month, day);
            var firstTick = date;
            var lastTick = firstTick.AddDays(1).AddTicks(-1);
            var reservations = await Repository
                .ReadReservations(restaurantId, firstTick, lastTick)
                .ConfigureAwait(false);

            var schedule = maitreD.Schedule(reservations);

            var dto = MakeCalendar(date, schedule);
            dto.Name = name;
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
