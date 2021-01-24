/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class HomeTests
    {
        [Fact]
        public async Task HomeReturnsJson()
        {
            using var api = new LegacyApi();
            var client = api.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Accept.ParseAdd("application/json");
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            Assert.Equal(
                "application/json",
                response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task HomeReturnsCorrectLinks()
        {
            using var api = new LegacyApi();
            var client = api.CreateClient();

            var response =
                await client.GetAsync(new Uri("", UriKind.Relative));

            var expected = new HashSet<string?>(new[]
            {
                "urn:reservations",
                "urn:year",
                "urn:month",
                "urn:day"
            });
            var actual = await response.ParseJsonContent<HomeDto>();
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

        [Fact]
        public async Task HomeReturnsRestaurants()
        {
            using var api = new LegacyApi();
            var client = api.CreateClient();

            var response =
                await client.GetAsync(new Uri("", UriKind.Relative));

            var dto = await response.ParseJsonContent<HomeDto>();
            Assert.NotEmpty(dto.Restaurants);
            Assert.All(dto.Restaurants, r => Assert.NotEmpty(r.Name));
            Assert.All(
                dto.Restaurants,
                r => Assert.Contains(r.Links, l => l.Rel == "urn:restaurant"));
            Assert.All(
                dto.Restaurants.SelectMany(r => r.Links),
                AssertHrefAbsoluteUrl);
        }
    }
}
