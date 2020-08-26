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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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
            services
                .AddControllers(opts => opts.Filters.Add<LinksFilter>())
                .AddJsonOptions(opts =>
                    opts.JsonSerializerOptions.IgnoreNullValues = true);

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Let's hope that this generates more than 128 bytes...")),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = "role"
                };
                opts.RequireHttpsMetadata = false;
            });

            var connStr = Configuration.GetConnectionString("Restaurant");
            services.AddSingleton<IReservationsRepository>(
                new SqlReservationsRepository(connStr));

            var restaurantSettings = new Settings.RestaurantSettings();
            Configuration.Bind("Restaurant", restaurantSettings);
            services.AddSingleton(restaurantSettings.ToMaitreD());

            var smtpSettings = new Settings.SmtpSettings();
            Configuration.Bind("Smtp", smtpSettings);
            services.AddSingleton(smtpSettings.ToPostOffice());
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
