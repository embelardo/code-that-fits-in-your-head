/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class SigningUrlHelperFactory : IUrlHelperFactory
    {
        private readonly IUrlHelperFactory inner;

        public SigningUrlHelperFactory(IUrlHelperFactory inner)
        {
            this.inner = inner;
        }

        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var url = inner.GetUrlHelper(context);
            return new SigningUrlHelper(url);
        }
    }
}
