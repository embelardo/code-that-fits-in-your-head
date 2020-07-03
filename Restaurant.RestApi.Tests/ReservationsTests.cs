/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
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

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class ReservationsTests
    {
        [Fact]
        public async Task PostValidReservation()
        {
            using var service = new RestaurantApiFactory();

            var expected = new ReservationDto
            {
                At = "2023-03-10 19:00",
                Email = "katinka@example.com",
                Name = "Katinka Ingabogovinanana",
                Quantity = 2
            };
            var response = await service.PostReservation(expected);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<ReservationDto>();
            Assert.Equal(expected, actual, new ReservationDtoComparer());
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
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            var dto = new ReservationDto
            {
                Id = "B50DF5B1-F484-4D99-88F9-1915087AF568",
                At = at,
                Email = email,
                Name = name,
                Quantity = quantity
            };
            await sut.Post(dto);

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Created,
                new Reservation(
                    Guid.Parse(dto.Id),
                    DateTime.Parse(dto.At, CultureInfo.InvariantCulture),
                    new Email(dto.Email),
                    new Name(dto.Name ?? ""),
                    dto.Quantity));
            Assert.Contains(expected.Reservation, db);
            Assert.Contains(expected, postOffice);
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
            var actual = await getResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(expected, actual, new ReservationDtoComparer());
            Assert.DoesNotContain(address.ToString(), char.IsUpper);
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

        [Theory]
        [InlineData("bar")]
        [InlineData("79F53E9D9A66458AB79E11DA130BF1D8")]
        public async Task DeleteAbsentReservation(string id)
        {
            using var service = new RestaurantApiFactory();

            var url = new Uri($"/reservations/{id}", UriKind.Relative);
            var resp = await service.CreateClient().DeleteAsync(url);

            Assert.True(
                resp.IsSuccessStatusCode,
                $"Actual status code: {resp.StatusCode}.");
        }

        [Fact]
        public async Task DeleteSendsEmail()
        {
            var r = Some.Reservation;
            var db = new FakeDatabase { r };
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            await sut.Delete(r.Id.ToString("N"));

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Deleted,
                r);
            Assert.Contains(expected, postOffice);
        }

        [Fact]
        public async Task DeleteAbsentReservationDoesNotSendEmail()
        {
            var db = new FakeDatabase();
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            await sut.Delete(Guid.NewGuid().ToString("N"));

            Assert.DoesNotContain(
                postOffice,
                o => o.Event == SpyPostOffice.Event.Deleted);
        }

        [Theory]
        [InlineData("2022-06-01 18:47", "b@example.net", "Björk", 2, 5)]
        [InlineData("2022-02-10 19:32", "e@example.gov", "Epica", 5, 4)]
        public async Task UpdateReservation(
            string at,
            string email,
            string name,
            int quantity,
            int newQuantity)
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

            dto.Quantity = newQuantity;
            var putResp = await service.PutReservation(address, dto);

            Assert.True(
                putResp.IsSuccessStatusCode,
                $"Actual status code: {putResp.StatusCode}.");
            var getResp = await service.CreateClient().GetAsync(address);
            var persisted = await getResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(dto, persisted, new ReservationDtoComparer());
            var actual = await putResp.ParseJsonContent<ReservationDto>();
            Assert.Equal(persisted, actual, new ReservationDtoComparer());
        }

        [Theory]
        [InlineData(null, "led@example.net", "Light Expansion Dread", 2)]
        [InlineData("not a date", "cygnet@example.edu", "Committee", 9)]
        [InlineData("2023-12-29 19:00", null, "Quince", 3)]
        [InlineData("2022-10-10 19:10", "4@example.org", "4 Beard", 0)]
        [InlineData("2045-01-31 18:45", "svn@example.com", "Severin", -1)]
        public async Task PutInvalidReservation(
            string at,
            string email,
            string name,
            int quantity)
        {
            using var service = new RestaurantApiFactory();
            var dto = new ReservationDto
            {
                At = "2022-03-22 19:00",
                Email = "soylent@example.net",
                Name = ":wumpscut:",
                Quantity = 1
            };
            var postResp = await service.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();
            Uri address = FindReservationAddress(postResp);

            var putResp = await service.PutReservation(
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
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

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
            var db = new FakeDatabase { Some.Reservation };
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            var dto = Some.Reservation
                .WithId(Guid.NewGuid())
                .WithName(new Name("Qux"))
                .ToDto();
            var id = Some.Reservation.Id.ToString("N");
            await sut.Put(id, dto);

            var r = Assert.Single(db);
            Assert.Equal(Some.Reservation.WithName(new Name("Qux")), r);
        }

        [Fact]
        public async Task PutAbsentReservation()
        {
            var db = new FakeDatabase();
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

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
            var db = new FakeDatabase { r1, r2 };
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

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
            using var service = new RestaurantApiFactory();
            var dto = new ReservationDto
            {
                At = "2023-04-10 20:01",
                Email = "aol@example.gov",
                Name = "Anette Olzon",
                Quantity = 5
            };
            var postResp = await service.PostReservation(dto);
            postResp.EnsureSuccessStatusCode();
            Uri address = FindReservationAddress(postResp);

            dto.Quantity++;
            var putResp = await service.PutReservation(address, dto);

            Assert.True(
                putResp.IsSuccessStatusCode,
                $"Actual status code: {putResp.StatusCode}.");
        }

        [Theory]
        [InlineData("ploeh")]
        [InlineData("fnaah")]
        public async Task PutSendsEmail(string newName)
        {
            var r = Some.Reservation;
            var db = new FakeDatabase { r };
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            var dto = r.WithName(new Name(newName)).ToDto();
            await sut.Put(r.Id.ToString("N"), dto);

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Updated,
                r.WithName(new Name(newName)));
            Assert.Contains(expected, postOffice);
            Assert.DoesNotContain(
                postOffice,
                o => o.Event == SpyPostOffice.Event.Updating);
        }

        [Theory]
        [InlineData("foo@example.com")]
        [InlineData("bar@example.gov")]
        public async Task PutSendsEmailToOldAddresOnChange(string newEmail)
        {
            var r = Some.Reservation;
            var db = new FakeDatabase { r };
            var postOffice = new SpyPostOffice();
            var sut = new ReservationsController(db, postOffice, Some.MaitreD);

            var dto = r.WithEmail(new Email(newEmail)).ToDto();
            await sut.Put(r.Id.ToString("N"), dto);

            var expected = new[] {
                new SpyPostOffice.Observation(
                    SpyPostOffice.Event.Updating,
                    r),
                new SpyPostOffice.Observation(
                    SpyPostOffice.Event.Updated,
                    r.WithEmail(new Email(newEmail))) }.ToHashSet();
            Assert.Superset(expected, postOffice.ToHashSet());
        }
    }
}
