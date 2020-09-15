/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class Unit
    {
        public readonly static Unit Instance = new Unit();

        private Unit()
        {
        }
    }
}