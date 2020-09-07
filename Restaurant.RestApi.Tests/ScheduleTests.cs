/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using Ploeh.Samples.Restaurant.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class ScheduleTests
    {
        [Theory]
        [InlineData(2022,  3, 11)]
        [InlineData(2018, 11, 25)]
        [InlineData(2025, 12, 31)]
        public async Task GetScheduleWhileUnauthorized(
            int year,
            int month,
            int day)
        {
            using var api = new LegacyApi();
            var response = await api.GetSchedule(year, month, day);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(2022,  2,  2)]
        [InlineData(2020,  8, 25)]
        [InlineData(2016, 10,  9)]
        public async Task GetScheduleWhileAuthorized(
            int year,
            int month,
            int day)
        {
            using var api = new LegacyApi();
            api.AuthorizeClient();

            var response = await api.GetSchedule(year, month, day);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            var dayDto = Assert.Single(actual.Days);
            Assert.Equal(
                new DateTime(year, month, day).ToIso8601DateString(),
                dayDto.Date);
        }

        [Theory]
        [InlineData(         "Hipgnosta", 2024, 11,  2)]
        [InlineData(              "Nono", 2018,  9,  9)]
        [InlineData("The Vatican Cellar", 2021, 10, 10)]
        public async Task GetRestaurantScheduleWhileAuthorized(
            string name,
            int year,
            int month,
            int day)
        {
            using var api = new SelfHostedApi();
            api.AuthorizeClient();

            var response =
                await api.GetSchedule(name, year, month, day);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            Assert.Equal(name, actual.Name);
            var dayDto = Assert.Single(actual.Days);
            Assert.Equal(
                new DateTime(year, month, day).ToIso8601DateString(),
                dayDto.Date);
        }

        [Fact]
        public async Task GetScheduleForDateWithoutReservations()
        {
            var sut = new ScheduleController(
                new OptionsRestaurantDatabase(
                    RestaurantOptionsBuilder.Grandfather.Build()),
                new FakeDatabase(),
                Some.MaitreD);

            var actual = await sut.Get(2020, 8, 26);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var calendar = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(calendar.Days);
            Assert.Empty(day.Entries);
        }

        [Fact]
        public async Task GetScheduleForDateWithReservation()
        {
            var r = Some.Reservation;
            var db = new FakeDatabase();
            await db.Create(Grandfather.Id, r);
            var sut = new ScheduleController(
                new OptionsRestaurantDatabase(
                    RestaurantOptionsBuilder.Grandfather.Build()),
                db,
                Some.MaitreD);

            var actual = await sut.Get(r.At.Year, r.At.Month, r.At.Day);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var calendar = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(calendar.Days);
            Assert.Contains(
                day.Entries.SelectMany(e => e.Reservations), 
                rdto => rdto.Id == r.Id.ToString("N"));
        }

        [Theory]
        [InlineData( 4)]
        [InlineData(22)]
        public async Task GetScheduleForDateWithReservationAtOtherRestaurant(
            int restaurantId)
        {
            var r = Some.Reservation;
            var db = new FakeDatabase();
            await db.Create(Grandfather.Id, r);
            var sut = new ScheduleController(
                new OptionsRestaurantDatabase(
                    RestaurantOptionsBuilder.Grandfather.Build(),
                    new RestaurantOptionsBuilder().WithId(restaurantId).Build()),
                db,
                Some.MaitreD);

            var actual =
                await sut.Get(restaurantId, r.At.Year, r.At.Month, r.At.Day);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var calendar = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(calendar.Days);
            Assert.Empty(day.Entries);
        }

        [Fact]
        public async Task GetScheduleWhenSeatingDurationIsShort()
        {
            var r1 = Some.Reservation;
            var r2 = r1.OneHourLater().WithId(Guid.NewGuid());
            var db = new FakeDatabase();
            await db.Create(2, r1);
            await db.Create(2, r2);
            var sut = new ScheduleController(
                new OptionsRestaurantDatabase(
                    new RestaurantOptionsBuilder()
                        .WithId(2)
                        .WithSeatingDuration(TimeSpan.FromHours(.5))
                        .Build()),
                db,
                Some.MaitreD);

            var actual = await sut.Get(2, r1.At.Year, r1.At.Month, r1.At.Day);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var calendar = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(calendar.Days);
            // Because the seating duration is so short, the entries shouldn't
            // overlap; thus, each entry should contain only a single
            // reservation.
            Assert.All(day.Entries, e => Assert.Single(e.Reservations));
        }

        [Fact]
        public async Task GetScheduleForAbsentRestaurant()
        {
            var sut = new ScheduleController(
                new OptionsRestaurantDatabase(
                    new RestaurantOptionsBuilder().WithId(2).Build()),
                new FakeDatabase(),
                Some.MaitreD);

            var actual = await sut.Get(3, 2089, 12, 9);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }
    }
}
