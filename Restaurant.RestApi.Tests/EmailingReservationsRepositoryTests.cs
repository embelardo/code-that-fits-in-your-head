/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class EmailingReservationsRepositoryTests
    {
        [Theory]
        [InlineData( 3)]
        [InlineData(12)]
        public async Task DeleteSendsEmail(int restaurantId)
        {
            var r = Some.Reservation;
            var db = new FakeDatabase();
            await db.Create(restaurantId, r);
            var postOffice = new SpyPostOffice();
            var sut = new EmailingReservationsRepository(
                postOffice,
                db);

            await sut.Delete(restaurantId, r.Id);

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Deleted,
                restaurantId,
                r);
            Assert.Contains(expected, postOffice);
            Assert.DoesNotContain(r, db[restaurantId]);
        }

        [Theory]
        [InlineData(19)]
        [InlineData(32)]
        public async Task CreateSendsEmail(int restaurantId)
        {
            var postOffice = new SpyPostOffice();
            var db = new FakeDatabase();
            var sut = new EmailingReservationsRepository(postOffice, db);

            var r = Some.Reservation;
            await sut.Create(restaurantId, r);

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Created,
                restaurantId,
                r);
            Assert.Contains(expected, postOffice);
            Assert.Contains(r, db[restaurantId]);
        }

        [Theory]
        [InlineData(32, "David")]
        [InlineData(58, "Robert")]
        [InlineData(58, "Jones")]
        public async Task UpdateSendsEmail(int restaurantId, string newName)
        {
            var postOffice = new SpyPostOffice();
            var existing = Some.Reservation;
            var db = new FakeDatabase();
            await db.Create(restaurantId, existing);
            var sut = new EmailingReservationsRepository(postOffice, db);

            var updated = existing.WithName(new Name(newName));
            await sut.Update(restaurantId, updated);

            var expected = new SpyPostOffice.Observation(
                SpyPostOffice.Event.Updated,
                restaurantId,
                updated);
            Assert.Contains(updated, db[restaurantId]);
            Assert.Contains(expected, postOffice);
            Assert.DoesNotContain(
                postOffice,
                o => o.Event == SpyPostOffice.Event.Updating);
        }

        [Theory]
        [InlineData(3, "foo@example.com")]
        [InlineData(6, "bar@example.gov")]
        public async Task UpdateSendsEmailToOldAddressOnChange(
            int restaurantId,
            string newEmail)
        {
            var postOffice = new SpyPostOffice();
            var existing = Some.Reservation;
            var db = new FakeDatabase();
            await db.Create(restaurantId, existing);
            var sut = new EmailingReservationsRepository(postOffice, db);

            var updated = existing.WithEmail(new Email(newEmail));
            await sut.Update(restaurantId, updated);

            var expected = new[] {
                new SpyPostOffice.Observation(
                    SpyPostOffice.Event.Updating,
                    restaurantId,
                    existing),
                new SpyPostOffice.Observation(
                    SpyPostOffice.Event.Updated,
                    restaurantId,
                    updated) }.ToHashSet();
            Assert.Superset(expected, postOffice.ToHashSet());
        }
    }
}
