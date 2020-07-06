/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly bool enableCalendar;

        public HomeController(CalendarFlag calendarFlag)
        {
            if (calendarFlag is null)
                throw new ArgumentNullException(nameof(calendarFlag));

            enableCalendar = calendarFlag.Enabled;
        }

        public IActionResult Get()
        {
            var links = new List<LinkDto>();
            links.Add(Url.LinkToReservations());
            if (enableCalendar)
            {
                links.Add(Url.LinkToYear(DateTime.Now.Year));
                links.Add(
                    Url.LinkToMonth(DateTime.Now.Year, DateTime.Now.Month));
                links.Add(Url.LinkToDay(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day));
            }
            return Ok(new HomeDto { Links = links.ToArray() });
        }
    }
}
