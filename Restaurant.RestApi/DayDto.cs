/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "DTO.")]
    public class DayDto
    {
        public LinkDto[]? Links { get; set; }
        public string? Date { get; set; }
        public TimeDto[]? Entries { get; set; }
    }
}