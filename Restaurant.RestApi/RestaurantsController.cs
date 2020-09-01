/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("restaurants")]
    public sealed class RestaurantsController
    {
        [HttpGet("{id}")]
#pragma warning disable CA1822 // Mark members as static
        public ActionResult Get(int id)
#pragma warning restore CA1822 // Mark members as static
        {
            var name = "Hipgnosta";
            if (id == 4)
                name = "Nono";
            if (id == 18)
                name = "The Vatican Cellar";

            return new OkObjectResult(new RestaurantDto { Name = name });
        }
    }
}
