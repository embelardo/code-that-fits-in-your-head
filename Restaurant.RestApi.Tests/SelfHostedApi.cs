/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi.Tests
{
    public sealed class SelfHostedApi : WebApplicationFactory<Startup>
    {
        private bool authorizeClient;
        private int[] restaurantIds;

        public SelfHostedApi()
        {
            restaurantIds = Array.Empty<int>();
        }

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

        internal void AuthorizeClient(params int[] restaurantIds)
        {
            authorizeClient = true;
            this.restaurantIds = restaurantIds;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (!authorizeClient)
                return;

            var generator = new JwtTokenGenerator(restaurantIds, "MaitreD");
            var token = generator.GenerateJwtToken();
            client.Authorize(token);
        }
    }
}
