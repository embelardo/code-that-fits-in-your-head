/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1812: Avoid uninstantiated internal classes",
        Justification = "This class is instantiated via Reflection.")]
    internal sealed class LinksFilter : IAsyncActionFilter
    {
        public IUrlHelperFactory UrlHelperFactory { get; }
        public IRestaurantDatabase Database { get; }
        public IClock Clock { get; }

        public LinksFilter(
            IClock clock,
            IUrlHelperFactory urlHelperFactory,
            IRestaurantDatabase database)
        {
            UrlHelperFactory = urlHelperFactory;
            Database = database;
            Clock = clock;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var ctxAfter = await next().ConfigureAwait(false);
            if (!(ctxAfter.Result is OkObjectResult ok))
                return;

            var url = UrlHelperFactory.GetUrlHelper(ctxAfter);
            switch (ok.Value)
            {
                case HomeDto homeDto:
                    await AddLinks(homeDto, url).ConfigureAwait(false);
                    break;
                case CalendarDto calendarDto:
                    await AddLinks(calendarDto, url).ConfigureAwait(false);
                    break;
                case RestaurantDto restaurantDto:
                    await AddLinks(restaurantDto, url).ConfigureAwait(false);
                    break;
                default:
                    break;
            }
        }

        private async Task AddLinks(HomeDto dto, IUrlHelper url)
        {
            var now = Clock.GetCurrentDateTime();
            dto.Links = new[]
            {
                url.LinkToReservations(Grandfather.Id),
                url.LinkToYear(Grandfather.Id, now.Year),
                url.LinkToMonth(Grandfather.Id, now.Year, now.Month),
                url.LinkToDay(Grandfather.Id, now.Year, now.Month, now.Day)
            };

            if (dto.Restaurants is { })
                foreach (var restaurant in dto.Restaurants)
                    await AddLinks(restaurant, url).ConfigureAwait(false);
        }

        private async Task AddLinks(RestaurantDto restaurant, IUrlHelper url)
        {
            if (restaurant.Name is null)
                return;

            var r = await Database.GetRestaurant(restaurant.Name)
                .ConfigureAwait(false);
            if (r is null)
                return;

            var now = Clock.GetCurrentDateTime();

            restaurant.Links = new[]
            {
                url.LinkToRestaurant(r.Id),
                url.LinkToReservations(r.Id),
                url.LinkToYear(r.Id, now.Year),
                url.LinkToMonth(r.Id, now.Year, now.Month),
                url.LinkToDay(r.Id, now.Year, now.Month, now.Day)
            };
        }

        private async Task AddLinks(CalendarDto dto, IUrlHelper url)
        {
            if (dto.Name is null)
                return;

            var r = await Database.GetRestaurant(dto.Name)
                .ConfigureAwait(false);
            if (r is null)
                return;

            var period = dto.ToPeriod();
            var previous = period.Accept(new PreviousPeriodVisitor());
            var next = period.Accept(new NextPeriodVisitor());

            dto.Links = new[]
            {
                url.LinkToPeriod(r.Id, previous, "previous"),
                url.LinkToPeriod(r.Id, next, "next")
            };

            if (dto.Days is { })
                foreach (var day in dto.Days)
                    AddLinks(r.Id, day, url);
        }

        private class PreviousPeriodVisitor : IPeriodVisitor<IPeriod>
        {
            public IPeriod VisitYear(int year)
            {
                var date = new DateTime(year, 1, 1);
                var previous = date.AddYears(-1);
                return Period.Year(previous.Year);
            }

            public IPeriod VisitMonth(int year, int month)
            {
                var date = new DateTime(year, month, 1);
                var previous = date.AddMonths(-1);
                return Period.Month(previous.Year, previous.Month);
            }

            public IPeriod VisitDay(int year, int month, int day)
            {
                var date = new DateTime(year, month, day);
                var previous = date.AddDays(-1);
                return Period.Day(previous.Year, previous.Month, previous.Day);
            }
        }

        private class NextPeriodVisitor : IPeriodVisitor<IPeriod>
        {
            public IPeriod VisitYear(int year)
            {
                var date = new DateTime(year, 1, 1);
                var next = date.AddYears(1);
                return Period.Year(next.Year);
            }

            public IPeriod VisitMonth(int year, int month)
            {
                var date = new DateTime(year, month, 1);
                var next = date.AddMonths(1);
                return Period.Month(next.Year, next.Month);
            }

            public IPeriod VisitDay(int year, int month, int day)
            {
                var date = new DateTime(year, month, day);
                var next = date.AddDays(1);
                return Period.Day(next.Year, next.Month, next.Day);
            }
        }

        private static void AddLinks(
            int restaurantId,
            DayDto dto,
            IUrlHelper url)
        {
            if (DateTime.TryParse(dto.Date, out var date))
                dto.Links = new[]
                {
                    url.LinkToYear(restaurantId, date.Year),
                    url.LinkToMonth(restaurantId, date.Year, date.Month),
                    url.LinkToDay(
                        restaurantId,
                        date.Year,
                        date.Month,
                        date.Day),
                    url.LinkToSchedule(
                        restaurantId,
                        date.Year,
                        date.Month,
                        date.Day)
                };
        }
    }
}
