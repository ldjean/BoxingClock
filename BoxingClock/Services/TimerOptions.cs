using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxingClock.Services
{
    public static class TimerOptions
    {
        public static List<string> RoundsOptions => GenerateNumberList("∞", 20);
        public static List<string> RoundTimes => GenerateRoundTimeOptions();
        public static List<string> BreakTimes => GenerateBreakTimeOptions();
        public static List<string> Intervals => new List<string> { "Off", "10", "15", "30", "60" };
        public static List<string> ReadyTimes => new List<string> { "Off", "5", "10", "15" };

        private static List<string> GenerateNumberList(string start, int final)
        {
            var numList = new List<string> { start };
            for (int i = 1; i <= final; i++)
            {
                numList.Add(i.ToString());
            }
            return numList;
        }

        private static List<string> GenerateRoundTimeOptions()
        {
            const int minSeconds = 30;      // 30 seconds minimum
            const int maxSeconds = 1800;    // 30 minutes maximum
            const int stepSeconds = 5;      // 5 second increments

            return GenerateTimeOptions(minSeconds, maxSeconds, stepSeconds, includeOff: false);
        }

        private static List<string> GenerateBreakTimeOptions()
        {
            const int minSeconds = 15;      // 15 seconds minimum
            const int maxSeconds = 600;     // 10 minutes maximum
            const int stepSeconds = 5;      // 5 second increments

            return GenerateTimeOptions(minSeconds, maxSeconds, stepSeconds, includeOff: true);
        }

        private static List<string> GenerateTimeOptions(int minSeconds, int maxSeconds, int stepSeconds, bool includeOff)
        {
            var times = new List<string>();

            if (includeOff)
            {
                times.Add("Off");
            }

            for (int totalSeconds = minSeconds; totalSeconds <= maxSeconds; totalSeconds += stepSeconds)
            {
                var timeSpan = TimeSpan.FromSeconds(totalSeconds);

                // Format based on duration
                if (timeSpan.TotalHours >= 1)
                {
                    times.Add(timeSpan.ToString(@"h\:mm\:ss"));
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    times.Add(timeSpan.ToString(@"m\:ss"));
                }
                else
                {
                    times.Add(timeSpan.ToString(@"m\:ss"));
                }
            }

            return times;
        }
    }
}