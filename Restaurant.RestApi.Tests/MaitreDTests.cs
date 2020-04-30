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
        [InlineData(new[] { 12 })]
        public void Accept(int[] tableSeats)
        {
            var tables =
                tableSeats.Select(s => new Table(TableType.Communal, s));
            var sut = new MaitreD(tables);

            var r = new Reservation(
                new DateTime(2022, 4, 1, 20, 15, 0),
                "x@example.net",
                "",
                11);
            var actual = sut.WillAccept(Array.Empty<Reservation>(), r);

            Assert.True(actual);
        }
    }
}
