/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.SqlIntegrationTests
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
    }
}
