/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
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

            var index = newController.LastIndexOf(
                controllerSuffix,
                StringComparison.OrdinalIgnoreCase);
            if (0 <= index)
                newController = newController.Remove(index);
            return new UrlBuilder(action, newController, values);
        }

        public UrlBuilder WithValues(object newValues)
        {
            return new UrlBuilder(action, controller, newValues);
        }

        public Uri BuildAbsolute(IUrlHelper url)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            var actionUrl = url.Action(
                action,
                controller,
                values,
                url.ActionContext.HttpContext.Request.Scheme,
                url.ActionContext.HttpContext.Request.Host.ToUriComponent());
            return new Uri(actionUrl);
        }

        public override bool Equals(object? obj)
        {
            return obj is UrlBuilder builder &&
                   action == builder.action &&
                   controller == builder.controller &&
                   EqualityComparer<object?>.Default.Equals(values, builder.values);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(action, controller, values);
        }
    }
}
