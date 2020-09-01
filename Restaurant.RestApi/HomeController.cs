/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("")]
    public sealed class HomeController
    {
        [SuppressMessage(
            "Performance",
            "CA1822:Mark members as static",
            Justification = "Controller methods must be instance methods.")]
        public ActionResult Get()
        {
            return new OkObjectResult(
                new HomeDto
                {
                    Restaurants = new[] {new RestaurantDto { } }
                });
        }
    }
}
