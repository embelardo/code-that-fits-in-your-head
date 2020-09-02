/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public class ReservationDto
    {
        public string? Id { get; set; }
        public string? At { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }

        internal Guid? ParseId()
        {
            if (Guid.TryParse(Id, out var id))
                return id;
            return null;
        }

        internal Reservation? Validate(Guid id)
        {
            if (!DateTime.TryParse(At, out var d))
                return null;
            if (Email is null)
                return null;
            if (Quantity < 1)
                return null;

            return new Reservation(id,
                d,
                new Email(Email),
                new Name(Name ?? ""),
                Quantity);
        }
    }
}