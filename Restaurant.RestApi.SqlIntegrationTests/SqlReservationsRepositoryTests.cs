/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.SqlIntegrationTests
{
    [UseDatabase]
    public class SqlReservationsRepositoryTests
    {
        [Theory]
        [InlineData(
            Grandfather.Id, "2022-06-29 12:00", "e@example.gov", "Enigma", 1)]
        [InlineData(
            Grandfather.Id, "2022-07-27 11:40", "c@example.com", "Carlie", 2)]
        [InlineData(2, "2021-09-03 14:32", "bon@example.edu", "Jovi", 4)]
        public async Task CreateAndReadRoundTrip(
            int restaurantId,
            string date,
            string email,
            string name,
            int quantity)
        {
            var expected = new Reservation(
                Guid.NewGuid(),
                DateTime.Parse(date, CultureInfo.InvariantCulture),
                new Email(email),
                new Name(name),
                quantity);
            var connectionString = ConnectionStrings.Reservations;
            var sut = new SqlReservationsRepository(connectionString);

            await sut.Create(restaurantId, expected);
            var actual = await sut.ReadReservation(restaurantId, expected.Id);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(
            Grandfather.Id, "2032-01-01 01:12", "z@example.net", "z", "Zet", 4)]
        [InlineData(
            Grandfather.Id, "2084-04-21 23:21", "q@example.gov", "q", "Quu", 9)]
        [InlineData(4, "2098-10-21 00:12", "rl@example.org", "Gr", "Grime", 2)]
        public async Task PutAndReadRoundTrip(
            int restaurantId,
            string date,
            string email,
            string name,
            string newName,
            int quantity)
        {
            var r = new Reservation(
                Guid.NewGuid(),
                DateTime.Parse(date, CultureInfo.InvariantCulture),
                new Email(email),
                new Name(name),
                quantity);
            var connectionString = ConnectionStrings.Reservations;
            var sut = new SqlReservationsRepository(connectionString);
            await sut.Create(restaurantId, r);

            var expected = r.WithName(new Name(newName));
            await sut.Update(restaurantId, expected);
            var actual = await sut.ReadReservation(restaurantId, expected.Id);

            Assert.Equal(expected, actual);
        }
    }
}
