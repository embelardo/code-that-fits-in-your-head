/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Email
    {
        private readonly string value;

        public Email(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }

        public static bool operator ==(Email x, Email y)
        {
            return Equals(x?.value, y?.value);
        }

        public static bool operator !=(Email x, Email y)
        {
            return !(x == y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Email email &&
                   value == email.value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value);
        }
    }
}
