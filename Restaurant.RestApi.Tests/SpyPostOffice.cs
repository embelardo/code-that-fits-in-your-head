/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal class SpyPostOffice :
        Collection<SpyPostOffice.Observation>, IPostOffice
    {
        public Task EmailReservationCreated(
            int restaurantId,
            Reservation reservation)
        {
            Add(new Observation(Event.Created, restaurantId, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationDeleted(
            int restaurantId,
            Reservation reservation)
        {
            Add(new Observation(Event.Deleted, restaurantId, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdating(
            int restaurantId,
            Reservation reservation)
        {
            Add(new Observation(Event.Updating, restaurantId, reservation));
            return Task.CompletedTask;
        }

        public Task EmailReservationUpdated(
            int restaurantId,
            Reservation reservation)
        {
            Add(new Observation(Event.Updated, restaurantId, reservation));
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
            public Observation(
                Event @event,
                int restaurantId,
                Reservation reservation)
            {
                Event = @event;
                RestaurantId = restaurantId;
                Reservation = reservation;
            }

            public Event Event { get; }
            public int RestaurantId { get; }
            public Reservation Reservation { get; }

            public override bool Equals(object? obj)
            {
                return obj is Observation observation &&
                       Event == observation.Event &&
                       RestaurantId == observation.RestaurantId &&
                       EqualityComparer<Reservation>.Default.Equals(Reservation, observation.Reservation);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Event, RestaurantId, Reservation);
            }
        }
    }
}