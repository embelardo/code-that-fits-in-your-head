/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class RestaurantTests
    {
        [Theory]
        [InlineData("Hipgnosta")]
        [InlineData("Nono")]
        [InlineData("The Vatican Cellar")]
        public async Task GetRestaurant(string name)
        {
            using var service = new SelfHostedService();

            var response = await service.GetRestaurant(name);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var content = await response.ParseJsonContent<RestaurantDto>();
            Assert.Equal(name, content.Name);
        }
    }
}
