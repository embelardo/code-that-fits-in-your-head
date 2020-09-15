/* Copyright (c) Mark Seemann 2020. All rights reserved. */
namespace Ploeh.Samples.Restaurants.RestApi
{
    public interface IEitherVisitor<TL, TR, T>
    {
        T VisitLeft(TL l);
        T VisitRight(TR r);
    }
}