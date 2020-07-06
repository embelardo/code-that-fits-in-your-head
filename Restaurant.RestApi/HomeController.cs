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
                var now = DateTime.Now;
                links.Add(Url.LinkToYear(now.Year));
                links.Add(Url.LinkToMonth(now.Year, now.Month));
                links.Add(Url.LinkToDay(now.Year, now.Month, now.Day));
            }
            return Ok(new HomeDto { Links = links.ToArray() });
        }
    }
}
