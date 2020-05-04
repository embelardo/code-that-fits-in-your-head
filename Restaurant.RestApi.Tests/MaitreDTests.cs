/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class MaitreDTests
    {
        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class AcceptTestCases :
            TheoryData<TimeSpan, TimeSpan, IEnumerable<Table>, IEnumerable<Reservation>>
        {
            public AcceptTestCases()
            {
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(12) },
                    Array.Empty<Reservation>());
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(8), Table.Communal(11) },
                    Array.Empty<Reservation>());
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(2), Table.Communal(11) },
                    new[] { Some.Reservation.WithQuantity(2) });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(11) },
                    new[] { Some.Reservation.WithQuantity(11).TheDayBefore() });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(11) },
                    new[] { Some.Reservation.WithQuantity(11).TheDayAfter() });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(2.5),
                    new[] { Table.Standard(12) },
                    new[] { Some.Reservation.WithQuantity(11).AddDate(
                        TimeSpan.FromHours(-2.5)) });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(1),
                    new[] { Table.Standard(14) },
                    new[] { Some.Reservation.WithQuantity(9).AddDate(
                        TimeSpan.FromHours(1)) });
            }
        }

        [Theory, ClassData(typeof(AcceptTestCases))]
        public void Accept(
            TimeSpan opensAt,
            TimeSpan seatingDuration,
            IEnumerable<Table> tables,
            IEnumerable<Reservation> reservations)
        {
            var sut = new MaitreD(opensAt, seatingDuration, tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(reservations, r);

            Assert.True(actual);
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class RejectTestCases :
            TheoryData<TimeSpan, TimeSpan, IEnumerable<Table>, IEnumerable<Reservation>>
        {
            public RejectTestCases()
            {
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Communal(6), Table.Communal(6) },
                    Array.Empty<Reservation>());
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Standard(12) },
                    new[] { Some.Reservation.WithQuantity(1) });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Standard(11) },
                    new[] { Some.Reservation.WithQuantity(1).OneHourBefore() });
                Add(TimeSpan.FromHours(18),
                    TimeSpan.FromHours(6),
                    new[] { Table.Standard(12) },
                    new[] { Some.Reservation.WithQuantity(2).OneHourLater() });
                /* Some.Reservation.At is the time of the 'hard-coded'
                 * reservation in the test below. Adding 30 minutes to it means
                 * that the restaurant opens 30 minutes later than the desired
                 * reservation time, and therefore must be rejected. */
                Add(Some.Reservation.At.AddMinutes(30).TimeOfDay,
                    TimeSpan.FromHours(6),
                    new[] { Table.Standard(12) },
                    Array.Empty<Reservation>());
            }
        }

        [Theory, ClassData(typeof(RejectTestCases))]
        public void Reject(
            TimeSpan opensAt,
            TimeSpan seatingDuration,
            IEnumerable<Table> tables,
            IEnumerable<Reservation> reservations)
        {
            var sut = new MaitreD(opensAt, seatingDuration, tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(reservations, r);

            Assert.False(actual);
        }
    }
}
