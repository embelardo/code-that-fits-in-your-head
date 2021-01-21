/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Ploeh.Samples.Restaurants.RestApi;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class SqlReservationsRepository : IReservationsRepository
    {
        public SqlReservationsRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public async Task Create(int restaurantId, Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(createReservationSql, conn);
            cmd.Parameters.AddWithValue("@Id", reservation.Id);
            cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
            cmd.Parameters.AddWithValue("@At", reservation.At);
            cmd.Parameters.AddWithValue("@Name", reservation.Name.ToString());
            cmd.Parameters.AddWithValue("@Email", reservation.Email.ToString());
            cmd.Parameters.AddWithValue("@Quantity", reservation.Quantity);

            await conn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private const string createReservationSql = @"
            INSERT INTO [dbo].[Reservations] (
                [PublicId], [RestaurantId], [At], [Name], [Email], [Quantity])
            VALUES (@Id, @RestaurantId, @At, @Name, @Email, @Quantity)";

        public async Task<IReadOnlyCollection<Reservation>> ReadReservations(
            int restaurantId,
            DateTime min,
            DateTime max)
        {
            var result = new List<Reservation>();

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(readByRangeSql, conn);
            cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
            cmd.Parameters.AddWithValue("@Min", min);
            cmd.Parameters.AddWithValue("@Max", max);

            await conn.OpenAsync().ConfigureAwait(false);
            using var rdr =
                await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await rdr.ReadAsync().ConfigureAwait(false))
                result.Add(ReadReservationRow(rdr));

            return result.AsReadOnly();
        }

        private const string readByRangeSql = @"
            SELECT [PublicId], [At], [Name], [Email], [Quantity]
            FROM [dbo].[Reservations]
            WHERE [RestaurantId] = @RestaurantId AND
                  @Min <= [At] AND [At] <= @Max";

        public async Task<Reservation?> ReadReservation(
            int restaurantId, Guid id)
        {
            const string readByIdSql = @"
                SELECT [PublicId], [At], [Name], [Email], [Quantity]
                FROM [dbo].[Reservations]
                WHERE [PublicId] = @id";

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(readByIdSql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync().ConfigureAwait(false);
            using var rdr =
                await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (!rdr.Read())
                return null;

            return ReadReservationRow(rdr);
        }

        private static Reservation ReadReservationRow(SqlDataReader rdr)
        {
            return new Reservation(
                (Guid)rdr["PublicId"],
                (DateTime)rdr["At"],
                new Email((string)rdr["Email"]),
                new Name((string)rdr["Name"]),
                (int)rdr["Quantity"]);
        }

        public async Task Update(int restaurantId, Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            const string updateSql = @"
                UPDATE [dbo].[Reservations]
                SET [At]       = @at,
                    [Name]     = @name,
                    [Email]    = @email,
                    [Quantity] = @quantity
                WHERE [PublicId] = @id";

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@id", reservation.Id);
            cmd.Parameters.AddWithValue("@at", reservation.At);
            cmd.Parameters.AddWithValue("@name", reservation.Name.ToString());
            cmd.Parameters.AddWithValue("@email", reservation.Email.ToString());
            cmd.Parameters.AddWithValue("@quantity", reservation.Quantity);

            await conn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task Delete(int restaurantId, Guid id)
        {
            const string deleteSql = @"
                DELETE [dbo].[Reservations]
                WHERE [PublicId] = @id";

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(deleteSql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
