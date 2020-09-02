/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class RestaurantApiClient
    {
        internal static async Task<HttpResponseMessage> GetRestaurant(
            this HttpClient client,
            string name)
        {
            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var restaurant =
                homeRepresentation.Restaurants.First(r => r.Name == name);
            var address = restaurant.Links.FindAddress("urn:restaurant");

            return await client.GetAsync(address);
        }

        internal static async Task<HttpResponseMessage> PostReservation(
            this HttpClient client,
            string name,
            object reservation)
        {
            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";

            var resp = await client.GetRestaurant(name);
            resp.EnsureSuccessStatusCode();
            var rest = await resp.ParseJsonContent<RestaurantDto>();
            var address = rest.Links.FindAddress("urn:reservations");

            return await client.PostAsync(address, content);
        }

        internal static async Task<HttpResponseMessage> PutReservation(
            this HttpClient client,
            Uri address,
            object reservation)
        {
            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PutAsync(address, content);
        }

        internal static async Task<HttpResponseMessage> GetCurrentYear(
            this HttpClient client,
            string name)
        {
            var resp = await client.GetRestaurant(name);
            resp.EnsureSuccessStatusCode();
            var rest = await resp.ParseJsonContent<RestaurantDto>();
            var address = rest.Links.FindAddress("urn:year");
            return await client.GetAsync(address);
        }

        internal static async Task<HttpResponseMessage> GetYear(
            this HttpClient client,
            string name,
            int year)
        {
            var resp = await client.GetCurrentYear(name);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();
            if (dto.Year == year)
                return resp;

            var rel = dto.Year < year ? "next" : "previous";

            do
            {
                var address = dto.Links.FindAddress(rel);
                resp = await client.GetAsync(address);
                resp.EnsureSuccessStatusCode();
                dto = await resp.ParseJsonContent<CalendarDto>();
            } while (dto.Year != year);

            return resp;
        }

        internal static async Task<HttpResponseMessage> GetDay(
            this HttpClient client,
            string name,
            int year,
            int month,
            int day)
        {
            var resp = await client.GetYear(name, year);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();

            var target = new DateTime(year, month, day).ToIso8601DateString();
            var dayCalendar = dto.Days.Single(d => d.Date == target);
            var address = dayCalendar.Links.FindAddress("urn:day");
            return await client.GetAsync(address);
        }

        internal static async Task<HttpResponseMessage> GetSchedule(
            this HttpClient client,
            string name,
            int year,
            int month,
            int day)
        {
            var resp = await client.GetYear(name, year);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();

            var target = new DateTime(year, month, day).ToIso8601DateString();
            var dayCalendar = dto.Days.Single(d => d.Date == target);
            var address = dayCalendar.Links.FindAddress("urn:schedule");
            return await client.GetAsync(address);
        }

        internal static HttpClient Authorize(
            this HttpClient client,
            string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
