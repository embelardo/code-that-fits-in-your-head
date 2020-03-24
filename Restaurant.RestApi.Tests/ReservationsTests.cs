/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
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
            using var factory = new RestaurantApiFactory();
            var client = factory.CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PostAsync("reservations", content);
        }

        [Theory]
        [InlineData(
            "2023-11-24 19:00", "juliad@example.net", "Julia Domna", 5)]
        [InlineData("2024-02-13 18:15", "x@example.com", "Xenia Ng", 9)]
        public async Task PostValidReservationWhenDatabaseIsEmpty(
            string at,
            string email,
            string name,
            int quantity)
        {
            var db = new FakeDatabase();
            var sut = new ReservationsController(db);

            var dto = new ReservationDto
            {
                At = at,
                Email = email,
                Name = name,
                Quantity = quantity
            };
            await sut.Post(dto);

            var expected = new Reservation(
                DateTime.Parse(dto.At, CultureInfo.InvariantCulture),
                dto.Email,
                dto.Name,
                dto.Quantity);
            Assert.Contains(expected, db);
        }

        [Theory]
        [InlineData(null, "j@example.net", "Jay Xerxes", 1)]
        [InlineData("not a date", "w@example.edu", "Wk Hd", 8)]
        [InlineData("2023-11-30 20:01", null, "Thora", 19)]
        [InlineData("2022-01-02 12:10", "3@example.org", "3 Beard", 0)]
        public async Task PostInvalidReservation(
            string at,
            string email,
            string name,
            int quantity)
        {
            var response =
                await PostReservation(new { at, email, name, quantity });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
