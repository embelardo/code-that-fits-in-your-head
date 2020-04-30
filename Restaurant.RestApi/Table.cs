/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurant.RestApi
{
    public class Table
    {
        public Table(TableType tableType, int seats)
        {
            TableType = tableType;
            Seats = seats;
        }

        public TableType TableType { get; }
        public int Seats { get; }
    }
}