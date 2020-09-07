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
            MaitreD maitreD)
        {
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
            MaitreD = maitreD;
        }

        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }
        public MaitreD MaitreD { get; }

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
            var name = await RestaurantDatabase.GetName(restaurantId)
                .ConfigureAwait(false);

            var date = new DateTime(year, month, day);
            var firstTick = date;
            var lastTick = firstTick.AddDays(1).AddTicks(-1);
            var reservations = await Repository
                .ReadReservations(Grandfather.Id, firstTick, lastTick)
                .ConfigureAwait(false);

            var schedule = MaitreD.Schedule(reservations);

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
