/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Mvc;
using Ploeh.Samples.Restaurants.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class CalendarTests
    {
        [Fact]
        public async Task GetCurrentYear()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetCurrentYear();
            var after = DateTime.Now;

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
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

            response.EnsureSuccessStatusCode();
            var actual = await response.ParseJsonContent<CalendarDto>();
            var beforeBefore = before.AddDays(-1);
            var beforeAfter = after.AddDays(-1);
            AssertOneOf(beforeBefore.Year, beforeAfter.Year, actual.Year);
            AssertOneOf(beforeBefore.Month, beforeAfter.Month, actual.Month);
            AssertOneOf(beforeBefore.Day, beforeAfter.Day, actual.Day);
        }

        [Fact]
        public async Task GetNextDay()
        {
            using var api = new LegacyApi();

            var before = DateTime.Now;
            var response = await api.GetNextDay();
            var after = DateTime.Now;

            response.EnsureSuccessStatusCode();
            var actual = await response.ParseJsonContent<CalendarDto>();
            var afterBefore = before.AddDays(1);
            var afterAfter = after.AddDays(1);
            AssertOneOf(afterBefore.Year, afterAfter.Year, actual.Year);
            AssertOneOf(afterBefore.Month, afterAfter.Month, actual.Month);
            AssertOneOf(afterBefore.Day, afterAfter.Day, actual.Day);
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

            response.EnsureSuccessStatusCode();
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
                    sut => sut.Get(Grandfather.Id, year),
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
                    sut => sut.Get(Grandfather.Id, year, month),
                    year,
                    month,
                    null,
                    expectedDays,
                    tableSize);
            }

            private void AddDay(int year, int month, int day, int tableSize)
            {
                Add(
                    sut => sut.Get(Grandfather.Id, year, month, day),
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
            var restaurant = from m in Grandfather.Restaurant
                             select m.WithTables(Table.Communal(tableSize));
            var sut = new CalendarController(
                new InMemoryRestaurantDatabase(restaurant),
                new FakeDatabase());

            var actual = await act(sut);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            Assert.Equal(year, dto.Year);
            Assert.Equal(month, dto.Month);
            Assert.Equal(day, dto.Day);
            AssertDays(expectedDays, dto);
            AssertEntries(expectedDays, tableSize, restaurant, dto);
        }

        private static void AssertDays(int expectedDays, CalendarDto dto)
        {
            Assert.NotNull(dto.Days);
            Assert.Equal(expectedDays, dto.Days?.Length);
            Assert.Equal(
                expectedDays,
                dto.Days.Select(d => d.Date).Distinct().Count());
        }

        private static void AssertEntries(
            int expectedDays,
            int tableSize,
            Restaurant restaurant,
            CalendarDto dto)
        {
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
            var opensAt = restaurant.MaitreD.OpensAt.ToIso8601TimeString();
            Assert.All(dto.Days, d => AssertContainsTime(opensAt, d));
            var lastSeating =
                restaurant.MaitreD.LastSeating.ToIso8601TimeString();
            Assert.All(dto.Days, d => AssertContainsTime(lastSeating, d));
        }

        private static void AssertContainsTime(string time, DayDto d)
        {
            Assert.Contains(time, d.Entries.Select(e => e.Time));
        }

        [Theory]
        [InlineData( 1)]
        [InlineData( 2)]
        [InlineData(99)]
        public async Task ViewCalendarForDayWithReservation(int restaurantId)
        {
            var db = new FakeDatabase();
            await db.Create(
                restaurantId,
                Some.Reservation
                    .WithQuantity(3)
                    .WithDate(new DateTime(2020, 8, 21, 19, 0, 0)));
            var sut = new CalendarController(
                new InMemoryRestaurantDatabase(
                    Some.Restaurant.WithId(restaurantId).Select(m => m
                        .WithOpensAt(TimeSpan.FromHours(18))
                        .WithLastSeating(TimeSpan.FromHours(20))
                        .WithSeatingDuration(TimeSpan.FromHours(.75))
                        .WithTables(Table.Standard(4)))),
                db);

            var actual = await sut.Get(restaurantId, 2020, 8, 21);

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

        [Theory]
        [InlineData( 4)]
        [InlineData(10)]
        [InlineData(87)]
        public async Task ViewCalendarForMonthWithReservation(int restaurantId)
        {
            var db = new FakeDatabase();
            await db.Create(
                restaurantId,
                Some.Reservation
                    .WithQuantity(3)
                    .WithDate(new DateTime(2020, 8, 22, 20, 30, 0)));
            var sut = new CalendarController(
                new InMemoryRestaurantDatabase(
                    Some.Restaurant.WithId(restaurantId).Select(m => m
                        .WithOpensAt(TimeSpan.FromHours(20))
                        .WithLastSeating(TimeSpan.FromHours(22))
                        .WithSeatingDuration(TimeSpan.FromHours(1))
                        .WithTables(Table.Communal(12)))),
                db);

            var actual = await sut.Get(restaurantId, 2020, 8);

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

        [Theory]
        [InlineData( 8)]
        [InlineData(20)]
        [InlineData(21)]
        public async Task ViewCalendarForYearWithReservation(int restaurantId)
        {
            var db = new FakeDatabase();
            await db.Create(
                restaurantId,
                Some.Reservation
                    .WithQuantity(5)
                    .WithDate(new DateTime(2020, 9, 23, 20, 15, 0)));
            var sut = new CalendarController(
                new InMemoryRestaurantDatabase(
                    Some.Restaurant.WithId(restaurantId).Select(m => m
                        .WithOpensAt(TimeSpan.FromHours(18.5))
                        .WithLastSeating(TimeSpan.FromHours(22))
                        .WithSeatingDuration(TimeSpan.FromHours(2))
                        .WithTables(Table.Standard(4), Table.Standard(6)))),
                db);

            var actual = await sut.Get(restaurantId, 2020);

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

        [Theory]
        [InlineData( 2, "11:30:00")]
        [InlineData( 3, "18:00:00")]
        [InlineData(28, "20:00:00")]
        public async Task ViewCalendarForSpecificRestaurant(
            int restaurantId,
            string opensAt)
        {
            var restaurant = from m in Some.Restaurant.WithId(restaurantId)
                             select m.WithOpensAt(TimeSpan.Parse(
                                 opensAt,
                                 CultureInfo.InvariantCulture));
            var restaurantDB = new InMemoryRestaurantDatabase(restaurant);
            var db = new FakeDatabase();
            var sut = new CalendarController(restaurantDB, db);

            var actual = await sut.Get(restaurantId, 2020, 9, 5);

            var ok = Assert.IsAssignableFrom<OkObjectResult>(actual);
            var dto = Assert.IsAssignableFrom<CalendarDto>(ok.Value);
            var day = Assert.Single(dto.Days);
            Assert.Equal(opensAt, day.Entries.First().Time);
        }

        [Fact]
        public async Task ViewYearForAbsentRestaurant()
        {
            var absentRestaurantId = 4;
            var restaurantDB = new InMemoryRestaurantDatabase(Some.Restaurant);
            var db = new FakeDatabase();
            var sut = new CalendarController(restaurantDB, db);
            var r = await restaurantDB.GetRestaurant(absentRestaurantId);
            Assert.Null(r);

            var actual = await sut.Get(absentRestaurantId, 2029);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Fact]
        public async Task ViewMonthForAbsentRestaurant()
        {
            var absentRestaurantId = 4;
            var restaurantDB = new InMemoryRestaurantDatabase(Some.Restaurant);
            var db = new FakeDatabase();
            var sut = new CalendarController(restaurantDB, db);
            var r = await restaurantDB.GetRestaurant(absentRestaurantId);
            Assert.Null(r);

            var actual = await sut.Get(absentRestaurantId, 1999, 12);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Fact]
        public async Task ViewDayForAbsentRestaurant()
        {
            var absentRestaurantId = 4;
            var restaurantDB = new InMemoryRestaurantDatabase(Some.Restaurant);
            var db = new FakeDatabase();
            var sut = new CalendarController(restaurantDB, db);
            var r = await restaurantDB.GetRestaurant(absentRestaurantId);
            Assert.Null(r);

            var actual = await sut.Get(absentRestaurantId, 2101, 2, 28);

            Assert.IsAssignableFrom<NotFoundResult>(actual);
        }

        [Theory]
        [InlineData("http://localhost/calendar/2020?sig=ePBoUg5gDw2RKMVWz8KIVzF%2Fgq74RL6ynECiPpDwVks%3D")]
        [InlineData("http://localhost/calendar/2020/9?sig=ZgxaZqg5ubDp0Z7IUx4dkqTzS%2Fyjv6veDUc2swdysDU%3D")]
        [InlineData("http://localhost/calendar/2020/9/8?sig=K%2FSVCXwk5LN1Ph0igx3AV6d3P56q7IVqCPmRZTjQL94%3D")]
        public async Task BookmarksStillWork(string bookmarkedAddress)
        {
            using var api = new LegacyApi();

            var actual = await api.CreateDefaultClient()
                .GetAsync(new Uri(bookmarkedAddress));

            Assert.Equal(HttpStatusCode.MovedPermanently, actual.StatusCode);
            var follow =
                await api.CreateClient().GetAsync(actual.Headers.Location);
            follow.EnsureSuccessStatusCode();
        }
    }
}
