/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Table
    {
        private Table(TableType tableType, int seats)
        {
            Seats = seats;
            IsStandard = tableType == TableType.Standard;
            IsCommunal = tableType == TableType.Communal;
        }

        public static Table Standard(int seats)
        {
            return new Table(TableType.Standard, seats);
        }

        public static Table Communal(int seats)
        {
            return new Table(TableType.Communal, seats);
        }

        public int Seats { get; }
        public bool IsStandard { get; }
        public bool IsCommunal { get; }

        public Table WithSeats(int newSeats)
        {
            return new Table(
                IsStandard ? TableType.Standard : TableType.Communal,
                newSeats);
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   Seats == table.Seats;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seats);
        }
    }
}