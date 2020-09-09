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

        internal static LinkDto LinkToYear(
            this IUrlHelper url,
            int restaurantId,
            int year)
        {
            return url.LinkToYear(restaurantId, year, "urn:year");
        }

        internal static LinkDto LinkToYear(
            this IUrlHelper url,
            int restaurantId,
            int year,
            string rel)
        {
            return calendar
                .WithAction(nameof(CalendarController.GetYear))
                .WithValues(new { restaurantId, year })
                .BuildAbsolute(url)
                .Link(rel);
        }

        internal static LinkDto LinkToMonth(
            this IUrlHelper url,
            int restaurantId,
            int year,
            int month)
        {
            return url.LinkToMonth(restaurantId, year, month, "urn:month");
        }

        internal static LinkDto LinkToMonth(
            this IUrlHelper url,
            int restaurantId,
            int year,
            int month,
            string rel)
        {
            return calendar
                .WithAction(nameof(CalendarController.GetMonth))
                .WithValues(new { restaurantId, year, month })
                .BuildAbsolute(url)
                .Link(rel);
        }

        internal static LinkDto LinkToDay(
            this IUrlHelper url,
            int restaurantId,
            int year,
            int month,
            int day)
        {
            return url.LinkToDay(restaurantId, year, month, day, "urn:day");
        }

        internal static LinkDto LinkToDay(
            this IUrlHelper url,
            int restaurantId,
            int year,
            int month,
            int day,
            string rel)
        {
            return calendar
                .WithAction(nameof(CalendarController.GetDay))
                .WithValues(new { restaurantId, year, month, day })
                .BuildAbsolute(url)
                .Link(rel);
        }

        internal static LinkDto LinkToPeriod(
            this IUrlHelper url,
            int restaurantId,
            IPeriod period,
            string rel)
        {
            var (action, values) = period.Accept(new ValuesVisitor(restaurantId));
            return calendar
                .WithAction(action)
                .WithValues(values)
                .BuildAbsolute(url)
                .Link(rel);
        }

        private class ValuesVisitor : IPeriodVisitor<(string, object)>
        {
            private readonly int restaurantId;

            public ValuesVisitor(int restaurantId)
            {
                this.restaurantId = restaurantId;
            }

            public (string, object) VisitYear(int year)
            {
                return ("GetYear", new { restaurantId, year });
            }

            public (string, object) VisitMonth(int year, int month)
            {
                return ("GetMonth", new { restaurantId, year, month });
            }

            public (string, object) VisitDay(int year, int month, int day)
            {
                return ("GetDay", new { restaurantId, year, month, day });
            }
        }

        internal static LinkDto LinkToSchedule(
            this IUrlHelper url,
            int restaurantId,
            int year,
            int month,
            int day)
        {
            return schedule
                .WithValues(new { restaurantId, year, month, day })
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
