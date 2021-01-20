/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    internal sealed class SigningUrlHelperFactory : IUrlHelperFactory
    {
        private readonly IUrlHelperFactory inner;
        private readonly byte[] urlSigningKey;

        public SigningUrlHelperFactory(
            IUrlHelperFactory inner,
            byte[] urlSigningKey)
        {
            this.inner = inner;
            this.urlSigningKey = urlSigningKey;
        }

        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var url = inner.GetUrlHelper(context);
            return new SigningUrlHelper(url, urlSigningKey);
        }
    }
}
