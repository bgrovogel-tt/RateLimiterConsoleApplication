using System;
using System.Collections.Generic;

namespace RateLimiterConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set the rate limits: 3 requests per minute, 5 per hour, 9 per day
            var limits = new Dictionary<TimeSpan, int>
            {
                { TimeSpan.FromMinutes(1), 3 },
                { TimeSpan.FromHours(1), 5 },
                { TimeSpan.FromDays(1), 9 }
            };

            RateLimiter rateLimiter = new RateLimiter(limits);

            Console.WriteLine("Press Enter to make a request. Press 'Q' to quit.");

            // Loop until the user quits
            while (true)
            {
                Console.WriteLine("Waiting for user input...");
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    TimeSpan remainingTime;
                    bool allowed = rateLimiter.AllowRequest(out remainingTime);
                    if (!allowed)
                    {
                        Console.WriteLine($"Request Blocked. Time remaining: {remainingTime.TotalSeconds} seconds.");
                    }
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
    }
}