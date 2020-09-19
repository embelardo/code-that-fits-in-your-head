/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [ApiController]
    public class ReservationsController
    {
        public ReservationsController(
            IClock clock,
            IRestaurantDatabase restaurantDatabase,
            IReservationsRepository repository,
            IPostOffice postOffice)
        {
            Clock = clock;
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
            PostOffice = postOffice;
        }

        public IClock Clock { get; }
        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }
        public IPostOffice PostOffice { get; }

        [HttpPost("reservations")]
        public Task<ActionResult> Post(ReservationDto dto)
        {
            return Post(Grandfather.Id, dto);
        }

        [HttpPost("restaurants/{restaurantId}/reservations")]
        public async Task<ActionResult> Post(
            int restaurantId,
            ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            var id = dto.ParseId() ?? Guid.NewGuid();
            Reservation? reservation = dto.Validate(id);
            if (reservation is null)
                return new BadRequestResult();

            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            return await TryCreate(restaurant, reservation)
                .ConfigureAwait(false);
        }

        private async Task<ActionResult> TryCreate(
            Restaurant restaurant,
            Reservation reservation)
        {
            using var scope =
                new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var reservations = await Repository
                .ReadReservations(restaurant.Id, reservation.At)
                .ConfigureAwait(false);
            var now = Clock.GetCurrentDateTime();
            if (!restaurant.MaitreD.WillAccept(now, reservations, reservation))
                return NoTables500InternalServerError();

            await Repository.Create(restaurant.Id, reservation)
                .ConfigureAwait(false);
            await PostOffice
                .EmailReservationCreated(restaurant.Id, reservation)
                .ConfigureAwait(false);

            scope.Complete();

            return Reservation201Created(restaurant.Id, reservation);
        }

        private static ActionResult NoTables500InternalServerError()
        {
            return new ObjectResult("No tables available.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        [SuppressMessage(
            "Globalization",
            "CA1305:Specify IFormatProvider",
            Justification = "Guids aren't culture-specific.")]
        private static ActionResult Reservation201Created(
            int restaurantId,
            Reservation r)
        {
            return new CreatedAtActionResult(
                nameof(Get),
                null,
                new { restaurantId, id = r.Id.ToString("N") },
                r.ToDto());
        }

        [HttpGet("reservations/{id}")]
        public Task<ActionResult> Get(string id)
        {
            return Get(Grandfather.Id, id);
        }

        [HttpGet("restaurants/{restaurantId}/reservations/{id}")]
        public async Task<ActionResult> Get(int restaurantId, string id)
        {
            if (!Guid.TryParse(id, out var rid))
                return new NotFoundResult();

            Reservation? r = await Repository
                .ReadReservation(restaurantId, rid).ConfigureAwait(false);
            if (r is null)
                return new NotFoundResult();

            return new OkObjectResult(r.ToDto());
        }

        [HttpPut("reservations/{id}")]
        public Task<ActionResult> Put(string id, ReservationDto dto)
        {
            return Put(Grandfather.Id, id, dto);
        }

        [HttpPut("restaurants/{restaurantId}/reservations/{id}")]
        public async Task<ActionResult> Put(
            int restaurantId,
            string id,
            ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));
            if (!Guid.TryParse(id, out var rid))
                return new NotFoundResult();

            Reservation? reservation = dto.Validate(rid);
            if (reservation is null)
                return new BadRequestResult();

            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            return
                await TryUpdate(restaurant, reservation).ConfigureAwait(false);
        }

        private async Task<ActionResult> TryUpdate(
            Restaurant restaurant, Reservation reservation)
        {
            using var scope = new TransactionScope(
                TransactionScopeAsyncFlowOption.Enabled);

            var existing = await Repository
                .ReadReservation(restaurant.Id, reservation.Id)
                .ConfigureAwait(false);
            if (existing is null)
                return new NotFoundResult();

            var ok = await WillAcceptUpdate(restaurant, reservation)
                .ConfigureAwait(false);
            if (!ok)
                return NoTables500InternalServerError();

            await Update(restaurant, reservation, existing)
                .ConfigureAwait(false);

            scope.Complete();

            return new OkObjectResult(reservation.ToDto());
        }

        private async Task<bool> WillAcceptUpdate(
            Restaurant restaurant,
            Reservation reservation)
        {
            var reservations = await Repository
                .ReadReservations(restaurant.Id, reservation.At)
                .ConfigureAwait(false);
            reservations =
                reservations.Where(r => r.Id != reservation.Id).ToList();
            var now = Clock.GetCurrentDateTime();
            return restaurant.MaitreD.WillAccept(
                now,
                reservations,
                reservation);
        }

        private async Task Update(
            Restaurant restaurant,
            Reservation reservation,
            Reservation existing)
        {
            if (existing.Email != reservation.Email)
                await PostOffice
                    .EmailReservationUpdating(restaurant.Id, existing)
                    .ConfigureAwait(false);
            await Repository.Update(restaurant.Id, reservation)
                .ConfigureAwait(false);
            await PostOffice
                .EmailReservationUpdated(restaurant.Id, reservation)
                .ConfigureAwait(false);
        }

        [HttpDelete("reservations/{id}")]
        public Task Delete(string id)
        {
            return Delete(Grandfather.Id, id);
        }

        [HttpDelete("restaurants/{restaurantId}/reservations/{id}")]
        public async Task Delete(int restaurantId, string id)
        {
            if (Guid.TryParse(id, out var rid))
            {
                var r = await Repository.ReadReservation(restaurantId, rid)
                    .ConfigureAwait(false);
                await
                    Repository.Delete(restaurantId, rid).ConfigureAwait(false);
                if (r is { })
                    await PostOffice.EmailReservationDeleted(restaurantId, r)
                        .ConfigureAwait(false);
            }
        }
    }
}
