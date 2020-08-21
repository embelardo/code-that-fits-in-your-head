/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Ploeh.Samples.Restaurant.RestApi;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public class SqlReservationsRepository : IReservationsRepository
    {
        public SqlReservationsRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public async Task Create(Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(createReservationSql, conn);
            cmd.Parameters.AddWithValue("@Id", reservation.Id);
            cmd.Parameters.AddWithValue("@At", reservation.At);
            cmd.Parameters.AddWithValue("@Name", reservation.Name.ToString());
            cmd.Parameters.AddWithValue("@Email", reservation.Email.ToString());
            cmd.Parameters.AddWithValue("@Quantity", reservation.Quantity);

            await conn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private const string createReservationSql = @"
            INSERT INTO [dbo].[Reservations] (
                [PublicId], [At], [Name], [Email], [Quantity])
            VALUES (@Id, @At, @Name, @Email, @Quantity)";

        public async Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime dateTime)
        {
            var result = new List<Reservation>();

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(readByRangeSql, conn);
            cmd.Parameters.AddWithValue("@at", dateTime.Date);

            await conn.OpenAsync().ConfigureAwait(false);
            using var rdr =
                await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (rdr.Read())
                result.Add(
                    new Reservation(
                        (Guid)rdr["PublicId"],
                        (DateTime)rdr["At"],
                        new Email((string)rdr["Email"]),
                        new Name((string)rdr["Name"]),
                        (int)rdr["Quantity"]));

            return result.AsReadOnly();
        }

        private const string readByRangeSql = @"
            SELECT [PublicId], [At], [Name], [Email], [Quantity]
            FROM [dbo].[Reservations]
            WHERE CONVERT(DATE, [At]) = @At";

        public async Task<IReadOnlyCollection<Reservation>> ReadReservations(
            DateTime min,
            DateTime max)
        {
            const string readByRangeSql = @"
                SELECT [PublicId], [Date], [Name], [Email], [Quantity]
                FROM [dbo].[Reservations]
                WHERE @Min <= [Date] AND [Date] <= @Max";
            
            var result = new List<Reservation>();

            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(readByRangeSql, conn);
            cmd.Parameters.AddWithValue("@Min", min);
            cmd.Parameters.AddWithValue("@Max", max);

            await conn.OpenAsync().ConfigureAwait(false);
            using var rdr =
                await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await rdr.ReadAsync().ConfigureAwait(false))
                result.Add(
                    new Reservation(
                        (Guid)rdr["PublicId"],
                        (DateTime)rdr["Date"],
                        new Email((string)rdr["Email"]),
                        new Name((string)rdr["Name"]),
                        (int)rdr["Quantity"]));

            return result.AsReadOnly();
        }

        public async Task<Reservation?> ReadReservation(Guid id)
        {
            const string readByIdSql = @"
                SELECT [At], [Name], [Email], [Quantity]
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

            return new Reservation(
                id,
                (DateTime)rdr["At"],
                new Email((string)rdr["Email"]),
                new Name((string)rdr["Name"]),
                (int)rdr["Quantity"]);
        }

        public async Task Update(Reservation reservation)
        {
            if (reservation is null)
                throw new ArgumentNullException(nameof(reservation));

            const string updateSql = @"
                UPDATE [dbo].[Reservations]
                SET [At]     = @at,
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

        public async Task Delete(Guid id)
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
