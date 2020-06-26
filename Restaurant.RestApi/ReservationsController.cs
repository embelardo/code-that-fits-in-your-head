/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("reservations")]
    public class ReservationsController
    {
        public ReservationsController(
            IReservationsRepository repository,
            IPostOffice postOffice,
            MaitreD maitreD)
        {
            Repository = repository;
            PostOffice = postOffice;
            MaitreD = maitreD;
        }

        public IReservationsRepository Repository { get; }
        public IPostOffice PostOffice { get; }
        public MaitreD MaitreD { get; }

        [HttpPost]
        public async Task<ActionResult> Post(ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            var id = dto.ParseId() ?? Guid.NewGuid();
            Reservation? r = dto.Validate(id);
            if (r is null)
                return new BadRequestResult();

            var reservations = await Repository
                .ReadReservations(r.At)
                .ConfigureAwait(false);
            if (!MaitreD.WillAccept(DateTime.Now, reservations, r))
                return NoTables500InternalServerError();

            await Repository.Create(r).ConfigureAwait(false);
            await PostOffice.EmailReservationCreated(r).ConfigureAwait(false);

            return Reservation201Created(r);
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
        private static ActionResult Reservation201Created(Reservation r)
        {
            return new CreatedAtActionResult(
                nameof(Get),
                null,
                new { id = r.Id.ToString("N") },
                null);
        }

        [SuppressMessage(
            "Globalization",
            "CA1305:Specify IFormatProvider",
            Justification = "ToString(\"o\") is already culture-neutral.")]
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (!Guid.TryParse(id, out var rid))
                return new NotFoundResult();

            Reservation? r =
                await Repository.ReadReservation(rid).ConfigureAwait(false);
            if (r is null)
                return new NotFoundResult();

            return new OkObjectResult(
                new ReservationDto
                {
                    Id = id,
                    At = r.At.ToString("o"),
                    Email = r.Email,
                    Name = r.Name,
                    Quantity = r.Quantity
                });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));
            if (!Guid.TryParse(id, out var rid))
                return new NotFoundResult();

            Reservation? res = dto.Validate(rid);
            if (res is null)
                return new BadRequestResult();

            var existing =
                await Repository.ReadReservation(rid).ConfigureAwait(false);
            if (existing is null)
                return new NotFoundResult();

            var reservations = await Repository
                .ReadReservations(res.At)
                .ConfigureAwait(false);
            reservations = reservations.Where(r => r.Id != res.Id).ToList();
            if (!MaitreD.WillAccept(DateTime.Now, reservations, res))
                return NoTables500InternalServerError();

            await Repository.Update(res).ConfigureAwait(false);

            return new OkResult();
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
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
