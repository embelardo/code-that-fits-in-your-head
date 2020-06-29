/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.SqlIntegrationTests
{
    [UseDatabase]
    public class SqlReservationsRepositoryTests
    {
        [Theory]
        [InlineData("2022-06-29 12:00", "e@example.gov", "Enigma", 1)]
        [InlineData("2022-07-27 11:40", "c@example.com", "Carlie", 2)]
        public async Task CreateAndReadRoundTrip(
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

            await sut.Create(expected);
            var actual = await sut.ReadReservation(expected.Id);

            Assert.Equal(expected, actual);
        }
    }
}
