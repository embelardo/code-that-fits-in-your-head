/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Table
    {
        private readonly ITable table;

        private Table(ITable table)
        {
            this.table = table;
        }

        public static Table Standard(int seats)
        {
            return new Table(new StandardTable(seats));
        }

        public static Table Communal(int seats)
        {
            return new Table(new CommunalTable(seats));
        }

        public int Seats
        {
            get { return table.Seats; }
        }

        public bool IsStandard
        {
            get { return table.Accept(new IsStandardVisitor()); }
        }

        public bool IsCommunal
        {
            get { return !IsStandard; }
        }

        public Table WithSeats(int newSeats)
        {
            return table.Accept(new CopyAndUpdateSeatsVisitor(newSeats));
        }

        internal bool Fits(int quantity)
        {
            return quantity <= Seats;
        }

        internal Table Reserve(Reservation reservation)
        {
            return WithSeats(Seats - reservation.Quantity);
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   Seats == table.Seats &&
                   IsStandard == table.IsStandard &&
                   IsCommunal == table.IsCommunal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seats, IsStandard, IsCommunal);
        }

        private interface ITable
        {
            int Seats { get; }
            T Accept<T>(ITableVisitor<T> visitor);
        }

        private interface ITableVisitor<T>
        {
            T VisitStandard(int seats);
            T VisitCommunal(int seats);
        }

        private sealed class StandardTable : ITable
        {
            public StandardTable(int seats)
            {
                Seats = seats;
            }

            public int Seats { get; }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitStandard(Seats);
            }
        }

        private sealed class CommunalTable : ITable
        {
            public CommunalTable(int seats)
            {
                Seats = seats;
            }

            public int Seats { get; }

            public T Accept<T>(ITableVisitor<T> visitor)
            {
                return visitor.VisitCommunal(Seats);
            }
        }

        private sealed class CopyAndUpdateSeatsVisitor : ITableVisitor<Table>
        {
            private readonly int newSeats;

            public CopyAndUpdateSeatsVisitor(int newSeats)
            {
                this.newSeats = newSeats;
            }

            public Table VisitStandard(int seats)
            {
                return new Table(new StandardTable(newSeats));
            }

            public Table VisitCommunal(int seats)
            {
                return new Table(new CommunalTable(newSeats));
            }
        }

        private sealed class IsStandardVisitor : ITableVisitor<bool>
        {
            public bool VisitStandard(int seats)
            {
                return true;
            }

            public bool VisitCommunal(int seats)
            {
                return false;
            }
        }
    }
}