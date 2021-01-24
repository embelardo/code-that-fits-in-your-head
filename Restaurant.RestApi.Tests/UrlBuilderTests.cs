/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class UrlBuilderTests
    {
        [Theory]
        [InlineData("Home")]
        [InlineData("Calendar")]
        [InlineData("Reservations")]
        public void WithControllerHandlesSuffix(string name)
        {
            var sut = new UrlBuilder();

            var actual = sut.WithController(name + "Controller");

            var expected = sut.WithController(name);
            Assert.Equal(expected, actual);
        }
    }
}
