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
            id = Grandfather.Id;
            name = "Hipgnosta";
            opensAt = (TimeSpan)Some.MaitreD.OpensAt;
            lastSeating = (TimeSpan)Some.MaitreD.LastSeating;
            seatingDuration = Some.MaitreD.SeatingDuration;
            tables = new[] { new TableOptionsBuilder().Build() };
        }

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
