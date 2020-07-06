/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class UrlBuilder
    {
        private readonly string? action;
        private readonly string? controller;
        private readonly object? values;

        public UrlBuilder()
        {
        }

        private UrlBuilder(string? action, string? controller, object? values)
        {
            this.action = action;
            this.controller = controller;
            this.values = values;
        }

        public UrlBuilder WithAction(string newAction)
        {
            return new UrlBuilder(newAction, controller, values);
        }

        public UrlBuilder WithController(string newController)
        {
            if (newController is null)
                throw new ArgumentNullException(nameof(newController));

            const string controllerSuffix = "controller";
            return new UrlBuilder(
                action,
                newController.Remove(newController.LastIndexOf(
                    controllerSuffix,
                    StringComparison.OrdinalIgnoreCase)),
                values);
        }

        public UrlBuilder WithValues(object newValues)
        {
            return new UrlBuilder(action, controller, newValues);
        }

        public string BuildAbsolute(IUrlHelper url)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            return url.Action(
                action,
                controller,
                values,
                url.ActionContext.HttpContext.Request.Scheme,
                url.ActionContext.HttpContext.Request.Host.ToUriComponent());
        }
    }
}
