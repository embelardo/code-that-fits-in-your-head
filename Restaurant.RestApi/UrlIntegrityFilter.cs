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

namespace Ploeh.Samples.Restaurant.RestApi
{
    [SuppressMessage(
        "Performance",
        "CA1812: Avoid uninstantiated internal classes",
        Justification = "This class is instantiated via Reflection.")]
    internal sealed class UrlIntegrityFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (IsGetHomeRequest(context))
            {
                await next().ConfigureAwait(false);
                return;
            }

            var sig = context.HttpContext.Request.Query["sig"];
            var sigBytes =
                Convert.FromBase64String(sig.ToString());

            var strippedUrl = GetUrlWithoutSignature(context);

            using var hmac =
                new HMACSHA256(Encoding.ASCII.GetBytes(SigningUrlHelper.secret));
            var expectedSignature =
                hmac.ComputeHash(Encoding.ASCII.GetBytes(strippedUrl));
            var signaturesMatch = expectedSignature.SequenceEqual(sigBytes);
            if (!signaturesMatch)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next().ConfigureAwait(false);
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
    }
}
