/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    internal class SpyPostOffice :
        Collection<SpyPostOffice.Observation>, IPostOffice
    {
        public Task EmailReservationCreated(Reservation reservation)
        {
            Add(new Observation(Event.Created, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationDeleted(Reservation reservation)
        {
            Add(new Observation(Event.Deleted, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdating(Reservation reservation)
        {
            Add(new Observation(Event.Updating, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdated(Reservation reservation)
        {
            Add(new Observation(Event.Updated, reservation));
            return Task.CompletedTask;
        }

        internal enum Event
        {
            Created = 0,
            Updating,
            Updated,
            Deleted
        }

        internal sealed class Observation
        {
            public Observation(Event @event, Reservation reservation)
            {
                Event = @event;
                Reservation = reservation;
            }

            public Event Event { get; }
            public Reservation Reservation { get; }

            public override bool Equals(object? obj)
            {
                return obj is Observation observation &&
                       Event == observation.Event &&
                       EqualityComparer<Reservation>.Default.Equals(Reservation, observation.Reservation);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Event, Reservation);
            }
        }
    }
}