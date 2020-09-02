/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public class ReservationTests
    {
        [Theory]
        [InlineData( 0)]
        [InlineData(-1)]
        public void QuantityMustBePositive(int invalidQantity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Reservation(
                    Guid.NewGuid(),
                    new DateTime(2024, 8, 19, 11, 30, 0),
                    new Email("mail@example.com"),
                    new Name("Marie Ilsøe"),
                    invalidQantity));
        }
    }
}
