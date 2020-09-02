/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    /// <summary>
    /// Contains information about the restaurant that was 'grandfathered' in
    /// when the system was expanded to a multitenant system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This code base was originally developed to support a single restaurant.
    /// In that context, the restaurant was an implicit part of the context
    /// instead of an explicit entity. When expanded to a multitenant system,
    /// the original restaurant had to be 'grandfathered' in. APIs that don't
    /// explicitly work on a given restaurant must be assumed to operate on the
    /// implicit 'first' restaurant, in order to not break existing third-
    /// party clients.
    /// </para>
    /// </remarks>
    public static class Grandfather
    {
        /// <summary>
        /// The ID of the 'original' restaurant, arbitrarily designated a
        /// particular number. A restaurant with this ID must exist in the
        /// restaurant configuration.
        /// </summary>
        public const int Id = 1;
    }
}
