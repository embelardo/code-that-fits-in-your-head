/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public static class Hypertext
    {
        private readonly static UrlBuilder reservations =
            new UrlBuilder()
                .WithAction(nameof(ReservationsController.Post))
                .WithController(nameof(ReservationsController));
        private readonly static UrlBuilder restaurants =
            new UrlBuilder()
                .WithAction(nameof(RestaurantsController.Get))
                .WithController(nameof(RestaurantsController));
        private readonly static UrlBuilder calendar =
            new UrlBuilder()
                .WithAction(nameof(CalendarController.Get))
                .WithController(nameof(CalendarController));
        private readonly static UrlBuilder schedule =
            new UrlBuilder()
                .WithAction(nameof(ScheduleController.Get))
                .WithController(nameof(ScheduleController));

        internal static LinkDto Link(this Uri uri, string rel)
        {
            return new LinkDto { Rel = rel, Href = uri.ToString() };
        }

        internal static LinkDto LinkToReservations(
            this IUrlHelper url,
            int restaurantId)
        {
            return reservations
                .WithValues(new { restaurantId })
                .BuildAbsolute(url)
                .Link("urn:reservations");
        }

        internal static LinkDto LinkToRestaurant(this IUrlHelper url, int id)
        {
            return restaurants
                .WithValues(new { id })
                .BuildAbsolute(url)
                .Link("urn:restaurant");
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

        internal static LinkDto LinkToPeriod(
            this IUrlHelper url,
            IPeriod period,
            string rel)
        {
            var values = period.Accept(new ValuesVisitor());
            return calendar
                .WithValues(values)
                .BuildAbsolute(url)
                .Link(rel);
        }

        private class ValuesVisitor : IPeriodVisitor<object>
        {
            public object VisitYear(int year)
            {
                return new { year };
            }

            public object VisitMonth(int year, int month)
            {
                return new { year, month };
            }

            public object VisitDay(int year, int month, int day)
            {
                return new { year, month, day };
            }
        }

        internal static LinkDto LinkToSchedule(
            this IUrlHelper url,
            int year,
            int month,
            int day)
        {
            return schedule
                .WithValues(new { year, month, day })
                .BuildAbsolute(url)
                .Link("urn:schedule");
        }

        public static Uri FindAddress(
            this IEnumerable<LinkDto>? links,
            string rel)
        {
            var address = links.Single(l => l.Rel == rel).Href;
            if (address is null)
                throw new InvalidOperationException(
                    $"Address for relationship type \"{rel}\" not found.");

            return new Uri(address);
        }
    }
}
