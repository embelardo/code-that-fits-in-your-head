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
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetCurrentYear();
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
        public async Task GetPreviousYear()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetPreviousYear();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddYears(-1).Year,
                after.AddYears(-1).Year,
                actual.Year);
            Assert.Null(actual.Month);
            Assert.Null(actual.Day);
        }

        [Fact]
        public async Task GetNextYear()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetNextYear();
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

        [Theory]
        [InlineData(2009)]
        [InlineData(2019)]
        [InlineData(2020)]
        [InlineData(2021)]
        [InlineData(2029)]
        public async Task GetSpecificYear(int year)
        {
            using var api = new LegacyApi();

            var response = await api.GetYear(year);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            Assert.Equal(year, actual.Year);
            Assert.Null(actual.Month);
            Assert.Null(actual.Day);
            AssertLinks(actual);
        }

        [Fact]
        public async Task GetCurrentMonth()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetCurrentMonth();
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
        public async Task GetPreviousMonth()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetPreviousMonth();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddMonths(-1).Year,
                after.AddMonths(-1).Year,
                actual.Year);
            AssertOneOf(
                before.AddMonths(-1).Month,
                after.AddMonths(-1).Month,
                actual.Month);
            Assert.Null(actual.Day);
        }

        [Fact]
        public async Task GetNextMonth()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetNextMonth();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddMonths(1).Year,
                after.AddMonths(1).Year,
                actual.Year);
            AssertOneOf(
                before.AddMonths(1).Month,
                after.AddMonths(1).Month,
                actual.Month);
            Assert.Null(actual.Day);
        }

        [Theory]
        [InlineData(2010, 12)]
        [InlineData(2020,  4)]
        [InlineData(2020,  7)]
        [InlineData(2020,  9)]
        [InlineData(2030,  8)]
        public async Task GetSpecificMonth(int year, int month)
        {
            using var api = new LegacyApi();

            var response = await api.GetMonth(year, month);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            Assert.Equal(year, actual.Year);
            Assert.Equal(month, actual.Month);
            Assert.Null(actual.Day);
            AssertLinks(actual);
        }

        [Fact]
        public async Task GetCurrentDay()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetCurrentDay();
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

        [Fact]
        public async Task GetPreviousDay()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetPreviousDay();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddDays(-1).Year,
                after.AddDays(-1).Year,
                actual.Year);
            AssertOneOf(
                before.AddDays(-1).Month,
                after.AddDays(-1).Month,
                actual.Month);
            AssertOneOf(
                before.AddDays(-1).Day,
                after.AddDays(-1).Day,
                actual.Day);
        }

        [Fact]
        public async Task GetNextDay()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetNextDay();
            var after = DateTime.Now;

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            AssertOneOf(
                before.AddDays(1).Year,
                after.AddDays(1).Year,
                actual.Year);
            AssertOneOf(
                before.AddDays(1).Month,
                after.AddDays(1).Month,
                actual.Month);
            AssertOneOf(
                before.AddDays(1).Day,
                after.AddDays(1).Day,
                actual.Day);
        }

        [Theory]
        [InlineData(2010, 3, 28)]
        [InlineData(2020, 7,  1)]
        [InlineData(2020, 7, 10)]
        [InlineData(2020, 7, 17)]
        [InlineData(2030, 2,  9)]
        public async Task GetSpecificDay(int year, int month, int day)
        {
            using var api = new LegacyApi();

            var response = await api.GetDay(year, month, day);

            Assert.True(
                response.IsSuccessStatusCode,
                $"Actual status code: {response.StatusCode}.");
            var actual = await response.ParseJsonContent<CalendarDto>();
            Assert.Equal(year, actual.Year);
            Assert.Equal(month, actual.Month);
            Assert.Equal(day, actual.Day);
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

            Assert.NotNull(actual.Days);
            AssertDayLinks(actual.Days, "urn:day");
            AssertDayLinks(actual.Days, "urn:month");
            AssertDayLinks(actual.Days, "urn:year");
            AssertDayLinks(actual.Days, "urn:schedule");
        }

        private static void AssertDayLinks(DayDto[]? days, string rel)
        {
            var links = days.SelectMany(d => d.Links.Where(l => l.Rel == rel));
            Assert.Equal(days?.Length, links.Count());
            Assert.All(links, AssertHrefAbsoluteUrl);
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
            TheoryData<Func<CalendarController, Task<ActionResult>>, int, int?, int?, int, int>
        {
            public CalendarTestCases()
            {
                AddYear(2000, 366, 10);
                AddYear(2019, 365,  3);
                AddYear(2020, 366, 12);
                AddYear(2040, 366,  8);
                AddYear(2100, 365, 20);
                AddMonth(2020, 7, 31, 1);
                AddMonth(2020, 6, 30, 6);
                AddMonth(2020, 2, 29, 9);
                AddMonth(2021, 2, 28, 8);
                AddDay(2020, 7,  3, 11);
                AddDay(2021, 8,  2, 11);
                AddDay(2022, 2, 28, 13);
            }

            private void AddYear(int year, int expectedDays, int tableSize)
            {
                Add(
                    sut => sut.GetYear(year),
                    year,
                    null,
                    null,
                    expectedDays,
                    tableSize);
            }

            private void AddMonth(
                int year,
                int month,
                int expectedDays,
                int tableSize)
            {
                Add(
                    sut => sut.GetMonth(year, month),
                    year,
                    month,
                    null,
                    expectedDays,
                    tableSize);
            }

            private void AddDay(int year, int month, int day, int tableSize)
            {
                Add(
                    sut => sut.GetDay(year, month, day),
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
        public async Task GetCalendar(
            Func<CalendarController, Task<ActionResult>> act,
            int year,
            int? month,
            int? day,
            int expectedDays,
            int tableSize)
        {
            var sut = new CalendarController(
                Some.RestaurantDatabase,
                new FakeDatabase(),
                Some.MaitreD.WithTables(Table.Communal(tableSize)));

            var actual = await act(sut);

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
            var timeSlotEntries =
                dto.Days.SelectMany(d => d.Entries ?? Array.Empty<TimeDto>());
            Assert.True(
                expectedDays <= (timeSlotEntries?.Count() ?? 0),
                $"Expected at least one time slot entry per day. Expected: {expectedDays}; actual: {timeSlotEntries?.Count() ?? 0}.");
            // There's no reservations in these test cases, so all time slots
            // should allow up to the (single) table's capacity.
            Assert.All(
                timeSlotEntries,
                t => Assert.Equal(tableSize, t.MaximumPartySize));
            Assert.All(
                dto.Days,
                d => Assert.Contains(
                    sut.MaitreD.OpensAt.ToIso8601TimeString(),
                    d.Entries.Select(e => e.Time)));
            Assert.All(
                dto.Days,
                d => Assert.Contains(
                    sut.MaitreD.LastSeating.ToIso8601TimeString(),
                    d.Entries.Select(e => e.Time)));
        }

        [Fact]
        public async Task ViewCalendarForDayWithReservation()
        {
            var date = new DateTime(2020, 8, 21);
            var maitreD = new MaitreD(
                TimeSpan.FromHours(18),
                TimeSpan.FromHours(20),
                TimeSpan.FromHours(.75),
                Table.Standard(4));
            var db = new FakeDatabase();
            await db.Create(
                Grandfather.Id,
                Some.Reservation
                    .WithQuantity(3)
                    .WithDate(new DateTime(2020, 8, 21, 19, 0, 0)));
            var sut =
                new CalendarController(Some.RestaurantDatabase, db, maitreD);

            var actual = await sut.GetDay(date.Year, date.Month, date.Day);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(dto.Days);
            var expected = new[]
            {
                new TimeDto { Time = "18:00:00", MaximumPartySize = 4, },
                new TimeDto { Time = "18:15:00", MaximumPartySize = 4, },
                new TimeDto { Time = "18:30:00", MaximumPartySize = 0, },
                new TimeDto { Time = "18:45:00", MaximumPartySize = 0, },
                new TimeDto { Time = "19:00:00", MaximumPartySize = 0, },
                new TimeDto { Time = "19:15:00", MaximumPartySize = 0, },
                new TimeDto { Time = "19:30:00", MaximumPartySize = 0, },
                new TimeDto { Time = "19:45:00", MaximumPartySize = 4, },
                new TimeDto { Time = "20:00:00", MaximumPartySize = 4, },
            };
            Assert.Equal(expected, day.Entries, new TimeDtoComparer());
        }

        [Fact]
        public async Task ViewCalendarForMonthWithReservation()
        {
            var maitreD = new MaitreD(
                TimeSpan.FromHours(20),
                TimeSpan.FromHours(22),
                TimeSpan.FromHours(1),
                Table.Communal(12));
            var db = new FakeDatabase();
            await db.Create(
                Grandfather.Id,
                Some.Reservation
                    .WithQuantity(3)
                    .WithDate(new DateTime(2020, 8, 22, 20, 30, 0)));
            var sut =
                new CalendarController(Some.RestaurantDatabase, db, maitreD);

            var actual = await sut.GetMonth(2020, 8);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day =
                Assert.Single(dto.Days.Where(d => d.Date == "2020-08-22"));
            var expected = new[]
            {
                new TimeDto { Time = "20:00:00", MaximumPartySize =  9, },
                new TimeDto { Time = "20:15:00", MaximumPartySize =  9, },
                new TimeDto { Time = "20:30:00", MaximumPartySize =  9, },
                new TimeDto { Time = "20:45:00", MaximumPartySize =  9, },
                new TimeDto { Time = "21:00:00", MaximumPartySize =  9, },
                new TimeDto { Time = "21:15:00", MaximumPartySize =  9, },
                new TimeDto { Time = "21:30:00", MaximumPartySize = 12, },
                new TimeDto { Time = "21:45:00", MaximumPartySize = 12, },
                new TimeDto { Time = "22:00:00", MaximumPartySize = 12, },
            };
            Assert.Equal(expected, day.Entries, new TimeDtoComparer());
        }

        [Fact]
        public async Task ViewCalendarForYearWithReservation()
        {
            var maitreD = new MaitreD(
                TimeSpan.FromHours(18.5),
                TimeSpan.FromHours(22),
                TimeSpan.FromHours(2),
                Table.Standard(4),
                Table.Standard(6));
            var db = new FakeDatabase();
            await db.Create(
                Grandfather.Id,
                Some.Reservation
                    .WithQuantity(5)
                    .WithDate(new DateTime(2020, 9, 23, 20, 15, 0)));
            var sut =
                new CalendarController(Some.RestaurantDatabase, db, maitreD);

            var actual = await sut.GetYear(2020);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day =
                Assert.Single(dto.Days.Where(d => d.Date == "2020-09-23"));
            var expected = new[]
            {
                new TimeDto { Time = "18:30:00", MaximumPartySize = 4, },
                new TimeDto { Time = "18:45:00", MaximumPartySize = 4, },
                new TimeDto { Time = "19:00:00", MaximumPartySize = 4, },
                new TimeDto { Time = "19:15:00", MaximumPartySize = 4, },
                new TimeDto { Time = "19:30:00", MaximumPartySize = 4, },
                new TimeDto { Time = "19:45:00", MaximumPartySize = 4, },
                new TimeDto { Time = "20:00:00", MaximumPartySize = 4, },
                new TimeDto { Time = "20:15:00", MaximumPartySize = 4, },
                new TimeDto { Time = "20:30:00", MaximumPartySize = 4, },
                new TimeDto { Time = "20:45:00", MaximumPartySize = 4, },
                new TimeDto { Time = "21:00:00", MaximumPartySize = 4, },
                new TimeDto { Time = "21:15:00", MaximumPartySize = 4, },
                new TimeDto { Time = "21:30:00", MaximumPartySize = 4, },
                new TimeDto { Time = "21:45:00", MaximumPartySize = 4, },
                new TimeDto { Time = "22:00:00", MaximumPartySize = 4, },
            };
            Assert.Equal(expected, day.Entries, new TimeDtoComparer());
        }
    }
}
