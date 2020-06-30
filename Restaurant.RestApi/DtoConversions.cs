/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public static class DtoConversions
    {
        [SuppressMessage(
            "Globalization",
            "CA1305:Specify IFormatProvider",
            Justification = "ToString(\"o\") is already culture-neutral.")]
        public static ReservationDto ToDto(this Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            return new ReservationDto
            {
                Id = reservation.Id.ToString("N"),
                At = reservation.At.ToString("o"),
                Email = reservation.Email.ToString(),
                Name = reservation.Name.ToString(),
                Quantity = reservation.Quantity
            };
        }
    }
}
