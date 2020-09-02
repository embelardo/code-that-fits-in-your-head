/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class LoggingClock : IClock
    {
        public LoggingClock(ILogger<LoggingClock> logger, IClock inner)
        {
            Logger = logger;
            Inner = inner;
        }

        public ILogger<LoggingClock> Logger { get; }
        public IClock Inner { get; }

        public DateTime GetCurrentDateTime()
        {
            var output = Inner.GetCurrentDateTime();
            Logger.LogInformation(
                "{method}() => {output}",
                nameof(GetCurrentDateTime),
                output);
            return output;
        }
    }
}
