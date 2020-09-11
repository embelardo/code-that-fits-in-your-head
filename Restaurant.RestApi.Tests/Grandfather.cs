/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class Grandfather
    {
        internal static int Id =>
            RestApi.Grandfather.Id;

        internal static Restaurant Restaurant =>
            new Restaurant(
                Id,
                "Hipgnosta",
                new MaitreD(
                    Some.MaitreD.OpensAt,
                    Some.MaitreD.LastSeating,
                    Some.MaitreD.SeatingDuration,
                    Table.Communal(10)));
    }
}
