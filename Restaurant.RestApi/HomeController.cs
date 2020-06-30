/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok(new HomeDto { Links = new[]
            {
                CreateReservationsLink()
            } });
        }

        private LinkDto CreateReservationsLink()
        {
            var controllerName = nameof(ReservationsController);
            var controller = controllerName.Remove(
                controllerName.LastIndexOf(
                    "Controller",
                    StringComparison.Ordinal));

            var href = Url.Action(
                nameof(ReservationsController.Post),
                controller,
                null,
                Request.Scheme,
                Request.Host.ToUriComponent());

            return new LinkDto
            {
                Rel = "urn:reservations",
                Href = href
            };
        }
    }
}
