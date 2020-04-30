/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class MaitreDTests
    {
        [Fact]
        public void Accept()
        {
            var sut = new MaitreD(new Table(TableType.Communal, 12));

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
