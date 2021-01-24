/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "DTO.")]
    public sealed class TimeDto
    {
        public string? Time { get; set; }
        public int? MaximumPartySize { get; set; }
        public ReservationDto[]? Reservations { get; set; }
    }
}