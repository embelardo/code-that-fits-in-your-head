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
            var controllerName = nameof(ReservationsController);
            var controller = controllerName.Remove(
                controllerName.LastIndexOf(
                    "Controller",
                    StringComparison.Ordinal));

            var href = Url.Action(
                nameof(ReservationsController.Post),
                controller,
                null,
                Url.ActionContext.HttpContext.Request.Scheme,
                Url.ActionContext.HttpContext.Request.Host.ToUriComponent());

            return new LinkDto
            {
                Rel = "urn:reservations",
                Href = href
            };
        }

        private LinkDto CreateYearLink()
        {
            var controllerName = nameof(CalendarController);
            var controller = controllerName.Remove(
                controllerName.LastIndexOf(
                    "Controller",
                    StringComparison.Ordinal));

            var href = Url.Action(
                nameof(CalendarController.Get),
                controller,
                new { year = DateTime.Now.Year },
                Url.ActionContext.HttpContext.Request.Scheme,
                Url.ActionContext.HttpContext.Request.Host.ToUriComponent());

            return new LinkDto
            {
                Rel = "urn:year",
                Href = href
            };
        }

        private LinkDto CreateMonthLink()
        {
            var controllerName = nameof(CalendarController);
            var controller = controllerName.Remove(
                controllerName.LastIndexOf(
                    "Controller",
                    StringComparison.Ordinal));

            var href = Url.Action(
                nameof(CalendarController.Get),
                controller,
                new { year = DateTime.Now.Year, month = DateTime.Now.Month },
                Url.ActionContext.HttpContext.Request.Scheme,
                Url.ActionContext.HttpContext.Request.Host.ToUriComponent());

            return new LinkDto
            {
                Rel = "urn:month",
                Href = href
            };
        }

        private LinkDto CreateDayLink()
        {
            var controllerName = nameof(CalendarController);
            var controller = controllerName.Remove(
                controllerName.LastIndexOf(
                    "Controller",
                    StringComparison.Ordinal));

            var href = Url.Action(
                nameof(CalendarController.Get),
                controller,
                new
                {
                    year = DateTime.Now.Year,
                    month = DateTime.Now.Month,
                    day = DateTime.Now.Day
                },
                Url.ActionContext.HttpContext.Request.Scheme,
                Url.ActionContext.HttpContext.Request.Host.ToUriComponent());

            return new LinkDto
            {
                Rel = "urn:day",
                Href = href
            };
        }
    }
}
