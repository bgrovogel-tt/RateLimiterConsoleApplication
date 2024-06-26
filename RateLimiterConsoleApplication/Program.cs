using System;
using System.Linq;
using System.Reflection;

namespace RateLimiterConsoleApplication
{
    public class Program
    {
        private static RateLimiterAttribute[] _rateLimiterAttributes;

        [RateLimiter(TimeUnit.Minute, 2)]
        [RateLimiter(TimeUnit.Hour, 3)]
        [RateLimiter(TimeUnit.Day, 4)]
        public static void Main(string[] args)
        {
            InitializeRateLimiters();
            Console.WriteLine("Press Enter to make a request. Press 'Q' to quit.");

            while (true)
            {
                Console.WriteLine("Waiting for user input...");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    InvokeRateLimitedMethod(nameof(HandleRequest));
                }
                else if (keyInfo.Key == ConsoleKey.Q)
                {
                    Console.WriteLine("Exiting the program...");
                    break; // Exit the loop if 'Q' is pressed
                }
                else
                {
                    Console.WriteLine("Invalid input. Press Enter to make a request. Press 'Q' to quit.");
                }
            }
        }

        private static void HandleRequest()
        {
            Console.WriteLine("Request Handled.");
        }

        private static void InitializeRateLimiters()
        {
            MethodInfo method = typeof(Program).GetMethod(nameof(Main), BindingFlags.Static | BindingFlags.Public);
            var attributes = method.GetCustomAttributes(typeof(RateLimiterAttribute), true)
                .Cast<RateLimiterAttribute>()
                .OrderByDescending(attr => attr.TimeUnit) // Ensure higher time units are checked first
                .ToArray();

            _rateLimiterAttributes = attributes;

            foreach (var attribute in _rateLimiterAttributes)
            {
                attribute.InitializeRateLimiter();
            }
        }

        private static void InvokeRateLimitedMethod(string methodName)
        {
            if (_rateLimiterAttributes != null)
            {
                TimeSpan maxRemainingTime = TimeSpan.Zero;
                bool allowed = true;

                foreach (var attribute in _rateLimiterAttributes)
                {
                    TimeSpan remainingTime;
                    if (!attribute.AllowRequest(out remainingTime))
                    {
                        allowed = false;
                        maxRemainingTime = remainingTime;
                        break; // Break the loop on first blocked request
                    }
                }

                if (allowed)
                {
                    MethodInfo method = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
                    method?.Invoke(null, null);
                }
                else
                {
                    Console.WriteLine($"Request Blocked. Time remaining: {maxRemainingTime.TotalSeconds} seconds.");
                }
            }
            else
            {
                MethodInfo method = typeof(Program).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
                method?.Invoke(null, null);
            }
        }
    }
}
