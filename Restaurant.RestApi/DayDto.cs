/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class DayDto
    {
        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
        public LinkDto[]? Links { get; set; }
        public string? Date { get; set; }
        public int MaximumPartySize { get; set; }
    }
}