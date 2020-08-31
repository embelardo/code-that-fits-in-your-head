/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
    internal sealed class SigningUrlHelper : IUrlHelper
    {
        private readonly IUrlHelper inner;
        public const string secret = "The very secret secret that's checked into source contro.";

        public SigningUrlHelper(IUrlHelper inner)
        {
            this.inner = inner;
        }

        public ActionContext ActionContext
        {
            get { return inner.ActionContext; }
        }

        public string Action(UrlActionContext actionContext)
        {
            var url = inner.Action(actionContext);
            var ub = new UriBuilder(url);

            using var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secret));
            var sig = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.ASCII.GetBytes(url)));

            ub.Query = new QueryString(ub.Query)
                .Add("sig", sig)
                .ToString();
            return ub.ToString();
        }

        public string Content(string contentPath)
        {
            return inner.Content(contentPath);
        }

        [SuppressMessage(
            "Design",
            "CA1055:URI-like return values should not be strings",
            Justification = "Interface implementation. Can't change types.")]
        public bool IsLocalUrl(string url)
        {
            return inner.IsLocalUrl(url);
        }

        public string Link(string routeName, object values)
        {
            return inner.Link(routeName, values);
        }

        [SuppressMessage(
            "Design",
            "CA1055:URI-like return values should not be strings",
            Justification = "Interface implementation. Can't change types.")]
        public string RouteUrl(UrlRouteContext routeContext)
        {
            return inner.RouteUrl(routeContext);
        }
    }
}
