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
        [Theory]
        [InlineData(new[]    { 12 },  new int[0])]
        [InlineData(new[] { 8, 11 },  new int[0])]
        [InlineData(new[] { 2, 11 }, new[] { 2 })]
        public void Accept(int[] tableSeats, int[] reservedSeats)
        {
            var tables =
                tableSeats.Select(s => new Table(TableType.Communal, s));
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
        private class RejectTestCases : TheoryData<IEnumerable<int>>
        {
            public RejectTestCases()
            {
                Add(new[] { 6, 6 });
            }
        }

        [Theory, ClassData(typeof(RejectTestCases))]
        public void Reject(int[] tableSeats)
        {
            var tables =
                tableSeats.Select(s => new Table(TableType.Communal, s));
            var sut = new MaitreD(tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(Array.Empty<Reservation>(), r);

            Assert.False(actual);
        }
    }
}
