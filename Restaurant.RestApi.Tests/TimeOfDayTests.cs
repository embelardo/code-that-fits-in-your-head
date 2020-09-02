/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public class TimeOfDayTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(25)]
        public void AttemptNegativeTimeOfDay(int hours)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new TimeOfDay(TimeSpan.FromHours(hours)));
        }
    }
}
