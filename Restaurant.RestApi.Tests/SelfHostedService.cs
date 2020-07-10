/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public class SelfHostedService : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservationsRepository>();
                services.AddSingleton<IReservationsRepository>(
                    new FakeDatabase());

                services.RemoveAll<CalendarFlag>();
                services.AddSingleton(new CalendarFlag(true));
            });
        }

        [SuppressMessage(
            "Usage",
            "CA2234:Pass system uri objects instead of strings",
            Justification = "URL isn't passed as variable, but as literal.")]
        public async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PostAsync("reservations", content);
        }

        public async Task<HttpResponseMessage> PutReservation(
            Uri address,
            object reservation)
        {
            var client = CreateClient();

            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await client.PutAsync(address, content);
        }

        public async Task<HttpResponseMessage> GetCurrentYear()
        {
            var yearAddress = await FindAddress("urn:year");
            return await CreateClient().GetAsync(yearAddress);
        }

        public async Task<HttpResponseMessage> GetPreviousYear()
        {
            var currentResp = await GetCurrentYear();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "previous").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type previous not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        public async Task<HttpResponseMessage> GetNextYear()
        {
            var currentResp = await GetCurrentYear();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "next").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type next not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        public async Task<HttpResponseMessage> GetYear(int year)
        {
            var resp = await GetCurrentYear();
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();
            if (dto.Year == year)
                return resp;
            else if (dto.Year < year)
            {
                var client = CreateClient();
                do
                {
                    var address = dto.Links.Single(l => l.Rel == "next").Href;
                    if (address is null)
                        throw new InvalidOperationException(
                            "Address for relationship type next not found.");
                    resp = await client.GetAsync(new Uri(address));
                    resp.EnsureSuccessStatusCode();
                    dto = await resp.ParseJsonContent<CalendarDto>();
                } while (dto.Year != year);
                return resp;
            }
            else
            {
                var client = CreateClient();
                do
                {
                    var address = dto.Links.Single(l => l.Rel == "previous").Href;
                    if (address is null)
                        throw new InvalidOperationException(
                            "Address for relationship type previous not found.");
                    resp = await client.GetAsync(new Uri(address));
                    resp.EnsureSuccessStatusCode();
                    dto = await resp.ParseJsonContent<CalendarDto>();
                } while (dto.Year != year);
                return resp;
            }
        }

        public async Task<HttpResponseMessage> GetCurrentMonth()
        {
            var monthAddress = await FindAddress("urn:month");
            return await CreateClient().GetAsync(monthAddress);
        }

        public async Task<HttpResponseMessage> GetPreviousMonth()
        {
            var currentResp = await GetCurrentMonth();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "previous").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type previous not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        public async Task<HttpResponseMessage> GetNextMonth()
        {
            var currentResp = await GetCurrentMonth();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "next").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type previous not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        public async Task<HttpResponseMessage> GetCurrentDay()
        {
            var dayAddress = await FindAddress("urn:day");
            return await CreateClient().GetAsync(dayAddress);
        }

        public async Task<HttpResponseMessage> GetPreviousDay()
        {
            var currentResp = await GetCurrentDay();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "previous").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type previous not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        public async Task<HttpResponseMessage> GetNextDay()
        {
            var currentResp = await GetCurrentDay();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.Single(l => l.Rel == "next").Href;
            if (address is null)
                throw new InvalidOperationException(
                    "Address for relationship type next not found.");

            var client = CreateClient();
            return await client.GetAsync(new Uri(address));
        }

        private async Task<Uri> FindAddress(string rel)
        {
            var client = CreateClient();

            var homeResponse =
                await client.GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();
            var address =
                homeRepresentation.Links.Single(l => l.Rel == rel).Href;
            if (address is null)
                throw new InvalidOperationException(
                    $"Address for relationship type {rel} not found.");

            return new Uri(address);
        }
    }
}
