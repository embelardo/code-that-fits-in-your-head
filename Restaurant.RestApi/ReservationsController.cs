/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("[controller]")]
    public class ReservationsController
    {
#pragma warning disable CA1822 // Mark members as static
        public void Post() { }
#pragma warning restore CA1822 // Mark members as static
    }
}
