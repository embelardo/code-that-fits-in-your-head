/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class RestaurantTests
    {
        [Theory]
        [InlineData("Hipgnosta")]
        [InlineData("Nono")]
        [InlineData("The Vatican Cellar")]
        public async Task GetRestaurant(string name)
        {
            using var api = new SelfHostedApi();
            var client = api.CreateClient();

            var response = await client.GetRestaurant(name);

            response.EnsureSuccessStatusCode();
            var content = await response.ParseJsonContent<RestaurantDto>();
            Assert.Equal(name, content.Name);
        }

        [Theory]
        [InlineData("Hipgnosta")]
        [InlineData("Nono")]
        [InlineData("The Vatican Cellar")]
        public async Task RestaurantReturnsCorrectLinks(string name)
        {
            using var api = new SelfHostedApi();
            var client = api.CreateClient();

            var response = await client.GetRestaurant(name);

            var expected = new HashSet<string?>(new[]
            {
                "urn:reservations",
                "urn:year",
                "urn:month",
                "urn:day"
            });
            var actual = await response.ParseJsonContent<RestaurantDto>();
            var actualRels = actual.Links.Select(l => l.Rel).ToHashSet();
            Assert.Superset(expected, actualRels);
            Assert.All(actual.Links, AssertHrefAbsoluteUrl);
        }

        private static void AssertHrefAbsoluteUrl(LinkDto dto)
        {
            Assert.True(
                Uri.TryCreate(dto.Href, UriKind.Absolute, out var _),
                $"Not an absolute URL: {dto.Href}.");
        }
    }
}
