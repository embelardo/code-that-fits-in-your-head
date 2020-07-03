/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class CalendarDto
    {
        public int Year { get; set; }

        public int? Month { get; set; }

        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
        public DayDto[]? Days { get; set; }
    }
}