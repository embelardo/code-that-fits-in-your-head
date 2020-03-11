/* Copyright (c) Mark Seemann 2020. All rights reserved. */
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

        public async Task Post(ReservationDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            await Repository
                .Create(
                    new Reservation(
                        new DateTime(2023, 11, 24, 19, 0, 0),
                        "juliad@example.net",
                        "Julia Domna",
                        5))
                .ConfigureAwait(false);
        }
    }
}
