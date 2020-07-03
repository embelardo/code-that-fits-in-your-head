/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class RestaurantApiFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservationsRepository>();
                services.AddSingleton<IReservationsRepository>(
                    new FakeDatabase());

                services.RemoveAll<CalendarFlag>();
                services.AddSingleton(new CalendarFlag(true));
            });
        }

        [SuppressMessage(
            "Usage",
            "CA2234:Pass system uri objects instead of strings",
            Justification = "URL isn't passed as variable, but as literal.")]
        public async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PostAsync("reservations", content);
        }

        public async Task<HttpResponseMessage> PutReservation(
            Uri address,
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PutAsync(address, content);
        }

        public async Task<HttpResponseMessage> GetCurrentYear()
        {
            var client = CreateClient();

            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var yearAddress =
                homeRepresentation.Links.Single(l => l.Rel == "urn:year").Href;
            if (yearAddress is null)
                throw new InvalidOperationException(
                    "Address for current year not found.");

            return await client.GetAsync(new Uri(yearAddress));
        }

        public async Task<HttpResponseMessage> GetCurrentMonth()
        {
            var client = CreateClient();

            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var yearAddress =
                homeRepresentation.Links.Single(l => l.Rel == "urn:month").Href;
            if (yearAddress is null)
                throw new InvalidOperationException(
                    "Address for current month not found.");

            return await client.GetAsync(new Uri(yearAddress));
        }

        public async Task<HttpResponseMessage> GetCurrentDay()
        {
            var client = CreateClient();

            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var yearAddress =
                homeRepresentation.Links.Single(l => l.Rel == "urn:day").Href;
            if (yearAddress is null)
                throw new InvalidOperationException(
                    "Address for current day not found.");

            return await client.GetAsync(new Uri(yearAddress));
        }
    }
}
