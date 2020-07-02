/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class CalendarTests
    {
        [Fact]
        public async Task GetCurrentYear()
        {
            using var service = new RestaurantApiFactory();

            var response = await service.GetCurrentYear();

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
        }
    }
}
