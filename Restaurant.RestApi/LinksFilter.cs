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
            if (dto.Month is null)
                dto.Links = new[]
                {
                    new LinkDto
                    {
                        Rel = "previous",
                        Href = url.LinkToYear(dto.Year - 1).Href
                    },
                    new LinkDto
                    {
                        Rel = "next",
                        Href = url.LinkToYear(dto.Year + 1).Href
                    }
                };
            else if (dto.Day is null)
            {
                var firstDay = new DateTime(dto.Year, dto.Month.Value, 1);
                var previousMonth = firstDay.AddMonths(-1);
                var nextMonth = firstDay.AddMonths(1);
                dto.Links = new[]
                {
                    new LinkDto
                    {
                        Rel = "previous",
                        Href = url
                            .LinkToMonth(previousMonth.Year, previousMonth.Month)
                            .Href
                    },
                    new LinkDto
                    {
                        Rel = "next",
                        Href = url
                            .LinkToMonth(nextMonth.Year, nextMonth.Month)
                            .Href
                    }
                };
            }
            else
                dto.Links = new[]
                {
                    new LinkDto
                    {
                        Rel = "previous",
                        Href = url
                            .LinkToDay(
                                dto.Year,
                                dto.Month.Value,
                                dto.Day.Value - 1)
                            .Href
                    },
                    new LinkDto
                    {
                        Rel = "next",
                        Href = url
                            .LinkToDay(
                                dto.Year,
                                dto.Month.Value,
                                dto.Day.Value + 1)
                            .Href
                    }
                };
        }
    }
}
