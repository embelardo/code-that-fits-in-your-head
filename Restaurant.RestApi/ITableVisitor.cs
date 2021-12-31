/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.Generic;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public interface ITableVisitor<T>
    {
        T VisitStandard(int seats, Reservation? reservation);
        T VisitCommunal(
            int seats,
            IReadOnlyCollection<Reservation> reservations);
    }
}