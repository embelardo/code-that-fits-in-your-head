/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class Gens
    {
        internal static Gen<Email> Email =>
            from s in Arb.Default.NonWhiteSpaceString().Generator
            select new Email(s.Item);

        internal static Gen<Name> Name =>
            from s in Arb.Default.StringWithoutNullChars().Generator
            select new Name(s.Item);

        internal static Gen<Reservation> Reservation =>
            from id in Arb.Default.Guid().Generator
            from d in Arb.Default.DateTime().Generator
            from e in Email
            from n in Name
            from q in Arb.Default.PositiveInt().Generator
            select new Reservation(id, d, e, n, q.Item);

        internal static Gen<Reservation[]> Reservations
        {
            get
            {
                var normalArrayGen = Reservation.ArrayOf();
                var adjacentReservationsGen = Reservation.ArrayOf()
                    .SelectMany(rs => Gen
                        .Sequence(rs.Select(AdjacentReservations))
                        .SelectMany(rss => Gen.Shuffle(
                            rss.SelectMany(rs => rs))));
                return Gen.OneOf(normalArrayGen, adjacentReservationsGen);
            }
        }

        /// <summary>
        /// Generate an adjacant reservation with a 25% chance.
        /// </summary>
        /// <param name="reservation">The candidate reservation</param>
        /// <returns>
        /// A generator of an array of reservations. The generated array is
        /// either a singleton or a pair. In 75% of the cases, the input
        /// <paramref name="reservation" /> is returned as a singleton array.
        /// In 25% of the cases, the array contains two reservations: the input
        /// reservation as well as another reservation adjacent to it.
        /// </returns>
        private static Gen<Reservation[]> AdjacentReservations(
            Reservation reservation)
        {
            return
                from adjacent in ReservationAdjacentTo(reservation)
                from useAdjacent in Gen.Frequency(
                    new WeightAndValue<Gen<bool>>(3, Gen.Constant(false)),
                    new WeightAndValue<Gen<bool>>(1, Gen.Constant(true)))
                let rs = useAdjacent ?
                    new[] { reservation, adjacent } :
                    new[] { reservation }
                select rs;
        }

        private static Gen<Reservation> ReservationAdjacentTo(
            Reservation reservation)
        {
            return
                from minutes in Gen.Choose(-6 * 4, 6 * 4) // 4: quarters/h
                from r in Reservation
                select r.WithDate(
                    reservation.At + TimeSpan.FromMinutes(minutes));
        }

        internal static Gen<MaitreD> MaitreD(
            IEnumerable<Reservation> reservations)
        {
            return
                from seatingDuration in Gen.Choose(1, 6)
                from tables in Tables(reservations)
                select new MaitreD(
                    TimeSpan.FromHours(18),
                    TimeSpan.FromHours(21),
                    TimeSpan.FromHours(seatingDuration),
                    tables);
        }

        /// <summary>
        /// Generate a table configuration that can at minimum accomodate all
        /// reservations.
        /// </summary>
        /// <param name="reservations">The reservations to accommodate</param>
        /// <returns>A generator of valid table configurations.</returns>
        private static Gen<IEnumerable<Table>> Tables(
            IEnumerable<Reservation> reservations)
        {
            // Create a table for each reservation, to ensure that all
            // reservations can be allotted a table.
            var tables = reservations.Select(r => Table.Standard(r.Quantity));
            return
                from moreTables in
                    Gen.Choose(1, 12).Select(Table.Standard).ArrayOf()
                let allTables =
                    tables.Concat(moreTables).OrderBy(t => t.Capacity)
                select allTables.AsEnumerable();
        }
    }
}
