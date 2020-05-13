/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class ReservationDto
    {
        public string? Id { get; set; }
        public string? At { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }

        internal Reservation? Validate(Guid fallbackId)
        {
            if (!DateTime.TryParse(At, out var d))
                return null;
            if (Email is null)
                return null;
            if (Quantity < 1)
                return null;

            if (!Guid.TryParse(Id, out var id))
                id = fallbackId;
            return new Reservation(id, d, Email, Name ?? "", Quantity);
        }
    }
}