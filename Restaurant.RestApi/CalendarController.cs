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
        public CalendarController(MaitreD maitreD)
        {
            MaitreD = maitreD;
        }

        public MaitreD MaitreD { get; }

        [HttpGet("{year}")]
        public ActionResult Get(int year)
        {
            var daysInYear = new GregorianCalendar().GetDaysInYear(year);
            var firstDay = new DateTime(year, 1, 1);
            var days = Enumerable.Range(0, daysInYear)
                .Select(i => MakeDay(firstDay, i))
                .ToArray();
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Days = days
                });
        }

        [HttpGet("{year}/{month}")]
        public ActionResult Get(int year, int month)
        {
            var daysInMonth =
                new GregorianCalendar().GetDaysInMonth(year, month);
            var firstDay = new DateTime(year, month, 1);
            var days = Enumerable.Range(0, daysInMonth)
                .Select(i => MakeDay(firstDay, i))
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
        public ActionResult Get(int year, int month, int day)
        {
            var days = new[] { MakeDay(new DateTime(year, month, day), 0) };
            return new OkObjectResult(
                new CalendarDto
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Days = days
                });
        }

        private DayDto MakeDay(DateTime origin, int offset)
        {
            return new DayDto
            {
                Date = origin.AddDays(offset).ToIso8601DateString(),
                Entries = new[]
                {
                    new TimeDto
                    {
                        MaximumPartySize = MaitreD.Tables.First().Capacity
                    }
                }
            };
        }
    }
}
