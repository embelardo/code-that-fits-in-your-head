/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "DTO.")]
    public class HomeDto
    {
        public LinkDto[]? Links { get; set; }
        public RestaurantDto[]? Restaurants { get; set; }
    }
}