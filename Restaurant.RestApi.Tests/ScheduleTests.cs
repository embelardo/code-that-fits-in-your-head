/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            using var service = new SelfHostedService();
            var response = await service.GetSchedule(year, month, day);
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
            using var service = new SelfHostedService();
            service.AuthorizeClient();

            var response = await service.GetSchedule(year, month, day);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            var dayDto = Assert.Single(actual.Days);
            Assert.Equal(
                new DateTime(year, month, day).ToIso8601DateString(),
                dayDto.Date);
        }

        [Fact]
        public async Task GetScheduleForDateWithoutReservations()
        {
            var db = new FakeDatabase();
            var sut = new ScheduleController(db, Some.MaitreD);

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
            await db.Create(r);
            var sut = new ScheduleController(db, Some.MaitreD);

            var actual = await sut.Get(r.At.Year, r.At.Month, r.At.Day);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var calendar = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(calendar.Days);
            Assert.NotEmpty(day.Entries);
        }
    }
}
