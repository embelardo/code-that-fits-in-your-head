/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Ploeh.Samples.Restaurants.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class Create
    {
        internal static ReservationDto ReservationDto(
            string at,
            string email,
            string name,
            int quantity)
        {
            return new ReservationDto
            {
                At = at,
                Email = email,
                Name = name,
                Quantity = quantity
            };
        }

        internal static SmtpOptions SmtpOptions(
            string host,
            int port,
            string userName,
            string password,
            string fromAddress)
        {
            return new SmtpOptions
            {
                Host = host,
                Port = port,
                UserName = userName,
                Password = password,
                FromAddress = fromAddress
            };
        }
    }
}
