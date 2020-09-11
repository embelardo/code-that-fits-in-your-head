/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Ploeh.Samples.Restaurants.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public static class Some
    {
        public readonly static DateTime Now =
            new DateTime(2022, 4, 1, 20, 15, 0);

        public readonly static Reservation Reservation =
            new Reservation(
                new Guid(0x81416928, 0xC236, 0x4EBF, 0xA4, 0x50, 0x24, 0x95, 0xA4, 0xDA, 0x92, 0x30),
                Now,
                new Email("x@example.net"),
                new Name(""),
                1);

        public readonly static MaitreD MaitreD =
            new MaitreD(
                TimeSpan.FromHours(16),
                TimeSpan.FromHours(21),
                TimeSpan.FromHours(12),
                Table.Communal(10));

        public readonly static Restaurant Restaurant =
            new Restaurant(
                id: 9, // Not the grandfather ID
                name: "Foo",
                new MaitreD(
                    TimeSpan.FromHours(12),
                    TimeSpan.FromHours(22),
                    TimeSpan.FromHours(2.5),
                    Table.Standard(1)));
    }
}
