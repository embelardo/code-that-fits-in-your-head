/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public interface IReservationsRepository
    {
        Task Create(Reservation reservation);
    }
}