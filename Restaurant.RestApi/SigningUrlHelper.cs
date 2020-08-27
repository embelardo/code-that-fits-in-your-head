/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    internal sealed class SigningUrlHelper : IUrlHelper
    {
        private readonly IUrlHelper inner;

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
            return inner.Action(actionContext);
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
