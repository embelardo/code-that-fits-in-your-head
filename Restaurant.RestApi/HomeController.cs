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
            links.Add(CreateReservationsLink());
            if (enableCalendar)
            {
                links.Add(CreateYearLink());
                links.Add(CreateMonthLink());
                links.Add(CreateDayLink());
            }
            return Ok(new HomeDto { Links = links.ToArray() });
        }

        private LinkDto CreateReservationsLink()
        {
            return new UrlBuilder()
                .WithAction(nameof(ReservationsController.Post))
                .WithController(nameof(ReservationsController))
                .BuildAbsolute(Url)
                .Link("urn:reservations");
        }

        private LinkDto CreateYearLink()
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new { year = DateTime.Now.Year })
                .BuildAbsolute(Url)
                .Link("urn:year");
        }

        private LinkDto CreateMonthLink()
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new
                {
                    year = DateTime.Now.Year,
                    month = DateTime.Now.Month
                })
                .BuildAbsolute(Url)
                .Link("urn:month");
        }

        private LinkDto CreateDayLink()
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new
                {
                    year = DateTime.Now.Year,
                    month = DateTime.Now.Month,
                    day = DateTime.Now.Day
                })
                .BuildAbsolute(Url)
                .Link("urn:day");
        }
    }
}
