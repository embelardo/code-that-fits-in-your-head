/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("calendar")]
    public class CalendarController
    {
        [HttpGet("{year}")]
        public ActionResult Get(int year)
        {
            var daysInYear = new GregorianCalendar().GetDaysInYear(year);
            var days = Enumerable.Repeat(new DayDto(), daysInYear).ToArray();
            return new OkObjectResult(
                new CalendarDto { Year = year, Days = days });
        }
    }
}
