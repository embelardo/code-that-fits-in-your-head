/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurants.RestApi
{
    public interface IEitherVisitor<TL, TR, T>
    {
        T Visit(TL l);
        T Visit(TR r);
    }
}