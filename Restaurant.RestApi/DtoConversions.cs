/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public static class DtoConversions
    {
        public static ReservationDto ToDto(this Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            return new ReservationDto
            {
                Id = reservation.Id.ToString("N"),
                At = reservation.At.ToIso8601DateTimeString(),
                Email = reservation.Email.ToString(),
                Name = reservation.Name.ToString(),
                Quantity = reservation.Quantity
            };
        }
    }
}
