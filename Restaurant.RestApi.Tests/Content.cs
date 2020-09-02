/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal static class Content
    {
        public static async Task<T> ParseJsonContent<T>(
            this HttpResponseMessage msg)
        {
            var json = await msg.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return dto;
        }
    }
}
