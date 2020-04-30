/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
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

            var rs = reservedSeats.Select(s => new Reservation(
                new DateTime(2022, 4, 1, 20, 15, 0),
                "x@example.net",
                "",
                s));
            var r = new Reservation(
                new DateTime(2022, 4, 1, 20, 15, 0),
                "x@example.net",
                "",
                11);
            var actual = sut.WillAccept(rs, r);

            Assert.True(actual);
        }

        [Fact]
        public void Reject()
        {
            var sut = new MaitreD(
                new Table(TableType.Communal, 6),
                new Table(TableType.Communal, 6));

            var r = new Reservation(
                new DateTime(2022, 4, 1, 20, 15, 0),
                "x@example.com",
                "",
                11);
            var actual = sut.WillAccept(Array.Empty<Reservation>(), r);

            Assert.False(actual);
        }
    }
}
