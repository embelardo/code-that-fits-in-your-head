/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Either<TL, TR>
    {
        private readonly IEither imp;

        private Either(Either<TL, TR>.IEither imp)
        {
            this.imp = imp;
        }

        internal static Either<TL, TR> CreateLeft(TL left)
        {
            return new Either<TL, TR>(new Left(left));
        }

        internal static Either<TL, TR> CreateRight(TR right)
        {
            return new Either<TL, TR>(new Right(right));
        }

        public T Accept<T>(IEitherVisitor<TL, TR, T> visitor)
        {
            return imp.Accept(visitor);
        }

        public override bool Equals(object? obj)
        {
            return obj is Either<TL, TR> either &&
                   EqualityComparer<Either<TL, TR>.IEither>.Default.Equals(imp, either.imp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(imp);
        }

        private interface IEither
        {
            T Accept<T>(IEitherVisitor<TL, TR, T> visitor);
        }

        private sealed class Left : IEither
        {
            private readonly TL left;

            public Left(TL left)
            {
                this.left = left;
            }

            public T Accept<T>(IEitherVisitor<TL, TR, T> visitor)
            {
                return visitor.VisitLeft(left);
            }

            public override bool Equals(object? obj)
            {
                return obj is Either<TL, TR>.Left left &&
                       EqualityComparer<TL>.Default.Equals(this.left, left.left);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(left);
            }
        }

        private sealed class Right : IEither
        {
            private readonly TR right;

            public Right(TR right)
            {
                this.right = right;
            }

            public T Accept<T>(IEitherVisitor<TL, TR, T> visitor)
            {
                return visitor.VisitRight(right);
            }

            public override bool Equals(object? obj)
            {
                return obj is Either<TL, TR>.Right right &&
                       EqualityComparer<TR>.Default.Equals(this.right, right.right);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(right);
            }
        }
    }

    public static class Either
    {
        public static Either<TL, TR> CreateLeft<TL, TR>(TL left)
        {
            return Either<TL, TR>.CreateLeft(left);
        }

        public static Either<TL, TR> CreateRight<TL, TR>(TR right)
        {
            return Either<TL, TR>.CreateRight(right);
        }

        public static Either<TL, TR2> Select<TL, TR1, TR2>(
            this Either<TL, TR1> source,
            Func<TR1, TR2> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.Accept(new SelectVisitor<TL, TR1, TR2>(selector));
        }

        private class SelectVisitor<TL, TR1, TR2> :
            IEitherVisitor<TL, TR1, Either<TL, TR2>>
        {
            private Func<TR1, TR2> selector;

            public SelectVisitor(Func<TR1, TR2> selector)
            {
                this.selector = selector;
            }

            public Either<TL, TR2> VisitLeft(TL l)
            {
                return CreateLeft<TL, TR2>(l);
            }

            public Either<TL, TR2> VisitRight(TR1 r)
            {
                return CreateRight<TL, TR2>(selector(r));
            }
        }

        public static Either<TL, TR2> SelectMany<TL, TR1, TR2>(
            this Either<TL, TR1> source,
            Func<TR1, Either<TL, TR2>> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return
                source.Accept(new SelectManyVisitor<TL, TR1, TR2>(selector));
        }

        private class SelectManyVisitor<TL, TR1, TR2> :
            IEitherVisitor<TL, TR1, Either<TL, TR2>>
        {
            private Func<TR1, Either<TL, TR2>> selector;

            public SelectManyVisitor(Func<TR1, Either<TL, TR2>> selector)
            {
                this.selector = selector;
            }

            public Either<TL, TR2> VisitLeft(TL l)
            {
                return CreateLeft<TL, TR2>(l);
            }

            public Either<TL, TR2> VisitRight(TR1 r)
            {
                return selector(r);
            }
        }

        public static T Bifold<T>(this Either<T, T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.Accept(new BifoldVisitor<T>());
        }

        private class BifoldVisitor<T> : IEitherVisitor<T, T, T>
        {
            public T VisitLeft(T l)
            {
                return l;
            }

            public T VisitRight(T r)
            {
                return r;
            }
        }
    }
}
