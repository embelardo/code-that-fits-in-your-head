/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class UrlIntegrityFilter : IAsyncActionFilter
    {
        private readonly byte[] urlSigningKey;

        public UrlIntegrityFilter(byte[] urlSigningKey)
        {
            this.urlSigningKey = urlSigningKey;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (IsGetHomeRequest(context))
            {
                await next().ConfigureAwait(false);
                return;
            }

            var strippedUrl = GetUrlWithoutSignature(context);
            if (SignatureIsValid(strippedUrl, context))
            {
                await next().ConfigureAwait(false);
                return;
            }

            context.Result = new NotFoundResult();
        }

        private static bool IsGetHomeRequest(ActionExecutingContext context)
        {
            return context.HttpContext.Request.Path == "/"
                && context.HttpContext.Request.Method == "GET";
        }

        private static string GetUrlWithoutSignature(
            ActionExecutingContext context)
        {
            var restOfQuery = QueryString.Create(
                context.HttpContext.Request.Query.Where(x => x.Key != "sig"));

            var url = context.HttpContext.Request.GetEncodedUrl();
            var ub = new UriBuilder(url);
            ub.Query = restOfQuery.ToString();
            return ub.Uri.AbsoluteUri;
        }

        private bool SignatureIsValid(
            string candidate,
            ActionExecutingContext context)
        {
            var sig = context.HttpContext.Request.Query["sig"];
            var receivedSignature = Convert.FromBase64String(sig.ToString());

            using var hmac = new HMACSHA256(urlSigningKey);
            var computedSignature =
                hmac.ComputeHash(Encoding.ASCII.GetBytes(candidate));

            var signaturesMatch =
                computedSignature.SequenceEqual(receivedSignature);
            return signaturesMatch;
        }
    }
}
