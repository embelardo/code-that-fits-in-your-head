/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Ploeh.Samples.Restaurant.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public sealed class RestaurantOptionsBuilder
    {
        private readonly int id;
        private readonly string name;
        private readonly TimeSpan opensAt;
        private readonly TimeSpan lastSeating;
        private readonly TimeSpan seatingDuration;
        private readonly TableOptions[] tables;

        public RestaurantOptionsBuilder()
        {
            id = 9; // Not the grandfather ID
            name = "Foo";
            opensAt = TimeSpan.FromHours(12);
            lastSeating = TimeSpan.FromHours(22);
            seatingDuration = TimeSpan.FromHours(2.5);
            tables = new[] { new TableOptionsBuilder().Build() };
        }

        /// <summary>
        /// A build that represents the restaurant that's 'grandfathered' into
        /// the system.
        /// </summary>
        /// <seealso cref="RestApi.Grandfather" />
        public static RestaurantOptionsBuilder Grandfather =>
            new RestaurantOptionsBuilder(
                RestApi.Grandfather.Id,
                "Hipgnosta",
                (TimeSpan)Some.MaitreD.OpensAt,
                (TimeSpan)Some.MaitreD.LastSeating,
                Some.MaitreD.SeatingDuration,
                new[] { new TableOptions { TableType = TableType.Communal, Seats = 10 } });

        private RestaurantOptionsBuilder(
            int id,
            string name,
            TimeSpan opensAt,
            TimeSpan lastSeating,
            TimeSpan seatingDuration,
            TableOptions[] tables)
        {
            this.id = id;
            this.name = name;
            this.opensAt = opensAt;
            this.lastSeating = lastSeating;
            this.seatingDuration = seatingDuration;
            this.tables = tables;
        }

        public RestaurantOptionsBuilder WithId(int newId)
        {
            return new RestaurantOptionsBuilder(
                newId,
                name,
                opensAt,
                lastSeating,
                seatingDuration,
                tables);
        }

        public RestaurantOptionsBuilder WithName(string newName)
        {
            return new RestaurantOptionsBuilder(
                id,
                newName,
                opensAt,
                lastSeating,
                seatingDuration,
                tables);
        }

        public RestaurantOptionsBuilder WithOpensAt(TimeSpan newOpensAt)
        {
            return new RestaurantOptionsBuilder(
                id,
                name,
                newOpensAt,
                lastSeating,
                seatingDuration,
                tables);
        }

        public RestaurantOptionsBuilder WithLastSeating(TimeSpan newLastSeating)
        {
            return new RestaurantOptionsBuilder(
                id,
                name,
                opensAt,
                newLastSeating,
                seatingDuration,
                tables);
        }

        public RestaurantOptionsBuilder WithSeatingDuration(
            TimeSpan newSeatingDuration)
        {
            return new RestaurantOptionsBuilder(
                id,
                name,
                opensAt,
                lastSeating,
                newSeatingDuration,
                tables);
        }

        public RestaurantOptionsBuilder WithTables(
            params TableOptions[] newTables)
        {
            return new RestaurantOptionsBuilder(
                id,
                name,
                opensAt,
                lastSeating,
                seatingDuration,
                newTables);
        }

        public RestaurantOptions Build()
        {
            return new RestaurantOptions
            {
                Id = id,
                Name = name,
                OpensAt = opensAt,
                LastSeating = lastSeating,
                SeatingDuration = seatingDuration,
                Tables = tables
            };
        }
    }
}
