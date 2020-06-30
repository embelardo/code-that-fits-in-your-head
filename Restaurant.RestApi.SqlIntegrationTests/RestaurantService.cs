/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.SqlIntegrationTests
{
    public class RestaurantService : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservationsRepository>();
                services.AddSingleton<IReservationsRepository>(
                    new SqlReservationsRepository(
                        ConnectionStrings.Reservations));
            });
        }

        public async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";

            var address = new Uri("reservations", UriKind.Relative);
            return await client.PostAsync(address, content);
        }

        public async Task<(Uri, ReservationDto)> PostReservation(
            DateTime date,
            int quantity)
        {
            var resp = await PostReservation(new ReservationDtoBuilder()
                .WithDate(date)
                .WithQuantity(quantity)
                .Build());
            resp.EnsureSuccessStatusCode();

            var dto = await ParseReservationContent(resp);

            return (resp.Headers.Location, dto);
        }

        private static async Task<ReservationDto> ParseReservationContent(
            HttpResponseMessage msg)
        {
            var json = await msg.Content.ReadAsStringAsync();
            var reservation = JsonSerializer.Deserialize<ReservationDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return reservation;
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
    }
}
