using System;
using System.Collections.Generic;
using System.Reflection;

namespace RateLimiterConsoleApplication
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RateLimiterAttribute : Attribute
    {
        public TimeUnit TimeUnit { get; }
        public int Count { get; }

        private RateLimiter _rateLimiter;

        public RateLimiterAttribute(TimeUnit timeUnit, int count)
        {
            TimeUnit = timeUnit;
            Count = count;
        }

        public void InitializeRateLimiter()
        {
            if (_rateLimiter == null)
            {
                TimeSpan timeSpan;
                switch (TimeUnit)
                {
                    case TimeUnit.Minute:
                        timeSpan = TimeSpan.FromMinutes(1);
                        break;
                    case TimeUnit.Hour:
                        timeSpan = TimeSpan.FromHours(1);
                        break;
                    case TimeUnit.Day:
                        timeSpan = TimeSpan.FromDays(1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(TimeUnit), TimeUnit, null);
                }

                _rateLimiter = new RateLimiter(
                    TimeUnit, 
                    new Dictionary<TimeSpan, int>
                    {
                        { timeSpan, Count }
                    });
            }
        }

        public bool AllowRequest(out TimeSpan remainingTime)
        {
            return _rateLimiter.AllowRequest(out remainingTime);
        }
    }
}