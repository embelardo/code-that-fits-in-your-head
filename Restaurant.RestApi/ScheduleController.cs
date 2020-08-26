/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("schedule")]
    public class ScheduleController
    {
        public ScheduleController(IReservationsRepository repository)
        {
            Repository = repository;
        }

        public IReservationsRepository Repository { get; }

#pragma warning disable CA1822 // Mark members as static
        [HttpGet("{year}/{month}/{day}"), Authorize(Roles = "MaitreD")]
        public ActionResult Get(int year, int month, int day)
#pragma warning restore CA1822 // Mark members as static
        {
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
                            Date = new DateTime(year, month, day)
                                .ToIso8601DateString(),
                            Entries = Array.Empty<TimeDto>()
                        } 
                    }
                });
        }
    }
}
