/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public static class ReservationEnvy
    {
        // This method is useful for testing, but seems misplaced in the
        // production code. Why would the system want to change the ID of a
        // reservation?
        // If it turns out that there's a valid reason, then consider moving
        // this function to the Reservation class.
        public static Reservation WithId(
            this Reservation reservation,
            Guid newId)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            return new Reservation(
                newId,
                reservation.At,
                reservation.Email,
                reservation.Name,
                reservation.Quantity);
        }

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

        public static Reservation OneHourLater(this Reservation reservation)
        {
            return reservation.AddDate(TimeSpan.FromHours(1));
        }

        public static Reservation TheDayAfter(this Reservation reservation)
        {
            return reservation.AddDate(TimeSpan.FromDays(1));
        }
    }
}
