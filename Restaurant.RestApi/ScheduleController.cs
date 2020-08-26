/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("schedule")]
    public class ScheduleController
    {
        public ScheduleController(
            IReservationsRepository repository,
            MaitreD maitreD)
        {
            Repository = repository;
            MaitreD = maitreD;
        }

        public IReservationsRepository Repository { get; }
        public MaitreD MaitreD { get; }

        [HttpGet("{year}/{month}/{day}"), Authorize(Roles = "MaitreD")]
        public async Task<ActionResult> Get(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var firstTick = date;
            var lastTick = firstTick.AddDays(1).AddTicks(-1);
            var reservations = await Repository.ReadReservations(firstTick, lastTick).ConfigureAwait(false);
            var schedule = MaitreD.Schedule(reservations);
            var entries = schedule.Select(o => new TimeDto
            {
                Time = o.At.TimeOfDay.ToIso8601TimeString(),
                Reservations = o.Value
                    .SelectMany(t => t.Accept(ReservationsVisitor.Instance))
                    .Select(r => r.ToDto())
                    .ToArray()
            }).ToArray();

            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = new[]
                    {
                        new DayDto 
                        {
                            Date = date.ToIso8601DateString(),
                            Entries = entries
                        } 
                    }
                });
        }
    }
}
