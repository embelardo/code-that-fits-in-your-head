/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class HomeDto
    {
        [SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "DTO.")]
        public LinkDto[]? Links { get; set; }
    }
}