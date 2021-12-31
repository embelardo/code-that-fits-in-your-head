using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    /// <summary>
    /// This class encapsulates the interactions expected by a typical HTTP
    /// client that has been interacting with the API before the expansion to
    /// a multitenant system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class exists to simulate a typical legacy client of the API. Thus,
    /// it helps to check that no breaking changes are introduced to the
    /// system. As such, the code in this class should rarely (if ever) be
    /// edited.
    /// </para>
    /// <para>
    /// It is most likely unrealistic to expect that the code in this class is
    /// never edited, but great care and consideration should be exhibited if
    /// or when editing. Edits should be benign (such as fixing a typo in a
    /// variable name) and not represent changes in behaviour.
    /// </para>
    /// <para>
    /// Exceptions to this rule may involve changes to the overrides
    /// <see cref="ConfigureWebHost(IWebHostBuilder)" /> as well as other
    /// hypothetical future overrides. This most likely doesn't represent
    /// externally visible behaviour, but may on the contrary be used to keep
    /// behaviour consistent.
    /// </para>
    /// <para>
    /// If telemetry, support agreements, or the like show that the no legacy
    /// clients exist, or none are supported, the above restrictions can be
    /// lifted.
    /// </para>
    /// </remarks>
    public sealed class LegacyApi : WebApplicationFactory<Startup>
    {
        private bool authorizeClient;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IReservationsRepository>();
                services.AddSingleton<IReservationsRepository>(
                    new FakeDatabase());
            });
        }

        internal void AuthorizeClient()
        {
            authorizeClient = true;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (!authorizeClient)
                return;

            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                "This is not the secret used in production.");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject =
                    new ClaimsIdentity(new[]
                    {
                        new Claim("role", "MaitreD"),
                        new Claim("restaurant", $"{Grandfather.Id}")
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<HttpResponseMessage> PostReservation(
            object reservation)
        {
            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";

            var address = await FindAddress("urn:reservations");
            return await CreateClient().PostAsync(address, content);
        }

        public async Task<HttpResponseMessage> PutReservation(
            Uri address,
            object reservation)
        {
            string json = JsonSerializer.Serialize(reservation);
            using var content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";
            return await CreateClient().PutAsync(address, content);
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
            var address = dto.Links.FindAddress("previous");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetNextYear()
        {
            var currentResp = await GetCurrentYear();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.FindAddress("next");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetYear(int year)
        {
            var resp = await GetCurrentYear();
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();
            if (dto.Year == year)
                return resp;

            var rel = dto.Year < year ? "next" : "previous";
            
            var client = CreateClient();
            do
            {
                var address = dto.Links.FindAddress(rel);
                resp = await client.GetAsync(address);
                resp.EnsureSuccessStatusCode();
                dto = await resp.ParseJsonContent<CalendarDto>();
            } while (dto.Year != year);
            
            return resp;
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
            var address = dto.Links.FindAddress("previous");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetNextMonth()
        {
            var currentResp = await GetCurrentMonth();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.FindAddress("next");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetMonth(int year, int month)
        {
            var resp = await GetYear(year);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();

            var target = new DateTime(year, month, 1).ToIso8601DateString();
            var monthCalendar = dto.Days.Single(d => d.Date == target);
            var address = monthCalendar.Links.FindAddress("urn:month");
            return await CreateClient().GetAsync(address);
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
            var address = dto.Links.FindAddress("previous");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetNextDay()
        {
            var currentResp = await GetCurrentDay();
            currentResp.EnsureSuccessStatusCode();
            var dto = await currentResp.ParseJsonContent<CalendarDto>();
            var address = dto.Links.FindAddress("next");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetDay(
            int year,
            int month,
            int day)
        {
            var resp = await GetYear(year);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();

            var target = new DateTime(year, month, day).ToIso8601DateString();
            var dayCalendar = dto.Days.Single(d => d.Date == target);
            var address = dayCalendar.Links.FindAddress("urn:day");
            return await CreateClient().GetAsync(address);
        }

        public async Task<HttpResponseMessage> GetSchedule(
            int year,
            int month,
            int day)
        {
            var resp = await GetYear(year);
            resp.EnsureSuccessStatusCode();
            var dto = await resp.ParseJsonContent<CalendarDto>();

            var target = new DateTime(year, month, day).ToIso8601DateString();
            var dayCalendar = dto.Days.Single(d => d.Date == target);
            var address = dayCalendar.Links.FindAddress("urn:schedule");
            return await CreateClient().GetAsync(address);
        }

        private async Task<Uri> FindAddress(string rel)
        {
            var homeResponse =
                await CreateClient().GetAsync(new Uri("", UriKind.Relative));
            homeResponse.EnsureSuccessStatusCode();
            var homeRepresentation =
                await homeResponse.ParseJsonContent<HomeDto>();

            return homeRepresentation.Links.FindAddress(rel);
        }
    }
}
