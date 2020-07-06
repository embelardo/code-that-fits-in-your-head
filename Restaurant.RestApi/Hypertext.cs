/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    internal static class Hypertext
    {
        internal static LinkDto Link(this Uri uri, string rel)
        {
            return new LinkDto { Rel = rel, Href = uri.ToString() };
        }

        internal static LinkDto LinkToReservations(this IUrlHelper url)
        {
            return new UrlBuilder()
                .WithAction(nameof(ReservationsController.Post))
                .WithController(nameof(ReservationsController))
                .BuildAbsolute(url)
                .Link("urn:reservations");
        }

        internal static LinkDto LinkToYear(this IUrlHelper url, int year)
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new { year })
                .BuildAbsolute(url)
                .Link("urn:year");
        }

        internal static LinkDto LinkToMonth(
            this IUrlHelper url,
            int year,
            int month)
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new { year, month })
                .BuildAbsolute(url)
                .Link("urn:month");
        }

        internal static LinkDto LinkToDay(
            this IUrlHelper url,
            int year,
            int month,
            int day)
        {
            return new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController))
                .WithValues(new { year, month, day })
                .BuildAbsolute(url)
                .Link("urn:day");
        }
    }
}
