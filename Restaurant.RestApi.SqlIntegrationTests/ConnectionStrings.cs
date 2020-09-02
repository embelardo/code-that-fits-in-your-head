/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.SqlIntegrationTests
{
    public static class ConnectionStrings
    {
        public const string Reservations =
            @"Server=(LocalDB)\MSSQLLocalDB;Database=RestaurantIntegrationTest;Integrated Security=true";
    }
}
