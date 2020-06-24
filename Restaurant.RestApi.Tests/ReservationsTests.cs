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
            using var service = new RestaurantApiFactory();
            var response = await service.PostReservation(new {
                at = "2023-03-10 19:00",
                email = "katinka@example.com",
                name = "Katinka Ingabogovinanana",
                quantity = 2 });
            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
        }

        [Theory]
        [InlineData(
            "2023-11-24 19:00", "juliad@example.net", "Julia Domna", 5)]
        [InlineData("2024-02-13 18:15", "x@example.com", "Xenia Ng", 9)]
        [InlineData("2023-08-23 16:55", "kite@example.edu", null, 2)]
        [InlineData("2022-03-18 17:30", "shli@example.org", "Shanghai Li", 5)]
        public async Task PostValidReservationWhenDatabaseIsEmpty(
            string at,
            string email,
            string name,
            int quantity)
        {
            var db = new FakeDatabase();
            var sut = new ReservationsController(db, Some.MaitreD);

            var dto = new ReservationDto
            {
                Id = "B50DF5B1-F484-4D99-88F9-1915087AF568",
                At = at,
                Email = email,
                Name = name,
                Quantity = quantity
            };
            await sut.Post(dto);

            var expected = new Reservation(
                Guid.Parse(dto.Id),
                DateTime.Parse(dto.At, CultureInfo.InvariantCulture),
                dto.Email,
                dto.Name ?? "",
                dto.Quantity);
            Assert.Contains(expected, db);
        }

        [Theory]
        [InlineData(null, "j@example.net", "Jay Xerxes", 1)]
        [InlineData("not a date", "w@example.edu", "Wk Hd", 8)]
        [InlineData("2023-11-30 20:01", null, "Thora", 19)]
        [InlineData("2022-01-02 12:10", "3@example.org", "3 Beard", 0)]
        [InlineData("2045-12-31 11:45", "git@example.com", "Gil Tan", -1)]
        public async Task PostInvalidReservation(
            string at,
            string email,
            string name,
            int quantity)
        {
            using var service = new RestaurantApiFactory();
            var response = await service.PostReservation(
                new { at, email, name, quantity });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task OverbookAttempt()
        {
            using var service = new RestaurantApiFactory();
            await service.PostReservation(new
            {
                at = "2022-03-18 17:30",
                email = "mars@example.edu",
                name = "Marina Seminova",
                quantity = 6
            });

            var response = await service.PostReservation(new
            {
                at = "2022-03-18 17:30",
                email = "shli@example.org",
                name = "Shanghai Li",
                quantity = 5
            });

            Assert.Equal(
                HttpStatusCode.InternalServerError,
                response.StatusCode);
            Assert.NotNull(response.Content);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(
                "tables",
                content,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task BookTableWhenFreeSeatingIsAvailable()
        {
            using var service = new RestaurantApiFactory();
            await service.PostReservation(new
            {
                at = "2023-01-02 18:15",
                email = "net@example.net",
                name = "Ned Tucker",
                quantity = 2
            });

            var response = await service.PostReservation(new
            {
                at = "2023-01-02 18:30",
                email = "kant@example.edu",
                name = "Katrine Nøhr Troelsen",
                quantity = 4
            });

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
        }

        [Theory]
        [InlineData(
            "2023-06-09 19:10", "adur@example.net", "Adrienne Ursa", 2)]
        [InlineData("2023-07-13 18:55", "emol@example.gov", "Emma Olsen", 5)]
        public async Task ReadSuccessfulReservation(
            string date,
            string email,
            string name,
            int quantity)
        {
            using var service = new RestaurantApiFactory();
            var expected = new ReservationDto
            {
                At = date,
                Email = email,
                Name = name,
                Quantity = quantity
            };
            var postResp = await service.PostReservation(expected);
            Uri address = FindReservationAddress(postResp);

            var getResp = await service.CreateClient().GetAsync(address);

            Assert.True(
                getResp.IsSuccessStatusCode,
                $"Actual status code: {postResp.StatusCode}.");
            var actual = await ParseReservationContent(getResp);
            Assert.Equal(expected, actual, new ReservationDtoComparer());
            Assert.DoesNotContain(address.ToString(), char.IsUpper);
        }

        private static Uri FindReservationAddress(HttpResponseMessage response)
        {
            return response.Headers.Location;
        }

        private static async Task<ReservationDto> ParseReservationContent(
            HttpResponseMessage actual)
        {
            var json = await actual.Content.ReadAsStringAsync();
            var reservation = JsonSerializer.Deserialize<ReservationDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return reservation;
        }

        [Theory]
        [InlineData("E56C0B933E91463685579CE1215F6956")]
        [InlineData("foo")]
        public async Task GetAbsentReservation(string id)
        {
            using var service = new RestaurantApiFactory();

            var url = new Uri($"/reservations/{id}", UriKind.Relative);
            var resp = await service.CreateClient().GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Theory]
        [InlineData("2023-06-24 18:47", "c@example.net", "Nick Klimenko", 2)]
        [InlineData("2023-07-12 18:50", "emot@example.gov", "Emma Otting", 5)]
        public async Task DeleteReservation(
            string at,
            string email,
            string name,
            int quantity)
        {
            using var service = new RestaurantApiFactory();
            var dto = new ReservationDto
            {
                At = at,
                Email = email,
                Name = name,
                Quantity = quantity
            };
            var postResp = await service.PostReservation(dto);
            Uri address = FindReservationAddress(postResp);

            var deleteResp = await service.CreateClient().DeleteAsync(address);

            Assert.True(
                deleteResp.IsSuccessStatusCode,
                $"Actual status code: {deleteResp.StatusCode}.");
            var getResp = await service.CreateClient().GetAsync(address);
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }
    }
}
