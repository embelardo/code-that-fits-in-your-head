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

        public MaitreD WithTables(params Table[] newTables)
        {
            return new MaitreD(
                OpensAt,
                LastSeating,
                SeatingDuration,
                newTables);
        }

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

            var seating = new Seating(SeatingDuration, candidate.At);
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

        public IEnumerable<Occurrence<IEnumerable<Table>>> Schedule(
            IEnumerable<Reservation> reservations)
        {
            return
                from r in reservations
                group r by r.At into g
                orderby g.Key
                let seating = new Seating(SeatingDuration, g.Key)
                let overlapping = reservations.Where(seating.Overlaps)
                select Allocate(overlapping).At(g.Key);
        }

        /// <summary>
        /// Segment a day into 15-minute segments during the restaurant's
        /// opening hours, with a table configuration for each segment.
        /// </summary>
        /// <param name="date">The day to segment.</param>
        /// <param name="reservations">
        /// Reservations relevant for <paramref name="date" />.
        /// </param>
        /// <returns>
        /// 15-minute segments starting at the restaurant's opening hour, and
        /// concluding at the restaurant's last seating time. Each segment
        /// contains the table allocation at that time.
        /// </returns>
        public IEnumerable<Occurrence<IEnumerable<Table>>> Segment(
            DateTime date,
            Reservation[] reservations)
        {
            for (var dur = (TimeSpan)OpensAt;
                 dur <= (TimeSpan)LastSeating;
                 dur = dur.Add(TimeSpan.FromMinutes(15)))
            {
                var at = date.Date.Add(dur);
                var seating = new Seating(SeatingDuration, at);
                var relevantReservations =
                    reservations.Where(seating.Overlaps);
                var allocation = Allocate(relevantReservations);
                yield return new Occurrence<IEnumerable<Table>>(at, allocation);
            }
        }
    }
}