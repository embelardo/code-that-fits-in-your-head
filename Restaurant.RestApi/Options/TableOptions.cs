/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Options
{
    public sealed class TableOptions
    {
        public TableType TableType { get; set; }
        public int Seats { get; set; }

        internal Table ToTable()
        {
            switch (TableType)
            {
                case TableType.Communal:
                    return Table.Communal(Seats);
                default:
                    return Table.Standard(Seats);
            }
        }
    }
}
