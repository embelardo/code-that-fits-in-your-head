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
            Assert.Null(actual.Day);
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
            AssertCurrentMonth(currentMonth, actual.Month);
            Assert.Null(actual.Day);
        }

        [Fact]
        public async Task GetCurrentDay()
        {
            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            var currentDay = now.Day;
            using var service = new RestaurantApiFactory();

            var response = await service.GetCurrentDay();

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await ParseCalendarContent(response);
            AssertCurrentYear(currentYear, actual.Year);
            AssertCurrentMonth(currentMonth, actual.Month);
            AssertCurrentDay(currentDay, actual.Day);
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

        private static void AssertCurrentMonth(int expected, int? actual)
        {
            Assert.NotNull(actual);

            /* If a test runs just at midnight on the last day of the month,
             * the month could change during execution. Thus, while the current
             * month is the most reasonable expectation, the next month should
             * also pass the test.
             * Note that the month could roll over from 12 one year to 1 the
             * year after, so if currentMonth is 12, then 1 is also okay. */
            if (expected < 12)
                Assert.InRange(actual!.Value, expected, expected + 1);
            else
                Assert.True(
                    actual == 12 || actual == 1,
                    $"Expected 12 or 1, but actual was: {actual}.");
        }

        private static void AssertCurrentDay(int expected, int? actual)
        {
            Assert.NotNull(actual);

            /* If a test runs just at midnight, the date could change during
             * execution. Thus, while the current day is the most reasonable
             * expectation, the next day should also passe the test.
             * Note that the day could roll over from 30 or 31 one month to 1
             * the month after. For February, this could happen already on the
             * 28th or 29th. Thus, numbers less than or equal to 31, as well as
             * 1, are also okay. */
            if (expected < 28)
                Assert.InRange(actual!.Value, expected, expected + 1);
            else
                Assert.True(
                    actual <= 31 || actual == 1,
                    $"Expected less than or equal to 31, or 1, but actual was: {actual}.");
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class CalendarTestCases :
            TheoryData<Func<CalendarController, ActionResult>, int, int?, int?, int, int>
        {
            public CalendarTestCases()
            {
                AddYear(2000, 366, 10);
                AddYear(2019, 365, 20);
                AddYear(2020, 366,  5);
                AddYear(2040, 366, 10);
                AddYear(2100, 365,  8);
                AddMonth(2020, 7, 31, 10);
                AddMonth(2020, 6, 30, 12);
                AddMonth(2020, 2, 29, 10);
                AddMonth(2021, 2, 28, 11);
            }

            private void AddYear(int year, int expectedDays, int tableSize)
            {
                Add(sut => sut.Get(year), year, null, null, expectedDays, tableSize);
            }

            private void AddMonth(
                int year,
                int month,
                int expectedDays,
                int tableSize)
            {
                Add(
                    sut => sut.Get(year, month),
                    year,
                    month,
                    null,
                    expectedDays,
                    tableSize);
            }
        }

        [SuppressMessage(
            "Design",
            "CA1062:Validate arguments of public methods",
            Justification = "Parametrised test.")]
        [Theory, ClassData(typeof(CalendarTestCases))]
        public void GetYear(
            Func<CalendarController, ActionResult> act,
            int year,
            int? month,
            int? day,
            int expectedDays,
            int tableSize)
        {
            var sut = new CalendarController(Table.Communal(tableSize));

            var actual = act(sut);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            Assert.Equal(year, dto.Year);
            Assert.Equal(month, dto.Month);
            Assert.Equal(day, dto.Day);
            Assert.NotNull(dto.Days);
            Assert.Equal(expectedDays, dto.Days?.Length);
            Assert.Equal(
                expectedDays,
                dto.Days.Select(d => d.Date).Distinct().Count());
            Assert.All(
                dto.Days.Select(d => d.MaximumPartySize),
                i => Assert.Equal(tableSize, i));
        }
    }
}
