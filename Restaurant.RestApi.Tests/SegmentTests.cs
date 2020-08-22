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
    public class SegmentTests
    {
        [Property]
        public Property Segment()
        {
            return Prop.ForAll(
                (from rs in Gens.Reservations
                 from m in Gens.MaitreD(rs)
                 from d in GenDate(rs)
                 select (m, d, rs)).ToArbitrary(),
                t => SegmentImp(t.m, t.d, t.rs));
        }

        private static void SegmentImp(
            MaitreD sut,
            DateTime date,
            Reservation[] reservations)
        {
            var actual = sut.Segment(date, reservations);
            Assert.NotEmpty(actual);
        }

        /// <summary>
        /// Generate either an unconstrained, random date, or one picked from
        /// one of the input <paramref name="reservations" />.
        /// </summary>
        /// <param name="reservations">
        /// The reservations from which a date might be picked.
        /// </param>
        /// <returns>
        /// A generator that may return a date among the supplied
        /// <paramref name="reservations" />. If so, the reservation is picked
        /// at random. If the collection of reservations is empy, a random date
        /// is returned. This may also be the case even if the reservation
        /// collection is non-empty. The chance of that is 50%.
        /// </returns>
        private static Gen<DateTime> GenDate(
            IEnumerable<Reservation> reservations)
        {
            var randomDayGen = Arb.Default.DateTime().Generator;
            if (!reservations.Any())
                return randomDayGen;

            var oneOfReservationsDayGet = Gen.Elements(reservations
                .Select(r => r.At));

            return Gen.OneOf(randomDayGen, oneOfReservationsDayGet);            
        }
    }
}
