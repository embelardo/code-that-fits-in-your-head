/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1812: Avoid uninstantiated internal classes",
        Justification = "This class is instantiated via Reflection.")]
    internal class LinksFilter : IAsyncActionFilter
    {
        public IUrlHelperFactory UrlHelperFactory { get; }

        public LinksFilter(IUrlHelperFactory urlHelperFactory)
        {
            UrlHelperFactory = urlHelperFactory;
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
                case CalendarDto calendarDto:
                    AddLinks(calendarDto, url);
                    break;
                default:
                    break;
            }
        }

        private static void AddLinks(CalendarDto dto, IUrlHelper url)
        {
            var period = dto.ToPeriod();
            var previous = period.Accept(new PreviousPeriodVisitor());
            var next = period.Accept(new NextPeriodVisitor());

            dto.Links = new[]
            {
                url.LinkToPeriod(previous, "previous"),
                url.LinkToPeriod(next, "next")
            };

            if (dto.Days is { })
                foreach (var day in dto.Days)
                    AddLinks(day, url);
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

        private static void AddLinks(DayDto dto, IUrlHelper url)
        {
            if (DateTime.TryParse(dto.Date, out var date))
                dto.Links = new[]
                {
                    url.LinkToDay(date.Year, date.Month, date.Day)
                };
        }
    }
}
