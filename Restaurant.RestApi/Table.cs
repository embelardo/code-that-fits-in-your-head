/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurant.RestApi
{
    public class Table
    {
        private TableType communal;

        public Table(TableType communal, int seats)
        {
            this.communal = communal;
            Seats = seats;
        }

        public int Seats { get; }
    }
}