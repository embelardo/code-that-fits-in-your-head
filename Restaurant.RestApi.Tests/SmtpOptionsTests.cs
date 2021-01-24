/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using Ploeh.Samples.Restaurants.RestApi.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class SmtpOptionsTests
    {
        [Theory]
        [InlineData("", 587, "foo", "bar", "w@example.com")]
        [InlineData(null, 588, "boo", "far", "a@example.com")]
        [InlineData("m.example.org", 2525, "", "qux", "w@example.com")]
        [InlineData("m.example.com", 525, null, "xuq", "w@example.net")]
        [InlineData("n.example.org", 465, "quux", "", "x@example.com")]
        [InlineData("n.example.com", 466, "xuuq", null, "x@example.org")]
        [InlineData("o.example.org", 25, "cuuz", "corge", "")]
        [InlineData("o.example.net", 24, "zuuc", "gorce", null)]
        public void ToPostOfficeReturnsNullOffice(
            string host,
            int port,
            string userName,
            string password,
            string fromAddress)
        {
            var sut = new SmtpOptions
            {
                Host = host,
                Port = port,
                UserName = userName,
                Password = password,
                FromAddress = fromAddress
            };
            IPostOffice actual =
                sut.ToPostOffice(new InMemoryRestaurantDatabase());
            Assert.Equal(NullPostOffice.Instance, actual);
        }

        [Theory]
        [InlineData("m.example.net", 587, "grault", "garply", "g@example.org")]
        [InlineData("n.example.net", 465, "corge", "waldo", "c@example.org")]
        public void ToPostOfficeReturnsSmtpOffice(
            string host,
            int port,
            string userName,
            string password,
            string from)
        {
            var sut = Create.SmtpOptions(host, port, userName, password, from);

            var db = new InMemoryRestaurantDatabase();
            var actual = sut.ToPostOffice(db);

            var expected =
                new SmtpPostOffice(host, port, userName, password, from, db);
            Assert.Equal(expected, actual);
        }
    }
}
