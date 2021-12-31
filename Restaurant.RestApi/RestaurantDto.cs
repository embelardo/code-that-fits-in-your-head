/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class RestaurantDto
    {
        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
        public LinkDto[]? Links { get; set; }
        public string? Name { get; set; }
    }
}