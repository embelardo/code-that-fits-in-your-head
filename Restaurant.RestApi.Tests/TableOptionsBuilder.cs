/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Ploeh.Samples.Restaurant.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public sealed class TableOptionsBuilder
    {
        private readonly TableType tableType;
        private readonly int seats;

        public TableOptionsBuilder()
        {
            tableType = TableType.Communal;
            seats = 10;
        }

        private TableOptionsBuilder(TableType tableType, int seats)
        {
            this.tableType = tableType;
            this.seats = seats;
        }

        public TableOptionsBuilder Standard()
        {
            return new TableOptionsBuilder(TableType.Standard, seats);
        }

        public TableOptionsBuilder Communal()
        {
            return new TableOptionsBuilder(TableType.Communal, seats);
        }

        public TableOptionsBuilder WithSeats(int newSeats)
        {
            return new TableOptionsBuilder(tableType, newSeats);
        }

        public TableOptions Build()
        {
            return new TableOptions { TableType = tableType, Seats = seats };
        }
    }
}
