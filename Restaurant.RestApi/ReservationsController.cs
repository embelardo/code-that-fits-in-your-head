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

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("reservations")]
    public class ReservationsController
    {
        public ReservationsController(
            IRestaurantDatabase restaurantDatabase,
            IReservationsRepository repository,
            IPostOffice postOffice,
            MaitreD maitreD)
        {
            RestaurantDatabase = restaurantDatabase;
            Repository = repository;
            PostOffice = postOffice;
            MaitreD = maitreD;
        }

        public IRestaurantDatabase RestaurantDatabase { get; }
        public IReservationsRepository Repository { get; }
        public IPostOffice PostOffice { get; }
        public MaitreD MaitreD { get; }

        [HttpPost]
        public Task<ActionResult> Post(ReservationDto dto)
        {
            return Post(Grandfather.Id, dto);
        }

        [HttpPost("{restaurantId}")]
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

            var maitreD = await RestaurantDatabase.GetMaitreD(restaurantId)
                .ConfigureAwait(false);
            if (maitreD is null)
                return new NotFoundResult();

            using var scope = new TransactionScope(
                TransactionScopeAsyncFlowOption.Enabled);
            var reservations = await Repository
                .ReadReservations(restaurantId, r.At)
                .ConfigureAwait(false);
            if (!maitreD.WillAccept(DateTime.Now, reservations, r))
                return NoTables500InternalServerError();

            await Repository.Create(restaurantId, r).ConfigureAwait(false);
            await PostOffice.EmailReservationCreated(r).ConfigureAwait(false);
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

        [HttpGet("{id}")]
        public Task<ActionResult> Get(string id)
        {
            return Get(Grandfather.Id, id);
        }

        [SuppressMessage(
            "Usage",
            "CA1801:Review unused parameters",
            Justification = "The restaurantId parameter is required in order to keep the REST API's URLs consistent across all verbs.")]
        [HttpGet("{restaurantId}/{id}")]
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

        [HttpPut("{id}")]
        public Task<ActionResult> Put(string id, ReservationDto dto)
        {
            return Put(Grandfather.Id, id, dto);
        }

        [HttpPut("{restaurantId}/{id}")]
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
            if (!maitreD!.WillAccept(DateTime.Now, reservations, res))
                return NoTables500InternalServerError();

            if (existing.Email != res.Email)
                await PostOffice.EmailReservationUpdating(existing)
                    .ConfigureAwait(false);
            await Repository.Update(res).ConfigureAwait(false);
            await PostOffice.EmailReservationUpdated(res).ConfigureAwait(false);

            scope.Complete();

            return new OkObjectResult(res.ToDto());
        }

        [HttpDelete("{id}")]
        public Task Delete(string id)
        {
            return Delete(Grandfather.Id, id);
        }

        [SuppressMessage(
            "Usage",
            "CA1801:Review unused parameters",
            Justification = "The restaurantId parameter is required in order to keep the REST API's URLs consistent across all verbs.")]
        [HttpDelete("{restaurantId}/{id}")]
        public async Task Delete(int restaurantId, string id)
        {
            if (Guid.TryParse(id, out var rid))
            {
                var r = await Repository.ReadReservation(rid)
                    .ConfigureAwait(false);
                await Repository.Delete(rid).ConfigureAwait(false);
                if (r is { })
                    await PostOffice.EmailReservationDeleted(r)
                        .ConfigureAwait(false);
            }
        }
    }
}
