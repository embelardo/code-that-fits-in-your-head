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
        private readonly MaitreD maitreD;

        public ReservationsController(IReservationsRepository repository)
        {
            Repository = repository;
            maitreD = new MaitreD(new Table(TableType.Communal, 10));
        }

        public IReservationsRepository Repository { get; }

        public async Task<ActionResult> Post(ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            Reservation? r = dto.Validate();
            if (r is null)
                return new BadRequestResult();

            var reservations = await Repository
                .ReadReservations(r.At)
                .ConfigureAwait(false);
            if (!maitreD.WillAccept(reservations, r))
                return new StatusCodeResult(
                    StatusCodes.Status500InternalServerError);

            await Repository.Create(r).ConfigureAwait(false);

            return new NoContentResult();
        }
    }
}
