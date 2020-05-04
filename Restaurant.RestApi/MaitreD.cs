﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class MaitreD
    {
        public MaitreD(
            TimeSpan opensAt,
            TimeSpan seatingDuration,
            params Table[] tables) :
            this(opensAt, seatingDuration, tables.AsEnumerable())
        {
        }

        public MaitreD(
            TimeSpan opensAt,
            TimeSpan seatingDuration,
            IEnumerable<Table> tables)
        {
            OpensAt = opensAt;
            SeatingDuration = seatingDuration;
            Tables = tables;
        }

        public TimeSpan OpensAt { get; }
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
            if (candidate.At.TimeOfDay < OpensAt)
                return false;

            var seating = new Seating(SeatingDuration, candidate);
            var relevantReservations =
                existingReservations.Where(seating.Overlaps);
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