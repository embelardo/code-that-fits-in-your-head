/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public static class ReservationEnvy
    {
        public static Reservation AddDate(
            this Reservation reservation,
            TimeSpan timeSpan)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            return reservation.WithDate(reservation.At.Add(timeSpan));
        }

        public static Reservation OneHourBefore(this Reservation reservation)
        {
            return reservation.AddDate(TimeSpan.FromHours(-1));
        }

        public static Reservation TheDayBefore(this Reservation reservation)
        {
            return reservation.AddDate(TimeSpan.FromDays(-1));
        }

        public static Reservation TheDayAfter(this Reservation reservation)
        {
            return reservation.AddDate(TimeSpan.FromDays(1));
        }
    }
}
