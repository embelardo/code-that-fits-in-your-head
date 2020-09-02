/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.SqlIntegrationTests
{
    [UseDatabase]
    public class ConcurrencyTests
    {
        [Fact]
        public async Task NoOverbookingRace()
        {
            var start = DateTimeOffset.UtcNow;
            var timeOut = TimeSpan.FromSeconds(30);
            var i = 0;
            while (DateTimeOffset.UtcNow - start < timeOut)
                await PostTwoConcurrentLiminalReservations(
                    start.DateTime.AddDays(++i));
        }

        private static async Task PostTwoConcurrentLiminalReservations(
            DateTime date)
        {
            date = date.Date.AddHours(18.5);
            using var service = new RestaurantService();
            await service.PostReservation(date, 9);

            var task1 = service.PostReservation(new ReservationDtoBuilder()
                .WithDate(date)
                .WithQuantity(1)
                .Build());
            var task2 = service.PostReservation(new ReservationDtoBuilder()
                .WithDate(date)
                .WithQuantity(1)
                .Build());
            var actual = await Task.WhenAll(task1, task2);

            Assert.Single(actual, msg => msg.IsSuccessStatusCode);
            Assert.Single(
                actual,
                msg => msg.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task NoOverbookingPutRace()
        {
            var start = DateTimeOffset.UtcNow;
            var timeOut = TimeSpan.FromSeconds(30);
            var i = 0;
            while (DateTimeOffset.UtcNow - start < timeOut)
                await PutTwoConcurrentLiminalReservations(
                    start.DateTime.AddDays(++i));
        }

        private static async Task PutTwoConcurrentLiminalReservations(
            DateTime date)
        {
            date = date.Date.AddHours(18.5);
            using var service = new RestaurantService();
            var (address1, dto1) = await service.PostReservation(date, 4);
            var (address2, dto2) = await service.PostReservation(date, 4);

            dto1.Quantity += 2;
            dto2.Quantity += 2;
            var task1 = service.PutReservation(address1, dto1);
            var task2 = service.PutReservation(address2, dto2);
            var actual = await Task.WhenAll(task1, task2);

            Assert.Single(actual, msg => msg.IsSuccessStatusCode);
            Assert.Single(
                actual,
                msg => msg.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
