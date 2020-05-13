/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Reservation
    {
        public Reservation(
            Guid id,
            DateTime at,
            string email,
            string name,
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
        public string Email { get; }
        public string Name { get; }
        public int Quantity { get; }

        public Reservation WithDate(DateTime newAt)
        {
            return new Reservation(Id, newAt, Email, Name, Quantity);
        }

        public Reservation WithEmail(string newEmail)
        {
            return new Reservation(Id, At, newEmail, Name, Quantity);
        }

        public Reservation WithName(string newName)
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
                   Email == reservation.Email &&
                   Name == reservation.Name &&
                   Quantity == reservation.Quantity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, At, Email, Name, Quantity);
        }
    }
}