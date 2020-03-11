/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class ReservationsTests
    {
        [Fact]
        public async Task PostValidReservation()
        {
            var response = await PostReservation(new {
                at = "2023-03-10 19:00",
                email = "katinka@example.com",
                name = "Katinka Ingabogovinanana",
                quantity = 2 });

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
        }

        [SuppressMessage(
            "Usage",
            "CA2234:Pass system uri objects instead of strings",
            Justification = "URL isn't passed as variable, but as literal.")]
        private async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            using var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PostAsync("reservations", content);
        }

        [Fact]
        public async Task PostValidReservationWhenDatabaseIsEmpty()
        {
            var db = new FakeDatabase();
            var sut = new ReservationsController(db);

            var dto = new ReservationDto
            {
                At = "2023-11-24 19:00",
                Email = "juliad@example.net",
                Name = "Julia Domna",
                Quantity = 5
            };
            await sut.Post(dto);

            var expected = new Reservation(
                new DateTime(2023, 11, 24, 19, 0, 0),
                dto.Email,
                dto.Name,
                dto.Quantity);
            Assert.Contains(expected, db);
        }
    }
}
