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
        private readonly static UrlBuilder reservations =
            new UrlBuilder()
                .WithAction(nameof(ReservationsController.Post))
                .WithController(nameof(ReservationsController));
        private readonly static UrlBuilder calendar =
            new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController));

        internal static LinkDto Link(this Uri uri, string rel)
        {
            return new LinkDto { Rel = rel, Href = uri.ToString() };
        }

        internal static LinkDto LinkToReservations(this IUrlHelper url)
        {
            return reservations.BuildAbsolute(url).Link("urn:reservations");
        }

        internal static LinkDto LinkToYear(this IUrlHelper url, int year)
        {
            return url.LinkToYear(year, "urn:year");
        }

        internal static LinkDto LinkToYear(
            this IUrlHelper url,
            int year,
            string rel)
        {
            return calendar
                .WithValues(new { year })
                .BuildAbsolute(url)
                .Link(rel);
        }

        internal static LinkDto LinkToMonth(
            this IUrlHelper url,
            int year,
            int month)
        {
            return url.LinkToMonth(year, month, "urn:month");
        }

        internal static LinkDto LinkToMonth(
            this IUrlHelper url,
            int year,
            int month,
            string rel)
        {
            return calendar
                .WithValues(new { year, month })
                .BuildAbsolute(url)
                .Link(rel);
        }

        internal static LinkDto LinkToDay(
            this IUrlHelper url,
            int year,
            int month,
            int day)
        {
            return url.LinkToDay(year, month, day, "urn:day");
        }

        internal static LinkDto LinkToDay(
            this IUrlHelper url,
            int year,
            int month,
            int day,
            string rel)
        {
            return calendar
                .WithValues(new { year, month, day })
                .BuildAbsolute(url)
                .Link(rel);
        }
    }
}
