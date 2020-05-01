/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class MaitreDTests
    {
        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class AcceptTestCases : TheoryData<IEnumerable<int>, IEnumerable<int>>
        {
            public AcceptTestCases()
            {
                Add(new[]    { 12 }, Array.Empty<int>());
                Add(new[] { 8, 11 }, Array.Empty<int>());
                Add(new[] { 2, 11 }, new[] { 2 });
            }
        }

        [Theory, ClassData(typeof(AcceptTestCases))]
        public void Accept(int[] tableSeats, int[] reservedSeats)
        {
            var tables = tableSeats.Select(Table.Communal);
            var sut = new MaitreD(tables);

            var rs = reservedSeats.Select(Some.Reservation.WithQuantity);
            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(rs, r);

            Assert.True(actual);
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class RejectTestCases :
            TheoryData<IEnumerable<Table>, IEnumerable<Reservation>>
        {
            public RejectTestCases()
            {
                Add(new[] { Table.Communal(6), Table.Communal(6) },
                    Array.Empty<Reservation>());
                Add(new[] { Table.Standard(12) },
                    new[] { Some.Reservation.WithQuantity(1) });
            }
        }

        [Theory, ClassData(typeof(RejectTestCases))]
        public void Reject(
            IEnumerable<Table> tables,
            IEnumerable<Reservation> reservations)
        {
            var sut = new MaitreD(tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(reservations, r);

            Assert.False(actual);
        }
    }
}
