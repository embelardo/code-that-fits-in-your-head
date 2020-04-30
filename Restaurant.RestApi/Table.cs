/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Table
    {
        private Table(TableType tableType, int seats)
        {
            TableType = tableType;
            Seats = seats;
        }

        public static Table Standard(int seats)
        {
            return new Table(TableType.Standard, seats);
        }

        public static Table Communal(int seats)
        {
            return new Table(TableType.Communal, seats);
        }

        public TableType TableType { get; }
        public int Seats { get; }

        public Table WithSeats(int newSeats)
        {
            return new Table(TableType, newSeats);
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   TableType == table.TableType &&
                   Seats == table.Seats;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableType, Seats);
        }
    }
}