/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public interface IRestaurantDatabase
    {
        Task<string?> GetName(int id);
        Task<int?> GetId(string name);
        Task<IEnumerable<string>> GetAllNames();
        Task<MaitreD?> GetMaitreD(int id);
    }
}
