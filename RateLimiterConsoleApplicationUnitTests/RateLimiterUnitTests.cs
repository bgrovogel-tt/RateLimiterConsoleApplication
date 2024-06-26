using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Reflection;

namespace RateLimiterConsoleApplication.Tests
{
    [TestFixture]
    public class RateLimiterTests
    {
        private RateLimiterAttribute[] _rateLimiterAttributes;

        [SetUp]
        public void Setup()
        {
            // Initialize rate limiter attributes for testing
            _rateLimiterAttributes = new[]
            {
                new RateLimiterAttribute(TimeUnit.Minute, 2),
                new RateLimiterAttribute(TimeUnit.Hour, 3),
                new RateLimiterAttribute(TimeUnit.Day, 4)
            };

            // Initialize rate limiters
            foreach (var attribute in _rateLimiterAttributes)
            {
                attribute.InitializeRateLimiter();
            }
        }

        [Test]
        public void AllowRequest_Allowed()
        {
            TimeSpan remainingTime;

            bool allowed = _rateLimiterAttributes[0].AllowRequest(out remainingTime);

            Assert.That(allowed, Is.EqualTo(true));
            Assert.That(TimeSpan.Zero, Is.EqualTo(remainingTime));
        }

        [Test]
        public void AllowRequest_BlockMinute()
        {
            TimeSpan remainingTime;
            for (int i = 0; i < 2; i++)
            {
                _rateLimiterAttributes[0].AllowRequest(out remainingTime);
            }

            bool allowed = _rateLimiterAttributes[0].AllowRequest(out remainingTime);

            Assert.That(allowed, Is.False);
            Assert.That(remainingTime, Is.GreaterThan(TimeSpan.Zero));
        }

        [Test]
        public void AllowRequest_BlockHour()
        {
            TimeSpan remainingTime;
            for (int i = 0; i < 3; i++)
            {
                _rateLimiterAttributes[1].AllowRequest(out remainingTime);
            }

            bool allowed = _rateLimiterAttributes[1].AllowRequest(out remainingTime);

            Assert.That(allowed, Is.False);
            Assert.That(remainingTime, Is.GreaterThan(TimeSpan.Zero));
        }

        [Test]
        public void AllowRequest_BlockDay()
        {
            TimeSpan remainingTime;
            for (int i = 0; i < 4; i++)
            {
                _rateLimiterAttributes[2].AllowRequest(out remainingTime);
            }

            bool allowed = _rateLimiterAttributes[2].AllowRequest(out remainingTime);

            Assert.That(allowed, Is.False);
            Assert.That(remainingTime, Is.GreaterThan(TimeSpan.Zero));
        }

        [Test]
        public void InvokeRateLimitedMethod_Allowed()
        {
            var methodInfo = typeof(RateLimiterTests).GetMethod(nameof(MockMethod), BindingFlags.Static | BindingFlags.NonPublic);

            object result = methodInfo.Invoke(null, null);

            Assert.That(result, Is.Not.Null);
        }

        private static object MockMethod()
        {
            return new object();
        }
    }
}
