/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public interface IRestaurantDatabase
    {
        Task<IReadOnlyCollection<Restaurant>> GetAll();
        Task<Restaurant?> GetRestaurant(int id);
        Task<Restaurant?> GetRestaurant(string name);
    }
}
