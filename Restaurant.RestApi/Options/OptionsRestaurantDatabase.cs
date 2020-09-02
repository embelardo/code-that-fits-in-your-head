/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Options
{
    internal class OptionsRestaurantDatabase : IRestaurantDatabase
    {
        private readonly RestaurantOptions[] restaurants;

        internal OptionsRestaurantDatabase(RestaurantOptions[] restaurants)
        {
            this.restaurants = restaurants;
        }

        public Task<IEnumerable<string>> GetAllNames()
        {
            return Task.FromResult(
                restaurants.Select(r => r.Name).OfType<string>());
        }

        public Task<int?> GetId(string name)
        {
            return Task.FromResult(restaurants
                .Where(r => r.Name == name)
                .Select(r => (int?)r.Id)
                .SingleOrDefault());
        }

        public Task<string?> GetName(int id)
        {
            return Task.FromResult(restaurants
                .Where(r => r.Id == id)
                .Select(r => r.Name)
                .SingleOrDefault());
        }
    }
}
