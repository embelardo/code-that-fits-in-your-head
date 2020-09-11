/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Restaurant
    {
        public Restaurant(int id, string name, MaitreD maitreD)
        {
            Id = id;
            Name = name;
            MaitreD = maitreD;
        }

        public int Id { get; }
        public string Name { get; }
        public MaitreD MaitreD { get; }

        public Restaurant WithId(int newId)
        {
            return new Restaurant(newId, Name, MaitreD);
        }

        public Restaurant WithName(string newName)
        {
            return new Restaurant(Id, newName, MaitreD);
        }

        public Restaurant WithMaitreD(MaitreD newMaitreD)
        {
            return new Restaurant(Id, Name, newMaitreD);
        }

        public Restaurant Select(Func<MaitreD, MaitreD> selector)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return WithMaitreD(selector(MaitreD));
        }

        public override bool Equals(object? obj)
        {
            return obj is Restaurant restaurant &&
                   Id == restaurant.Id &&
                   Name == restaurant.Name &&
                   Equals(MaitreD, restaurant.MaitreD);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, MaitreD);
        }
    }
}
