/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
    public class CalendarDto
    {
        public LinkDto[]? Links { get; set; }

        public int Year { get; set; }

        public int? Month { get; set; }

        public int? Day { get; set; }

        public DayDto[]? Days { get; set; }
    }
}