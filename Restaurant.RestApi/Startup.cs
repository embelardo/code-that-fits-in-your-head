/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
