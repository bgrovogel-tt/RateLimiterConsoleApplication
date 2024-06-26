using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiterConsoleApplication
{
    public class RateLimiter
    {
        private readonly Dictionary<TimeSpan, int> _limits;
        private readonly Dictionary<TimeSpan, LinkedList<DateTime>> _requestTimes;
        private readonly object _lock = new object();

        public RateLimiter(Dictionary<TimeSpan, int> limits)
        {
            _limits = limits ?? throw new ArgumentNullException(nameof(limits));
            _requestTimes = new Dictionary<TimeSpan, LinkedList<DateTime>>();

            foreach (KeyValuePair<TimeSpan, int> limit in _limits)
            {
                _requestTimes[limit.Key] = new LinkedList<DateTime>();
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

                    LinkedList<DateTime> requestList = _requestTimes[timeWindow];
                    while (requestList.Count > 0 && (now - requestList.First.Value) > timeWindow)
                    {
                        requestList.RemoveFirst();
                    }

                    if (requestList.Count >= maxRequests)
                    {
                        remainingTime = requestList.Count > 0 ? timeWindow - (now - requestList.First.Value) : timeWindow;
                        Console.WriteLine($"Request Blocked: Exceeded limit of {maxRequests} requests per {timeWindow}. Time remaining: {remainingTime.TotalSeconds} seconds.");
                        return false;
                    }
                }

                foreach (TimeSpan timeWindow in _limits.Keys)
                {
                    _requestTimes[timeWindow].AddLast(now);
                }

                Console.WriteLine($"[{now}] Request Allowed.");
                return true;
            }
        }
    }
}
