/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    [Route("")]
    public sealed class HomeController
    {
        public HomeController(IRestaurantDatabase database)
        {
            Database = database;
        }

        public IRestaurantDatabase Database { get; }

        public async Task<ActionResult> Get()
        {
            var rs = await Database.GetAll().ConfigureAwait(false);
            var restaurants = rs
                .Select(r => new RestaurantDto { Name = r.Name })
                .ToArray();

            return new OkObjectResult(
                new HomeDto { Restaurants = restaurants });
        }
    }
}
