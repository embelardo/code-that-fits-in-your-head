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
            var href = new UrlBuilder()
                .WithAction(nameof(ReservationsController.Post))
                .WithController(nameof(ReservationsController))
                .BuildAbsolute(Url);

            return new LinkDto
            {
                Rel = "urn:reservations",
                Href = href.ToString()
            };
        }

        private LinkDto CreateYearLink()
        {
            var href = new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new { year = DateTime.Now.Year })
                .BuildAbsolute(Url);

            return new LinkDto
            {
                Rel = "urn:year",
                Href = href.ToString()
            };
        }

        private LinkDto CreateMonthLink()
        {
            var href = new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new
                {
                    year = DateTime.Now.Year,
                    month = DateTime.Now.Month
                })
                .BuildAbsolute(Url);

            return new LinkDto
            {
                Rel = "urn:month",
                Href = href.ToString()
            };
        }

        private LinkDto CreateDayLink()
        {
            var href = new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new
                {
                    year = DateTime.Now.Year,
                    month = DateTime.Now.Month,
                    day = DateTime.Now.Day
                })
                .BuildAbsolute(Url);

            return new LinkDto
            {
                Rel = "urn:day",
                Href = href.ToString()
            };
        }
    }
}
