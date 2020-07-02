/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [ApiController, Route("calendar")]
    public class CalendarController
    {
        [HttpGet("{year}")]
        public ActionResult Get(int year)
        {
            return new OkObjectResult(
                new CalendarDto { Year = year });
        }
    }
}
