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
            using var service = new SelfHostedService();

            var before = DateTime.Now;
            var response = await service.GetCurrentYear();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(before.Year, after.Year, actual.Year);
            Assert.Null(actual.Month);
            Assert.Null(actual.Day);
            AssertLinks(actual);
        }

        [Fact]
        public async Task GetNextYear()
        {
            using var service = new SelfHostedService();

            var before = DateTime.Now;
            var response = await service.GetNextYear();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddYears(1).Year,
                after.AddYears(1).Year,
                actual.Year);
            Assert.Null(actual.Month);
            Assert.Null(actual.Day);
        }

        [Fact]
        public async Task GetCurrentMonth()
        {
            using var service = new SelfHostedService();

            var before = DateTime.Now;
            var response = await service.GetCurrentMonth();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(before.Year, after.Year, actual.Year);
            AssertOneOf(before.Month, after.Month, actual.Month);
            Assert.Null(actual.Day);
            AssertLinks(actual);
        }

        [Fact]
        public async Task GetCurrentDay()
        {
            using var service = new SelfHostedService();

            var before = DateTime.Now;
            var response = await service.GetCurrentDay();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(before.Year, after.Year, actual.Year);
            AssertOneOf(before.Month, after.Month, actual.Month);
            AssertOneOf(before.Day, after.Day, actual.Day);
            AssertLinks(actual);
        }

        private static void AssertOneOf(
            int expected1,
            int expected2,
            int? actual)
        {
            Assert.True(
                expected1 == actual || expected2 == actual,
                $"Expected {expected1} or {expected2}, but actual was: {actual}.");
        }

        private static void AssertLinks(CalendarDto actual)
        {
            Assert.NotNull(actual.Links);

            var prev = Assert.Single(actual.Links, l => l.Rel == "previous");
            AssertHrefAbsoluteUrl(prev);

            var next = Assert.Single(actual.Links, l => l.Rel == "next");
            AssertHrefAbsoluteUrl(next);
        }

        private static void AssertHrefAbsoluteUrl(LinkDto dto)
        {
            Assert.True(
                Uri.TryCreate(dto.Href, UriKind.Absolute, out var _),
                $"Not an absolute URL: {dto.Href}.");
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
                AddDay(2020, 7, 3, 8);
                AddDay(2021, 8, 2, 2);
                AddDay(2022, 2, 28, 7);
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

            private void AddDay(
                int year,
                int month,
                int day,
                int tableSize)
            {
                Add(
                    sut => sut.Get(year, month, day),
                    year,
                    month,
                    day,
                    1,
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
