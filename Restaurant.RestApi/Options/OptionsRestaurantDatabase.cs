/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Options
{
    public class OptionsRestaurantDatabase : IRestaurantDatabase
    {
        private readonly RestaurantOptions[] restaurants;

        public OptionsRestaurantDatabase(params RestaurantOptions[] restaurants)
        {
            this.restaurants = restaurants;
        }

        public Task<IReadOnlyCollection<Restaurant>> GetAll()
        {
            var list = restaurants
                .Select(r => r.ToRestaurant())
                .OfType<Restaurant>()
                .ToList();
            return Task.FromResult<IReadOnlyCollection<Restaurant>>(list);
        }

        public Task<IEnumerable<string>> GetAllNames()
        {
            return Task.FromResult(
                restaurants.Select(r => r.Name).OfType<string>());
        }

        public Task<Restaurant?> GetRestaurant(int id)
        {
            var restaurant =
                restaurants.Where(r => r.Id == id).SingleOrDefault();

            if (restaurant is null)
                return Task.FromResult<Restaurant?>(null);
            if (restaurant.Name is null)
                return Task.FromResult<Restaurant?>(null);

            return Task.FromResult<Restaurant?>(new Restaurant(
                restaurant.Id,
                restaurant.Name,
                restaurant.ToMaitreD()));
        }

        public Task<Restaurant?> GetRestaurant(string name)
        {
            var restaurant =
                restaurants.Where(r => r.Name == name).SingleOrDefault();

            if (restaurant is null)
                return Task.FromResult<Restaurant?>(null);

            return Task.FromResult<Restaurant?>(new Restaurant(
                restaurant.Id,
                name,
                restaurant.ToMaitreD()));
        }
    }
}
