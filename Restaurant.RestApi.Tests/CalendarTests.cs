/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class CalendarTests
    {
        [Fact]
        public async Task GetCurrentYear()
        {
            var currentYear = DateTime.Now.Year;
            using var service = new RestaurantApiFactory();

            var response = await service.GetCurrentYear();

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await ParseCalendarContent(response);
            AssertCurrentYear(currentYear, actual.Year);
            Assert.Null(actual.Month);
        }

        [Fact]
        public async Task GetCurrentMonth()
        {
            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            using var service = new RestaurantApiFactory();

            var response = await service.GetCurrentMonth();

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await ParseCalendarContent(response);
            AssertCurrentYear(currentYear, actual.Year);
            AssertCurrentMonth(currentMonth, actual.Month ?? 0);
        }

        private static async Task<CalendarDto> ParseCalendarContent(
            HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var calendar = JsonSerializer.Deserialize<CalendarDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return calendar;
        }

        private static void AssertCurrentYear(int expected, int actual)
        {
            /* If a test runs just at midnight on December 31, the year could
             * increment during execution. Thus, while the current year is the
             * most reasonable expectation, the next year should also pass the
             * test. */
            Assert.InRange(actual, expected, expected + 1);
        }

        private static void AssertCurrentMonth(int expected, int actual)
        {
            /* If a test runs just at midnight on the last day of the month,
             * the month could change during execution. Thus, while the current
             * month is the most reasonable expectation, the next month should
             * also pass the test.
             * Note that the month could roll over from 12 one year to 1 the
             * year after, so if currentMonth is 12, then 1 is also okay. */
            if (expected < 12)
                Assert.InRange(actual, expected, expected + 1);
            else
                Assert.True(
                    actual == 12 || actual == 1,
                    $"Expected 12 or 1, but actual was: {actual}.");
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class CalendarTestCases : TheoryData<int, int, int>
        {
            public CalendarTestCases()
            {
                Add(2000, 366, 10);
                Add(2019, 365, 20);
                Add(2020, 366,  5);
                Add(2040, 366, 10);
                Add(2100, 365,  8);
            }
        }

        [Theory, ClassData(typeof(CalendarTestCases))]
        public void GetYear(int year, int expectedDays, int tableSize)
        {
            var sut = new CalendarController(Table.Communal(tableSize));

            var actual = sut.Get(year);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            Assert.Equal(year, dto.Year);
            Assert.NotNull(dto.Days);
            var days = dto.Days;
            Assert.Equal(expectedDays, days?.Length);
            Assert.Equal(
                expectedDays,
                days.Select(d => d.Date).Distinct().Count());
            Assert.All(
                dto.Days.Select(d => d.MaximumPartySize),
                i => Assert.Equal(tableSize, i));
        }
    }
}
