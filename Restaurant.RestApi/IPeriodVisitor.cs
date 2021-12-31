/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿namespace Ploeh.Samples.Restaurants.RestApi
{
    internal interface IPeriodVisitor<T>
    {
        T VisitYear(int year);
        T VisitMonth(int year, int month);
        T VisitDay(int year, int month, int day);
    }
}