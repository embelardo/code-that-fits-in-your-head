/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
    public sealed class CalendarDto
    {
        public LinkDto[]? Links { get; set; }

        public string? Name { get; set; }

        public int Year { get; set; }

        public int? Month { get; set; }

        public int? Day { get; set; }

        public DayDto[]? Days { get; set; }

        internal IPeriod ToPeriod()
        {
            if (Month is null)
                return Period.Year(Year);
            else if (Day is null)
                return Period.Month(Year, Month.Value);
            else
                return Period.Day(Year, Month.Value, Day.Value);
        }
    }
}