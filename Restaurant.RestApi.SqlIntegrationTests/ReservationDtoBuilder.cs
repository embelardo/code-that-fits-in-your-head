/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Globalization;

namespace Ploeh.Samples.Restaurants.RestApi.SqlIntegrationTests
{
    internal class ReservationDtoBuilder
    {
        private readonly ReservationDto dto;

        public ReservationDtoBuilder()
        {
            dto = new ReservationDto
            {
                At = "2020-06-30 12:30",
                Email = "x@example.com",
                Name = "",
                Quantity = 1
            };
        }

        internal ReservationDtoBuilder WithDate(DateTime newDate)
        {
            dto.At = newDate.ToString("o", CultureInfo.InvariantCulture);
            return this;
        }

        internal ReservationDtoBuilder WithQuantity(int newQuantity)
        {
            dto.Quantity = newQuantity;
            return this;
        }

        internal ReservationDto Build()
        {
            return dto;
        }
    }
}