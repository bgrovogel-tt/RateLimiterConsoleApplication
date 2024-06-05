using System;
using System.Collections.Generic;

namespace RateLimiterConsoleApplication
{
    public class RateLimiter
    {
        private readonly Dictionary<TimeSpan, int> _limits;
        private readonly Dictionary<TimeSpan, Queue<DateTime>> _requestTimes;
        private readonly object _lock = new object();

        public RateLimiter(Dictionary<TimeSpan, int> limits)
        {
            _limits = limits ?? throw new ArgumentNullException(nameof(limits));
            _requestTimes = new Dictionary<TimeSpan, Queue<DateTime>>();

            foreach (KeyValuePair<TimeSpan, int> limit in _limits)
            {
                _requestTimes[limit.Key] = new Queue<DateTime>();
            }
        }

        public bool AllowRequest(out TimeSpan remainingTime)
        {
            lock (_lock)
            {
                DateTime now = DateTime.UtcNow;

                remainingTime = TimeSpan.Zero;

                foreach (KeyValuePair<TimeSpan, int> limit in _limits)
                {
                    TimeSpan timeWindow = limit.Key;
                    int maxRequests = limit.Value;

                    Queue<DateTime> requestQueue = _requestTimes[timeWindow];
                    while (requestQueue.Count > 0 && (now - requestQueue.Peek()) > timeWindow)
                    {
                        requestQueue.Dequeue();
                    }

                    if (requestQueue.Count >= maxRequests)
                    {
                        remainingTime = requestQueue.Count > 0 ? timeWindow - (now - requestQueue.Peek()) : timeWindow;
                        Console.WriteLine($"Request Blocked: Exceeded limit of {maxRequests} requests per {timeWindow}. Time remaining: {remainingTime.TotalSeconds} seconds.");
                        return false;
                    }
                }

                foreach (TimeSpan timeWindow in _limits.Keys)
                {
                    _requestTimes[timeWindow].Enqueue(now);
                }

                Console.WriteLine("Request Allowed.");
                return true;
            }
        }
    }
}