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
            Reservation? r = dto.Validate(id);
            if (r is null)
                return new BadRequestResult();

            var restaurant = await RestaurantDatabase
                .GetRestaurant(restaurantId).ConfigureAwait(false);
            if (restaurant is null)
                return new NotFoundResult();

            using var scope = new TransactionScope(
                TransactionScopeAsyncFlowOption.Enabled);
            var reservations = await Repository
                .ReadReservations(restaurantId, r.At)
                .ConfigureAwait(false);
            var now = Clock.GetCurrentDateTime();
            if (!restaurant.MaitreD.WillAccept(now, reservations, r))
                return NoTables500InternalServerError();

            await Repository.Create(restaurantId, r).ConfigureAwait(false);
            await PostOffice.EmailReservationCreated(restaurantId, r)
                .ConfigureAwait(false);
            scope.Complete();

            return Reservation201Created(restaurantId, r);
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

        [SuppressMessage(
            "Usage",
            "CA1801:Review unused parameters",
            Justification = "The restaurantId parameter is required in order to keep the REST API's URLs consistent across all verbs.")]
        [HttpGet("restaurants/{restaurantId}/reservations/{id}")]
        public async Task<ActionResult> Get(int restaurantId, string id)
        {
            if (!Guid.TryParse(id, out var rid))
                return new NotFoundResult();

            Reservation? r =
                await Repository.ReadReservation(rid).ConfigureAwait(false);
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

            Reservation? res = dto.Validate(rid);
            if (res is null)
                return new BadRequestResult();

            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            using var scope = new TransactionScope(
                TransactionScopeAsyncFlowOption.Enabled);

            var existing =
                await Repository.ReadReservation(rid).ConfigureAwait(false);
            if (existing is null)
                return new NotFoundResult();

            var reservations = await Repository
                .ReadReservations(restaurantId, res.At)
                .ConfigureAwait(false);
            reservations = reservations.Where(r => r.Id != res.Id).ToList();
            var now = Clock.GetCurrentDateTime();
            if (!maitreD!.WillAccept(now, reservations, res))
                return NoTables500InternalServerError();

            if (existing.Email != res.Email)
                await PostOffice
                    .EmailReservationUpdating(restaurantId, existing)
                    .ConfigureAwait(false);
            await Repository.Update(res).ConfigureAwait(false);
            await PostOffice.EmailReservationUpdated(restaurantId, res)
                .ConfigureAwait(false);

            scope.Complete();

            return new OkObjectResult(res.ToDto());
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
                var r = await Repository.ReadReservation(rid)
                    .ConfigureAwait(false);
                await Repository.Delete(rid).ConfigureAwait(false);
                if (r is { })
                    await PostOffice.EmailReservationDeleted(restaurantId, r)
                        .ConfigureAwait(false);
            }
        }
    }
}
