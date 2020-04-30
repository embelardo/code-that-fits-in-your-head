/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Table
    {
        public Table(TableType tableType, int seats)
        {
            TableType = tableType;
            Seats = seats;
        }

        public TableType TableType { get; }
        public int Seats { get; }

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