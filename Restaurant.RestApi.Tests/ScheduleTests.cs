/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class ScheduleTests
    {
        [Property]
        public Property Schedule()
        {
            return Prop.ForAll(
                GenReservations
                    .SelectMany(rs => GenMaitreD(rs).Select(m => (m, rs)))
                    .ToArbitrary(),
                t => ScheduleImp(t.m, t.rs));
        }

        private static void ScheduleImp(
            MaitreD sut,
            Reservation[] reservations)
        {
            var actual = sut.Schedule(reservations);

            Assert.Equal(
                reservations.Select(r => r.At).Distinct().Count(),
                actual.Count());
            Assert.Equal(
                actual.Select(o => o.At).OrderBy(d => d),
                actual.Select(o => o.At));
            Assert.All(actual, o => AssertTables(sut.Tables, o.Value));
            Assert.All(
                actual,
                o => AssertRelevance(reservations, sut.SeatingDuration, o));
        }

        private static void AssertTables(
            IEnumerable<Table> expected,
            IEnumerable<Table> actual)
        {
            Assert.Equal(expected.Count(), actual.Count());
            Assert.Equal(
                expected.Sum(t => t.Capacity),
                actual.Sum(t => t.Capacity));
        }

        private static void AssertRelevance(
            IEnumerable<Reservation> reservations,
            TimeSpan seatingDuration,
            Occurrence<IEnumerable<Table>> occurrence)
        {
            var seating = new Seating(seatingDuration, occurrence.At);
            var expected = reservations
                .Select(r => (new Seating(seatingDuration, r.At), r))
                .Where(t => seating.Overlaps(t.Item1))
                .Select(t => t.r)
                .ToHashSet();

            var actual = occurrence.Value
                .SelectMany(t => t.Accept(new ReservationsVisitor()))
                .ToHashSet();

            Assert.True(
                expected.SetEquals(actual),
                $"Expected: {expected}; actual {actual}.");
        }

        private sealed class ReservationsVisitor :
            ITableVisitor<IEnumerable<Reservation>>
        {
            public IEnumerable<Reservation> VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return reservations;
            }

            public IEnumerable<Reservation> VisitStandard(
                int seats,
                Reservation? reservation)
            {
                if (reservation is { })
                    yield return reservation;
            }
        }

        private static Gen<Email> GenEmail =>
            from s in Arb.Default.NonWhiteSpaceString().Generator
            select new Email(s.Item);

        private static Gen<Name> GenName =>
            from s in Arb.Default.StringWithoutNullChars().Generator
            select new Name(s.Item);

        private static Gen<Reservation> GenReservation =>
            from id in Arb.Default.Guid().Generator
            from d in Arb.Default.DateTime().Generator
            from e in GenEmail
            from n in GenName
            from q in Arb.Default.PositiveInt().Generator
            select new Reservation(id, d, e, n, q.Item);

        private static Gen<Reservation[]> GenReservations
        {
            get
            {
                var normalArrayGen = GenReservation.ArrayOf();
                var adjacentReservationsGen = GenReservation.ArrayOf()
                    .SelectMany(rs => Gen
                        .Sequence(rs.Select(GenAdjacentReservations))
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
        private static Gen<Reservation[]> GenAdjacentReservations(
            Reservation reservation)
        {
            return
                from adjacent in GenReservationAdjacentTo(reservation)
                from useAdjacent in Gen.Frequency(
                    new WeightAndValue<Gen<bool>>(3, Gen.Constant(false)),
                    new WeightAndValue<Gen<bool>>(1, Gen.Constant(true)))
                let rs = useAdjacent ?
                    new[] { reservation, adjacent } :
                    new[] { reservation }
                select rs;
        }

        private static Gen<Reservation> GenReservationAdjacentTo(
            Reservation reservation)
        {
            return
                from minutes in Gen.Choose(-6 * 4, 6 * 4) // 4: quarters/h
                from r in GenReservation
                select r.WithDate(
                    reservation.At + TimeSpan.FromMinutes(minutes));
        }

        private static Gen<MaitreD> GenMaitreD(
            IEnumerable<Reservation> reservations)
        {
            return
                from seatingDuration in Gen.Choose(1, 6)
                from tables in GenTables(reservations)
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
        private static Gen<IEnumerable<Table>> GenTables(
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
