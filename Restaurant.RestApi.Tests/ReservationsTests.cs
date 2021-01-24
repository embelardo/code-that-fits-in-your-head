/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Ploeh.Samples.Restaurants.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class ReservationsTests
    {
        [Fact]
        public async Task PostValidReservation()
        {
            using var api = new LegacyApi();

            var expected = new ReservationDto
            {
                At = DateTime.Today.AddDays(778).At(19, 0)
                        .ToIso8601DateTimeString(),
                Email = "katinka@example.com",
                Name = "Katinka Ingabogovinanana",
                Quantity = 2
            };
            var response = await api.PostReservation(expected);

            response.EnsureSuccessStatusCode();
            var actual = await response.ParseJsonContent<ReservationDto>();
            Assert.Equal(expected, actual, new ReservationDtoComparer());
        }

        [Theory]
        [InlineData(1049, 19, 00, "juliad@example.net", "Julia Domna", 5)]
        [InlineData(1130, 18, 15, "x@example.com", "Xenia Ng", 9)]
        [InlineData( 956, 16, 55, "kite@example.edu", null, 2)]
        [InlineData( 433, 17, 30, "shli@example.org", "Shanghai Li", 5)]
        public async Task PostValidReservationWhenDatabaseIsEmpty(
            int days,
            int hours,
            int minutes,
            string email,
            string name,
            int quantity)
        {
            var at = DateTime.Now.Date + new TimeSpan(days, hours, minutes, 0);
            var db = new FakeDatabase();
            var sut = new ReservationsController(
                new SystemClock(),
                new InMemoryRestaurantDatabase(Grandfather.Restaurant),
                db);

            var dto = new ReservationDto
            {
                Id = "B50DF5B1-F484-4D99-88F9-1915087AF568",
                At = at.ToString("O"),
                Email = email,
                Name = name,
                Quantity = quantity
            };
            await sut.Post(dto);

            var expected = new Reservation(
                Guid.Parse(dto.Id),
                DateTime.Parse(dto.At, CultureInfo.InvariantCulture),
                new Email(dto.Email),
                new Name(dto.Name ?? ""),
                dto.Quantity);
            Assert.Contains(expected, db.Grandfather);
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
            using var api = new LegacyApi();
            var response = await api.PostReservation(
                new { at, email, name, quantity });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task OverbookAttempt()
        {
            using var api = new LegacyApi();
            await api.PostReservation(new
            {
                at = "2022-03-18 17:30",
                email = "mars@example.edu",
                name = "Marina Seminova",
                quantity = 6
            });

            var response = await api.PostReservation(new
            {
                at = "2022-03-18 17:30",
                email = "shli@example.org",
                name = "Shanghai Li",
                quantity = 5
            });

            await AssertOverbookResponse(response);
        }

        private static async Task AssertOverbookResponse(
            HttpResponseMessage response)
        {
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
            var date = DateTime.Today.AddDays(714).ToIso8601DateString();
            using var api = new LegacyApi();
            await api.PostReservation(new
            {
                at = $"{date} 18:15",
                email = "net@example.net",
                name = "Ned Tucker",
                quantity = 2
            });

            var response = await api.PostReservation(new
            {
                at = $"{date} 18:30",
                email = "kant@example.edu",
                name = "Katrine Nøhr Troelsen",
                quantity = 4
            });

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(867, 19, 10, "adur@example.net", "Adrienne Ursa", 2)]
        [InlineData(901, 18, 55, "emol@example.gov", "Emma Olsen", 5)]
        public async Task ReadSuccessfulReservation(
            int days,
            int hours,
            int minutes,
            string email,
            string name,
            int quantity)
        {
            using var api = new LegacyApi();
            var at = DateTime.Today.AddDays(days).At(hours, minutes)
                .ToIso8601DateTimeString();
            var expected = Create.ReservationDto(at, email, name, quantity);
            var postResp = await api.PostReservation(expected);
            Uri address = FindReservationAddress(postResp);

            var getResp = await api.CreateClient().GetAsync(address);

            getResp.EnsureSuccessStatusCode();
            var actual = await getResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(expected, actual, new ReservationDtoComparer());
            AssertUrlFormatIsIdiomatic(address);
        }

        private static void AssertUrlFormatIsIdiomatic(Uri address)
        {
            // Consider the URL to be idiomatically formatted if it's entirely
            // in lower case. Exempt from this rule are querystring values,
            // which may contain run-time data, or, as is more the case in this
            // API, Base64-encoded HMAC signatures.
            Assert.DoesNotContain(
                address.GetLeftPart(UriPartial.Path),
                char.IsUpper);
        }

        private static Uri FindReservationAddress(HttpResponseMessage response)
        {
            return response.Headers.Location;
        }

        [Theory]
        [InlineData("E56C0B933E91463685579CE1215F6956")]
        [InlineData("foo")]
        public async Task GetAbsentReservation(string id)
        {
            using var api = new LegacyApi();

            var url = new Uri($"/reservations/{id}", UriKind.Relative);
            var resp = await api.CreateClient().GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Theory]
        [InlineData("DA5EFE7FAE914F43828467D403DD9814")]
        [InlineData("d4fec75f8d054299975515f757f1223e")]
        public async Task NoHackingOfUrlsAllowed(string id)
        {
            using var api = new LegacyApi();
            var dto = Some.Reservation.ToDto();
            dto.Id = id;
            dto.At = DateTime.Today.AddDays(435).At(20, 15)
                .ToIso8601DateTimeString();
            var postResp = await api.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();

            /* This is the sort of 'hacking' of URLs that clients should be
             * discouraged from. Clients should be following links, as all the
             * other tests in this test suite demonstrate. */
            var urlHack = new Uri($"/reservations/{id}", UriKind.Relative);
            var getResp = await api.CreateClient().GetAsync(urlHack);

            /* The expected result of a 'hacked' URL is 404 Not Found rather
             * than 403 Forbidden. Clients are simply requesting a URL which
             * doesn't exist. */
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }

        [Theory]
        [InlineData(884, 18, 47, "c@example.net", "Nick Klimenko", 2)]
        [InlineData(902, 18, 50, "emot@example.gov", "Emma Otting", 5)]
        public async Task DeleteReservation(
            int days,
            int hours,
            int minutes,
            string email,
            string name,
            int quantity)
        {
            using var api = new LegacyApi();
            var at = DateTime.Today.AddDays(days).At(hours, minutes)
                .ToIso8601DateTimeString();
            var dto = Create.ReservationDto(at, email, name, quantity);
            var postResp = await api.PostReservation(dto);
            Uri address = FindReservationAddress(postResp);

            var deleteResp = await api.CreateClient().DeleteAsync(address);

            deleteResp.EnsureSuccessStatusCode();
            var getResp = await api.CreateClient().GetAsync(address);
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }

        [Theory]
        [InlineData("d46668b1ea484d9d918e2143e2f6e991")]
        [InlineData("79F53E9D9A66458AB79E11DA130BF1D8")]
        public async Task DeleteIsIdempotent(string id)
        {
            using var api = new LegacyApi();
            var dto = Some.Reservation.ToDto();
            dto.Id = id;
            dto.At = DateTime.Today.AddDays(435).At(20, 15)
                .ToIso8601DateTimeString();
            var postResp = await api.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();
            var url = FindReservationAddress(postResp);

            await api.CreateClient().DeleteAsync(url);
            var resp = await api.CreateClient().DeleteAsync(url);

            resp.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(494, 18, 47, "b@example.net", "Björk", 2, 5)]
        [InlineData(383, 19, 32, "e@example.gov", "Epica", 5, 4)]
        public async Task UpdateReservation(
            int days,
            int hours,
            int minutes,
            string email,
            string name,
            int quantity,
            int newQuantity)
        {
            using var api = new LegacyApi();
            var at = DateTime.Today.AddDays(days).At(hours, minutes)
                .ToIso8601DateTimeString();
            var dto = Create.ReservationDto(at, email, name, quantity);
            var postResp = await api.PostReservation(dto);
            Uri address = FindReservationAddress(postResp);

            dto.Quantity = newQuantity;
            var putResp = await api.PutReservation(address, dto);

            putResp.EnsureSuccessStatusCode();
            var getResp = await api.CreateClient().GetAsync(address);
            var persisted = await getResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(dto, persisted, new ReservationDtoComparer());
            var actual = await putResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(persisted, actual, new ReservationDtoComparer());
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private sealed class PutInvalidReservationTestCases :
            TheoryData<string?, string?, string, int>
        {
            public PutInvalidReservationTestCases()
            {
                Add(null, "led@example.net", "Light Expansion Dread", 2);
                Add("not a date", "cygnet@example.edu", "Committee", 9);
                AddWithDate(1071, 19, 0, null, "Quince", 3);
                AddWithDate( 626, 19, 10, "4@example.org", "4 Beard", 0);
                AddWithDate(8775, 18, 45, "svn@example.com", "Severin", -1);
            }

            private void AddWithDate(
                int days,
                int hours,
                int minutes,
                string? email,
                string name,
                int quantity)
            {
                var at = DateTime.Today.AddDays(days).At(hours, minutes);
                Add(at.ToIso8601DateTimeString(), email, name, quantity);
            }
        }

        [Theory, ClassData(typeof(PutInvalidReservationTestCases))]
        public async Task PutInvalidReservation(
            string at,
            string email,
            string name,
            int quantity)
        {
            using var api = new LegacyApi();
            var dto = new ReservationDto
            {
                At = DateTime.Today.AddDays(423).At(19, 0)
                        .ToIso8601DateTimeString(),
                Email = "soylent@example.net",
                Name = ":wumpscut:",
                Quantity = 1
            };
            var postResp = await api.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();
            Uri address = FindReservationAddress(postResp);

            var putResp = await api.PutReservation(
                address,
                new { at, email, name, quantity });

            Assert.Equal(HttpStatusCode.BadRequest, putResp.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("bas")]
        public async Task PutInvalidId(string invalidId)
        {
            var db = new FakeDatabase();
            var sut = new ReservationsController(
                new SystemClock(),
                new InMemoryRestaurantDatabase(Some.Restaurant),
                db);

            var dummyDto = new ReservationDto
            {
                At = "2024-06-25 18:19",
                Email = "thorne@example.com",
                Name = "Tracy Thorne",
                Quantity = 2
            };
            var actual = await sut.Put(invalidId, dummyDto);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Fact]
        public async Task PutConflictingIds()
        {
            var db = new FakeDatabase();
            var r = Some.Reservation.WithDate(
                DateTime.Today.AddDays(435).At(20, 15));
            db.Grandfather.Add(r);
            var sut = new ReservationsController(
                new SystemClock(),
                new InMemoryRestaurantDatabase(Grandfather.Restaurant),
                db);

            var dto = r
                .WithId(Guid.NewGuid())
                .WithName(new Name("Qux"))
                .ToDto();
            await sut.Put(r.Id.ToString("N"), dto);

            var actual = Assert.Single(db.Grandfather);
            Assert.Equal(r.WithName(new Name("Qux")), actual);
        }

        [Fact]
        public async Task PutAbsentReservation()
        {
            var db = new FakeDatabase();
            var sut = new ReservationsController(
                new SystemClock(),
                new InMemoryRestaurantDatabase(Some.Restaurant),
                db);

            var dto = new ReservationDto
            {
                At = "2023-11-23 18:21",
                Email = "tori@example.org",
                Name = "Tori Amos",
                Quantity = 9
            };
            var id = "7a4d6e05a6ae41a3a7d00943be05048c";
            var actual = await sut.Put(id, dto);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Fact]
        public async Task ChangeDateToSoldOutDate()
        {
            var r1 = Some.Reservation;
            var r2 = Some.Reservation
                .WithId(Guid.NewGuid())
                .TheDayAfter()
                .WithQuantity(10);
            var db = new FakeDatabase();
            db.Grandfather.Add(r1);
            db.Grandfather.Add(r2);
            var sut = new ReservationsController(
                new SystemClock(),
                new InMemoryRestaurantDatabase(Grandfather.Restaurant),
                db);

            var dto = r1.WithDate(r2.At).ToDto();
            var actual = await sut.Put(r1.Id.ToString("N"), dto);

            var oRes = Assert.IsAssignableFrom<ObjectResult>(actual);
            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                oRes.StatusCode);
        }

        [Fact]
        public async Task EditReservationOnSameDayNearCapacity()
        {
            using var api = new LegacyApi();
            var dto = new ReservationDto
            {
                At = DateTime.Today.AddDays(809).At(20, 1)
                        .ToIso8601DateTimeString(),
                Email = "aol@example.gov",
                Name = "Anette Olzon",
                Quantity = 5
            };
            var postResp = await api.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();
            Uri address = FindReservationAddress(postResp);

            dto.Quantity++;
            var putResp = await api.PutReservation(address, dto);

            putResp.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ReserveTableAtNono()
        {
            using var api = new SelfHostedApi();
            var client = api.CreateClient();
            var at = DateTime.Today.AddDays(434).At(20, 15);
            var dto = Some.Reservation.WithDate(at).WithQuantity(6).ToDto();

            var response = await client.PostReservation("Nono", dto);

            await AssertRemainingCapacity(client, at, "Nono", 4);
            await AssertRemainingCapacity(client, at, "Hipgnosta", 10);
        }

        private static async Task AssertRemainingCapacity(
            HttpClient client,
            DateTime date,
            string name,
            int expected)
        {
            var response =
                await client.GetDay(name, date.Year, date.Month, date.Day);
            var day = await response.ParseJsonContent<CalendarDto>();
            Assert.All(
                day.Days.Single().Entries,
                e => Assert.Equal(expected, e.MaximumPartySize));
        }

        [Fact]
        public async Task ReserveTableAtTheVaticanCellar()
        {
            using var api = new SelfHostedApi();
            var client = api.CreateClient();
            var timeOfDayLaterThanLastSeatingAtTheOtherRestaurants =
                TimeSpan.FromHours(21.5);

            var at = DateTime.Today.AddDays(433).Add(
                timeOfDayLaterThanLastSeatingAtTheOtherRestaurants);
            var dto = Some.Reservation.WithDate(at).ToDto();
            var response =
                await client.PostReservation("The Vatican Cellar", dto);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostToAbsentRestaurant()
        {
            var restaurantDB = new InMemoryRestaurantDatabase(Some.Restaurant);
            var sut = new ReservationsController(
                new SystemClock(),
                restaurantDB,
                new FakeDatabase());
            var absentRestaurantId = 4;
            var r = await restaurantDB.GetRestaurant(absentRestaurantId);
            Assert.Null(r);

            var actual =
                await sut.Post(absentRestaurantId, Some.Reservation.ToDto());

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Fact]
        public async Task ChangeTableAtTheVaticanCellar()
        {
            var r = Some.Reservation.WithDate(
                DateTime.Today.AddDays(437).At(20, 15));
            using var api = new SelfHostedApi();
            var client = api.CreateClient();
            var postResponse = await client.PostReservation(
                "The Vatican Cellar",
                r.ToDto());
            postResponse.EnsureSuccessStatusCode();
            var address = FindReservationAddress(postResponse);

            var timeOfDayLaterThanLastSeatingAtTheOtherRestaurants =
                TimeSpan.FromHours(21.5);
            var putResponse = await client.PutReservation(
                address,
                r
                    .WithDate(
                       r.At.Date +
                       timeOfDayLaterThanLastSeatingAtTheOtherRestaurants)
                    .ToDto());

            putResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PutToAbsentRestaurant()
        {
            var absentRestaurantId = 4;
            var restaurantDB = new InMemoryRestaurantDatabase(Some.Restaurant);
            var db = new FakeDatabase();
            await db.Create(absentRestaurantId, Some.Reservation);
            var sut = new ReservationsController(
                new SystemClock(),
                restaurantDB,
                db);
            var r = await restaurantDB.GetRestaurant(absentRestaurantId);
            Assert.Null(r);

            var actual = await sut.Put(
                absentRestaurantId,
                Some.Reservation.Id.ToString("N"),
                Some.Reservation.ToDto());

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }
    }
}
