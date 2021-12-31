/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal sealed class ReservationDtoComparer :
        IEqualityComparer<ReservationDto>
    {
        public bool Equals(ReservationDto? x, ReservationDto? y)
        {
            var datesAreEqual = Equals(x?.At, y?.At);
            if (!datesAreEqual &&
                DateTime.TryParse(x?.At, out var xDate) &&
                DateTime.TryParse(y?.At, out var yDate))
                datesAreEqual = Equals(xDate, yDate);

            return datesAreEqual
                && Equals(x?.Email, y?.Email)
                && Equals(x?.Name, y?.Name)
                && Equals(x?.Quantity, y?.Quantity);
        }

        public int GetHashCode(ReservationDto obj)
        {
            var dateHash =
                obj.At?.GetHashCode(StringComparison.InvariantCulture);
            if (DateTime.TryParse(obj.At, out var dt))
                dateHash = dt.GetHashCode();

            return HashCode.Combine(
                dateHash,
                obj.Email,
                obj.Name,
                obj.Quantity);
        }
    }
}