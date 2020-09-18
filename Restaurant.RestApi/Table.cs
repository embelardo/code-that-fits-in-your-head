/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed partial class Table
    {
        private readonly ITable table;
        private readonly int seats;

        private Table(ITable table, int seats)
        {
            this.table = table;
            this.seats = seats;
        }

        public static Table Standard(int seats)
        {
            return new Table(new StandardTable(seats), seats);
        }

        public static Table Communal(int seats)
        {
            return new Table(new CommunalTable(seats), seats);
        }

        public int Capacity
        {
            get { return Accept(new CapacityVisitor()); }
        }

        public int RemainingSeats
        {
            get { return Accept(new RemainingSeatsVisitor()); }
        }

        internal bool Fits(int quantity)
        {
            return quantity <= RemainingSeats;
        }

        public Table Reserve(Reservation reservation)
        {
            return Accept(new ReserveVisitor(reservation));
        }

        public T Accept<T>(ITableVisitor<T> visitor)
        {
            return table.Accept(visitor);
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   Equals(this.table, table.table);
        }

        public override int GetHashCode()
        {
            return table.GetHashCode();
        }

        private interface ITable
        {
            T Accept<T>(ITableVisitor<T> visitor);
        }

        private sealed class StandardTable : ITable
        {
            private readonly int seats;
            private readonly Reservation? reservation;

            public StandardTable(int seats)
            {
                this.seats = seats;
            }

            public StandardTable(int seats, Reservation reservation)
            {
                this.seats = seats;
                this.reservation = reservation;
            }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitStandard(seats, reservation);
            }

            public override bool Equals(object? obj)
            {
                return obj is StandardTable table &&
                       seats == table.seats &&
                       Equals(reservation, table.reservation);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(seats, reservation);
            }
        }

        private sealed class CommunalTable : ITable
        {
            private readonly int seats;
            private readonly IReadOnlyCollection<Reservation> reservations;

            public CommunalTable(int seats, params Reservation[] reservations)
            {
                this.seats = seats;
                this.reservations = reservations;
            }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitCommunal(seats, reservations);
            }

            public override bool Equals(object? obj)
            {
                return obj is CommunalTable table &&
                       seats == table.seats &&
                       Enumerable.SequenceEqual(
                           reservations,
                           table.reservations);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(seats, reservations);
            }
        }

        private sealed class ReserveVisitor : ITableVisitor<Table>
        {
            private readonly Reservation reservation;

            public ReserveVisitor(Reservation reservation)
            {
                this.reservation = reservation;
            }

            public Table VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return new Table(
                    new CommunalTable(
                        seats,
                        reservations.Append(reservation).ToArray()),
                    seats);
            }

            public Table VisitStandard(int seats, Reservation? reservation)
            {
                return new Table(
                    new StandardTable(
                        seats,
                        this.reservation),
                    seats);
            }
        }

        private sealed class RemainingSeatsVisitor : ITableVisitor<int>
        {
            public int VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return seats - reservations.Sum(r => r.Quantity);
            }

            public int VisitStandard(int seats, Reservation? reservation)
            {
                return reservation is null ? seats : 0;
            }
        }

        private sealed class CapacityVisitor : ITableVisitor<int>
        {
            public int VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return seats;
            }

            public int VisitStandard(int seats, Reservation? reservation)
            {
                return seats;
            }
        }
    }
}