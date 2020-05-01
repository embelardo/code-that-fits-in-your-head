/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class MaitreD
    {
        public MaitreD(TimeSpan seatingDuration, params Table[] tables) :
            this(seatingDuration, tables.AsEnumerable())
        {
        }

        public MaitreD(TimeSpan seatingDuration, IEnumerable<Table> tables)
        {
            SeatingDuration = seatingDuration;
            Tables = tables;
        }

        public TimeSpan SeatingDuration { get; }
        public IEnumerable<Table> Tables { get; }

        public bool WillAccept(
            IEnumerable<Reservation> existingReservations,
            Reservation candidate)
        {
            if (existingReservations is null)
                throw new ArgumentNullException(nameof(existingReservations));
            if (candidate is null)
                throw new ArgumentNullException(nameof(candidate));

            var relevantReservations =
                existingReservations.Where(candidate.Overlaps);
            var availableTables = Allocate(relevantReservations);
            return availableTables.Any(t => t.Fits(candidate.Quantity));
        }

        private IEnumerable<Table> Allocate(
            IEnumerable<Reservation> reservations)
        {
            List<Table> availableTables = Tables.ToList();
            foreach (var r in reservations)
            {
                var table = availableTables.Find(t => t.Fits(r.Quantity));
                if (table is { })
                {
                    availableTables.Remove(table);
                    if (table.IsCommunal)
                        availableTables.Add(table.Reserve(r.Quantity));
                }
            }

            return availableTables;
        }
    }
}