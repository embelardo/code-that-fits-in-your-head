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
        public RestaurantsController(IRestaurantDatabase database)
        {
            Database = database;
        }

        public IRestaurantDatabase Database { get; }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var name = await Database.GetName(id).ConfigureAwait(false);

            return new OkObjectResult(new RestaurantDto { Name = name });
        }
    }
}
