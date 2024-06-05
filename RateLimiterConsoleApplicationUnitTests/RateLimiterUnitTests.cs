using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using RateLimiterConsoleApplication;

namespace RateLimiterConsoleApplicationUnitTests
{
    public class RateLimiterUnitTests
    {
        [Test]
        public void TestRequestAllowed()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 },
                { TimeSpan.FromHours(1), 20 },
                { TimeSpan.FromDays(1), 100 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act
            bool allowed = rateLimiter.AllowRequest(out _);

            // Assert
            Assert.That(allowed, Is.True);
        }

        [Test]
        public void TestRequestBlocked()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 },
                { TimeSpan.FromHours(1), 20 },
                { TimeSpan.FromDays(1), 100 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act
            // Fill the request queue with maximum number of requests
            for (int i = 0; i < 5; i++)
            {
                rateLimiter.AllowRequest(out _);
            }

            // Try to make one more request
            bool allowed = rateLimiter.AllowRequest(out TimeSpan remainingTime);

            // Assert
            Assert.That(allowed, Is.False);
            Assert.That(remainingTime > TimeSpan.Zero, Is.True);
        }

        [Test]
        public void TestRateLimitWithinOneMinute()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                Assert.That(rateLimiter.AllowRequest(out _), Is.True);
            }
            Assert.That(rateLimiter.AllowRequest(out _), Is.False);
        }

        [Test]
        public void TestRemainingTimeCalculation()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act
            for (int i = 0; i < 5; i++)
            {
                rateLimiter.AllowRequest(out _);
            }
            bool allowed = rateLimiter.AllowRequest(out TimeSpan remainingTime);

            // Assert
            Assert.That(allowed, Is.False);
            Assert.That(remainingTime, Is.LessThanOrEqualTo(TimeSpan.FromMinutes(1)));
        }

        [Test]
        public void TestRateLimitResetAfterTimeLimit()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromSeconds(1), 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                Assert.That(rateLimiter.AllowRequest(out _), Is.True);
            }
            Assert.That(rateLimiter.AllowRequest(out _), Is.False);

            // Wait for the time limit to pass
            Thread.Sleep(1000);

            // The rate limit should be reset now
            Assert.That(rateLimiter.AllowRequest(out _), Is.True);
        }

        [Test]
        public void TestRapidSuccessionRequests()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                if (i < 5)
                {
                    Assert.That(rateLimiter.AllowRequest(out _), Is.True);
                }
                else
                {
                    Assert.That(rateLimiter.AllowRequest(out _), Is.False);
                }
            }
        }

        [Test]
        public void TestNoRequestsMade()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act
            bool allowed = rateLimiter.AllowRequest(out TimeSpan remainingTime);

            // Assert
            Assert.That(allowed, Is.True);
            Assert.That(remainingTime, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void TestZeroTimeLimit()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.Zero, 5 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                Assert.That(rateLimiter.AllowRequest(out _), Is.True);
            }
            Assert.That(rateLimiter.AllowRequest(out _), Is.False);
        }

        [Test]
        public void TestZeroRequestLimit()
        {
            // Arrange
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 0 }
            };
            RateLimiter rateLimiter = new RateLimiter(limits);

            // Act
            bool allowed = rateLimiter.AllowRequest(out TimeSpan remainingTime);

            // Assert
            Assert.That(allowed, Is.False);
            Assert.That(remainingTime, Is.EqualTo(TimeSpan.FromMinutes(1)));
        }

    }
}
