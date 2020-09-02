/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public class MaitreDScheduleTests
    {
        [Property]
        public Property Schedule()
        {
            return Prop.ForAll(
                (from rs in Gens.Reservations
                 from  m in Gens.MaitreD(rs)
                 select (m, rs)).ToArbitrary(),
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
                .SelectMany(t => t.Accept(ReservationsVisitor.Instance))
                .ToHashSet();

            Assert.True(
                expected.SetEquals(actual),
                $"Expected: {expected}; actual {actual}.");
        }
    }
}
