/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public class ReservationTests
    {
        [Property]
        public void QuantityMustBePositive(
            Guid id,
            DateTime at,
            Email email,
            NonNegativeInt i)
        {
            var invalidQuantity = -i?.Item ?? 0;
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Reservation(
                    id,
                    at,
                    email,
                    new Name("Ann da Lucia"),
                    invalidQuantity));
        }
    }
}
