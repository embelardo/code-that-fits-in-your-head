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
            TheoryData<IEnumerable<Table>, IEnumerable<Reservation>>
        {
            public AcceptTestCases()
            {
                Add(new[] { Table.Communal(12) }, Array.Empty<Reservation>());
                Add(new[] { Table.Communal(8), Table.Communal(11) },
                    Array.Empty<Reservation>());
                Add(new[] { Table.Communal(2), Table.Communal(11) },
                    new[] { Some.Reservation.WithQuantity(2) });
            }
        }

        [Theory, ClassData(typeof(AcceptTestCases))]
        public void Accept(
            IEnumerable<Table> tables,
            IEnumerable<Reservation> reservations)
        {
            var sut = new MaitreD(tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(reservations, r);

            Assert.True(actual);
        }

        [SuppressMessage(
            "Performance",
            "CA1812: Avoid uninstantiated internal classes",
            Justification = "This class is instantiated via Reflection.")]
        private class RejectTestCases :
            TheoryData<IEnumerable<Table>, IEnumerable<Reservation>>
        {
            public RejectTestCases()
            {
                Add(new[] { Table.Communal(6), Table.Communal(6) },
                    Array.Empty<Reservation>());
                Add(new[] { Table.Standard(12) },
                    new[] { Some.Reservation.WithQuantity(1) });
            }
        }

        [Theory, ClassData(typeof(RejectTestCases))]
        public void Reject(
            IEnumerable<Table> tables,
            IEnumerable<Reservation> reservations)
        {
            var sut = new MaitreD(tables);

            var r = Some.Reservation.WithQuantity(11);
            var actual = sut.WillAccept(reservations, r);

            Assert.False(actual);
        }
    }
}
