/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    [SuppressMessage(
        "Naming",
        "CA1710:Identifiers should have correct suffix",
        Justification = "The role of the class is a Test Double.")]
    public class FakeDatabase :
        Collection<Reservation>, IReservationsRepository
    {
        public Task Create(Reservation reservation)
        {
            Add(reservation);
            return Task.CompletedTask;
        }
    }
}