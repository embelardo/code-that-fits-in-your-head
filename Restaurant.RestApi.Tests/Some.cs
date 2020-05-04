/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public static class Some
    {
        public readonly static Reservation Reservation =
            new Reservation(
                new DateTime(2022, 4, 1, 20, 15, 0),
                "x@example.net",
                "",
                1);

        public readonly static MaitreD MaitreD =
            new MaitreD(
                TimeSpan.FromHours(16),
                TimeSpan.FromHours(12),
                Table.Communal(10));
    }
}
