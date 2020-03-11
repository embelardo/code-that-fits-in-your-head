﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurant.RestApi
{
    public class ReservationDto
    {
        public string? At { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
    }
}