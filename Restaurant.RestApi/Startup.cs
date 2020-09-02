/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ploeh.Samples.Restaurant.RestApi.Options;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var urlSigningKey = Encoding.ASCII.GetBytes(
                Configuration.GetValue<string>("UrlSigningKey"));

            services
                .AddControllers(opts =>
                {
                    opts.Filters.Add<LinksFilter>();
                    opts.Filters.Add(
                        new UrlIntegrityFilter(
                            urlSigningKey));
                })
                .AddJsonOptions(opts =>
                    opts.JsonSerializerOptions.IgnoreNullValues = true);

            ConfigureUrSigning(services, urlSigningKey);

            ConfigureAuthorization(services);

            services
                .AddSingleton<IRestaurantDatabase, OptionsRestaurantDatabase>();

            var connStr = Configuration.GetConnectionString("Restaurant");
            services.AddSingleton<IReservationsRepository>(
                new SqlReservationsRepository(connStr));

            var restaurantOptions = new Options.RestaurantOptions();
            Configuration.Bind("Restaurant", restaurantOptions);
            services.AddSingleton(restaurantOptions.ToMaitreD());

            var smtpOptions = new Options.SmtpOptions();
            Configuration.Bind("Smtp", smtpOptions);
            services.AddSingleton(smtpOptions.ToPostOffice());
        }

        private static void ConfigureUrSigning(
            IServiceCollection services,
            byte[] urlSigningKey)
        {
            services.RemoveAll<IUrlHelperFactory>();
            services.AddSingleton<IUrlHelperFactory>(
                new SigningUrlHelperFactory(
                    new UrlHelperFactory(),
                    urlSigningKey));
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            var secret = Configuration["JwtIssuerSigningKey"];

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = "role"
                };
                opts.RequireHttpsMetadata = false;
            });
        }

        public static void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
