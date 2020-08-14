/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurant.RestApi
{
    public sealed class Occurrence<T>
    {
        public Occurrence(DateTime at, T value)
        {
            At = at;
            Value = value;
        }

        public DateTime At { get; }
        public T Value { get; }

        public Occurrence<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            return new Occurrence<TResult>(At, selector(Value));
        }

        public override bool Equals(object? obj)
        {
            return obj is Occurrence<T> occurrence &&
                   At == occurrence.At &&
                   EqualityComparer<T>.Default.Equals(Value, occurrence.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(At, Value);
        }
    }

    public static class Occurrence
    {
        public static Occurrence<T> At<T>(this T value, DateTime at)
        {
            return new Occurrence<T>(at, value);
        }
    }
}
