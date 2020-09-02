/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    /// <summary>
    /// An access control list for restaurants.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is, perhaps, a bit of a stretch to call this an access control list
    /// in the classic file-descriptor sense of the term. It does, however,
    /// provide a list of restaurant IDs that a particular user can access.
    /// </para>
    /// </remarks>
    public sealed class AccessControlList
    {
        private readonly IReadOnlyCollection<int> restaurantIds;

        public AccessControlList(IReadOnlyCollection<int> restaurantIds)
        {
            this.restaurantIds = restaurantIds;
        }

        public AccessControlList(params int[] restaurantIds) :
            this(restaurantIds.ToList())
        {
        }

        internal bool Authorize(int restaurantId)
        {
            return restaurantIds.Contains(restaurantId);
        }

        internal static AccessControlList FromUser(ClaimsPrincipal user)
        {
            var restaurantIds = user
                .FindAll("restaurant")
                .SelectMany(c => ClaimToRestaurantId(c))
                .ToList();
            return new AccessControlList(restaurantIds);
        }

        private static int[] ClaimToRestaurantId(Claim claim)
        {
            if (int.TryParse(claim.Value, out var i))
                return new[] { i };
            return Array.Empty<int>();
        }
    }
}
