/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class InMemoryRestaurantDatabase : IRestaurantDatabase
    {
        private readonly Restaurant[] restaurants;

        public InMemoryRestaurantDatabase(params Restaurant[] restaurants)
        {
            this.restaurants = restaurants;
        }

        public Task<IReadOnlyCollection<Restaurant>> GetAll()
        {
            return
                Task.FromResult<IReadOnlyCollection<Restaurant>>(restaurants);
        }

        public Task<Restaurant?> GetRestaurant(int id)
        {
            var restaurant = restaurants.SingleOrDefault(r => r.Id == id);
            return Task.FromResult<Restaurant?>(restaurant);
        }

        public Task<Restaurant?> GetRestaurant(string name)
        {
            var restaurant = restaurants.SingleOrDefault(r => r.Name == name);
            return Task.FromResult<Restaurant?>(restaurant);
        }
    }
}
