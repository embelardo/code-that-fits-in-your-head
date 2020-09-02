/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Ploeh.Samples.Restaurants.RestApi.SqlIntegrationTests
{
    public class UseDatabaseAttribute : BeforeAfterTestAttribute
    {
        [SuppressMessage(
            "Security",
            "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "No user input, but resource stream.")]
        public override void Before(MethodInfo methodUnderTest)
        {
            DeleteDatabase();

            var builder = new SqlConnectionStringBuilder(
                ConnectionStrings.Reservations);
            builder.InitialCatalog = "master";
            
            using var conn = new SqlConnection(builder.ConnectionString);
            using var cmd = new SqlCommand();
            conn.Open();
            cmd.Connection = conn;

            foreach (var sql in ReadSchema())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }

            base.Before(methodUnderTest);
        }

        private static IEnumerable<string> ReadSchema()
        {
            yield return "CREATE DATABASE [RestaurantIntegrationTest]";
            yield return "USE [RestaurantIntegrationTest]";

            var dbSchema = ReadDdl("RestaurantDbSchema");
            foreach (var s in SeperateStatements(dbSchema))
                yield return s;

            var addGuidColumn = ReadDdl("AddGuidColumnToReservations");
            foreach (var s in SeperateStatements(addGuidColumn))
                yield return s;

            var addTenantIdColumn = ReadDdl("AddTenantColumnToReservations");
            foreach (var s in SeperateStatements(addTenantIdColumn))
                yield return s;
        }

        private static string ReadDdl(string name)
        {
            using var strm = typeof(SqlReservationsRepository)
                .Assembly
                .GetManifestResourceStream(
                    $"Ploeh.Samples.Restaurants.RestApi.{name}.sql");
            using var rdr = new StreamReader(strm!);
            return rdr.ReadToEnd();
        }

        private static IEnumerable<string> SeperateStatements(string schemaSql)
        {
            return schemaSql.Split(
                new[] { "GO" },
                StringSplitOptions.RemoveEmptyEntries);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            base.After(methodUnderTest);
            DeleteDatabase();
        }

        private static void DeleteDatabase()
        {
            var dropCmd = @"
                IF EXISTS (SELECT name
                    FROM master.dbo.sysdatabases
                    WHERE name = N'RestaurantIntegrationTest')

                BEGIN
                    -- This closes existing connections:
                    ALTER DATABASE [RestaurantIntegrationTest]
                    SET SINGLE_USER WITH ROLLBACK IMMEDIATE

                    DROP DATABASE [RestaurantIntegrationTest]
                END";

            var builder = new SqlConnectionStringBuilder(
                ConnectionStrings.Reservations);
            builder.InitialCatalog = "master";
            using var conn = new SqlConnection(builder.ConnectionString);
            using var cmd = new SqlCommand(dropCmd, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}