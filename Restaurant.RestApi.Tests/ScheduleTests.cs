/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
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
                GenReservation.ArrayOf().ToArbitrary(),
                ScheduleImp);
        }

        private static void ScheduleImp(Reservation[] reservations)
        {
            // Create a table for each reservation, to ensure that all
            // reservations can be allotted a table.
            var tables = reservations.Select(r => Table.Standard(r.Quantity));
            var sut = new MaitreD(
                TimeSpan.FromHours(18),
                TimeSpan.FromHours(21),
                TimeSpan.FromHours(6),
                tables);

            var actual = sut.Schedule(reservations);

            Assert.Equal(
                reservations.Select(r => r.At).Distinct().Count(),
                actual.Count());
            Assert.Equal(
                actual.Select(o => o.At).OrderBy(d => d),
                actual.Select(o => o.At));
            Assert.All(actual, o => AssertTables(tables, o.Value));
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
    }
}
