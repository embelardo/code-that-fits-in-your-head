/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    internal sealed class JwtTokenGenerator
    {
        private readonly IEnumerable<string> roles;
        private readonly IEnumerable<int> restaurantIds;

        internal JwtTokenGenerator(
            IEnumerable<int> restaurantIds,
            IEnumerable<string> roles)
        {
            this.roles = roles;
            this.restaurantIds = restaurantIds;
        }

        internal JwtTokenGenerator(
            IEnumerable<int> restaurantIds,
            params string[] roles) :
            this(restaurantIds, roles.AsEnumerable())
        {

        }

        internal string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                "This is not the secret used in production.");

            var restaurantClaims = restaurantIds
                .Select(id => new Claim("restaurant", $"{id}"));
            var roleClaims = roles.Select(r => new Claim("role", r));
            var claims = restaurantClaims.Concat(roleClaims).ToArray();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
