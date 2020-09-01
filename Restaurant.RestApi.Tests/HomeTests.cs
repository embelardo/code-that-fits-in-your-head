/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
            using var service = new SelfHostedService();
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

        [Fact]
        public async Task HomeReturnsCorrectLinks()
        {
            using var service = new SelfHostedService();
            var client = service.CreateClient();

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
            using var service = new SelfHostedService();
            var client = service.CreateClient();

            var response =
                await client.GetAsync(new Uri("", UriKind.Relative));

            var dto = await response.ParseJsonContent<HomeDto>();
            Assert.NotEmpty(dto.Restaurants);
        }
    }
}
