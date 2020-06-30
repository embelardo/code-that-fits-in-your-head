/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class HomeTests
    {
        [Fact]
        [SuppressMessage(
            "Usage", "CA2234:Pass system uri objects instead of strings",
            Justification = "URL isn't passed as variable, but as literal.")]
        public async Task HomeReturnsJson()
        {
            using var service = new RestaurantApiFactory();
            var client = service.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Accept.ParseAdd("application/json");
            var response = await client.SendAsync(request);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            Assert.Equal(
                "application/json",
                response.Content.Headers.ContentType?.MediaType);
        }
    }
}
