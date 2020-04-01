/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("[controller]")]
    public class ReservationsController
    {
        public ReservationsController(IReservationsRepository repository)
        {
            Repository = repository;
        }

        public IReservationsRepository Repository { get; }

        public async Task<ActionResult> Post(ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));
            if (!dto.IsValid)
                return new BadRequestResult();

            var d = DateTime.Parse(dto.At!, CultureInfo.InvariantCulture);

            var reservations =
                await Repository.ReadReservations(d).ConfigureAwait(false);
            int reservedSeats = reservations.Sum(r => r.Quantity);
            if (10 < reservedSeats + dto.Quantity)
                return new StatusCodeResult(
                    StatusCodes.Status500InternalServerError);

            var r =
                new Reservation(d, dto.Email!, dto.Name ?? "", dto.Quantity);
            await Repository.Create(r).ConfigureAwait(false);

            return new NoContentResult();
        }
    }
}
