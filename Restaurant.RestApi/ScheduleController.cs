/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("schedule")]
    public class ScheduleController
    {
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1801 // Review unused parameters
        [HttpGet("{year}/{month}/{day}")]
        public ActionResult Get(int year, int month, int day)
#pragma warning restore CA1801 // Review unused parameters
#pragma warning restore CA1822 // Mark members as static
        {
            return new UnauthorizedResult();
        }
    }
}
