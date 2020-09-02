/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Reservation
    {
        public Reservation(
            Guid id,
            DateTime at,
            Email email,
            Name name,
            int quantity)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "The value must be a positive (non-zero) number.");

            Id = id;
            At = at;
            Email = email;
            Name = name;
            Quantity = quantity;
        }

        public Guid Id { get; }
        public DateTime At { get; }
        public Email Email { get; }
        public Name Name { get; }
        public int Quantity { get; }

        public Reservation WithDate(DateTime newAt)
        {
            return new Reservation(Id, newAt, Email, Name, Quantity);
        }

        public Reservation WithEmail(Email newEmail)
        {
            return new Reservation(Id, At, newEmail, Name, Quantity);
        }

        public Reservation WithName(Name newName)
        {
            return new Reservation(Id, At, Email, newName, Quantity);
        }

        public Reservation WithQuantity(int newQuantity)
        {
            return new Reservation(Id, At, Email, Name, newQuantity);
        }

        public override bool Equals(object? obj)
        {
            return obj is Reservation reservation &&
                   Id.Equals(reservation.Id) &&
                   At == reservation.At &&
                   EqualityComparer<Email>.Default.Equals(Email, reservation.Email) &&
                   EqualityComparer<Name>.Default.Equals(Name, reservation.Name) &&
                   Quantity == reservation.Quantity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, At, Email, Name, Quantity);
        }
    }
}