/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class MaitreD
    {
        public MaitreD(
            TimeOfDay opensAt,
            TimeOfDay lastSeating,
            TimeSpan seatingDuration,
            params Table[] tables) :
            this(opensAt, lastSeating, seatingDuration, tables.AsEnumerable())
        {
        }

        public MaitreD(
            TimeOfDay opensAt,
            TimeOfDay lastSeating,
            TimeSpan seatingDuration,
            IEnumerable<Table> tables)
        {
            OpensAt = opensAt;
            LastSeating = lastSeating;
            SeatingDuration = seatingDuration;
            Tables = tables;
        }

        public TimeOfDay OpensAt { get; }
        public TimeOfDay LastSeating { get; }
        public TimeSpan SeatingDuration { get; }
        public IEnumerable<Table> Tables { get; }

        public bool WillAccept(
            DateTime now,
            IEnumerable<Reservation> existingReservations,
            Reservation candidate)
        {
            if (existingReservations is null)
                throw new ArgumentNullException(nameof(existingReservations));
            if (candidate is null)
                throw new ArgumentNullException(nameof(candidate));
            if (candidate.At < now)
                return false;
            if (IsOutsideOfOpeningHours(candidate))
                return false;

            var seating = new Seating(SeatingDuration, candidate);
            var relevantReservations =
                existingReservations.Where(seating.Overlaps);
            var availableTables = Allocate(relevantReservations);
            return availableTables.Any(t => t.Fits(candidate.Quantity));
        }

        private bool IsOutsideOfOpeningHours(Reservation reservation)
        {
            return reservation.At.TimeOfDay < OpensAt
                || LastSeating < reservation.At.TimeOfDay;
        }

        private IEnumerable<Table> Allocate(
            IEnumerable<Reservation> reservations)
        {
            List<Table> allocation = Tables.ToList();
            foreach (var r in reservations)
            {
                var table = allocation.Find(t => t.Fits(r.Quantity));
                if (table is { })
                {
                    allocation.Remove(table);
                    allocation.Add(table.Reserve(r));
                }
            }

            return allocation;
        }

#pragma warning disable CA1822 // Mark members as static
        public IEnumerable<Occurrence<IEnumerable<Table>>> Schedule(
#pragma warning restore CA1822 // Mark members as static
            IEnumerable<Reservation> reservations)
        {
            var tables = reservations.Select(r => Table.Communal(12).Reserve(r));
            return
                from r in reservations
                group r by r.At into g
                orderby g.Key
                select tables.At(g.Key);
        }
    }
}