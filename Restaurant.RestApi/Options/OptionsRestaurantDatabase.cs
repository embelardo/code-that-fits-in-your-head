/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Options
{
    public class OptionsRestaurantDatabase : IRestaurantDatabase
    {
        public Task<IEnumerable<string>> GetAllNames()
        {
            return Task.FromResult(
                new[] { "Hipgnosta", "Nono", "The Vatican Cellar" }
                .AsEnumerable());
        }

        public Task<string?> GetName(int id)
        {
            var name = "Hipgnosta";
            if (id == 4)
                name = "Nono";
            if (id == 18)
                name = "The Vatican Cellar";

            return Task.FromResult((string?)name);
        }
    }
}
