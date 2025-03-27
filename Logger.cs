using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2 {
    class PreciseTime {
        private static Stopwatch stopwatch = Stopwatch.StartNew();
        private static DateTime dateTime = DateTime.UtcNow.AddHours(8);

        public static string TimeStamp() {
            long ticks = stopwatch.ElapsedTicks;
            long ns = ticks * (1_000_000_000L / Stopwatch.Frequency);

            DateTime time = dateTime.AddTicks(ns / 100);
            return $"[{time:dd-MM-yyyy HH:mm:ss}.{ns % 1_000_000_000:D9}]:\t";
        }
    }
    class Logger {
        private static string path = "log.txt";
        private static StreamWriter logFile;
        private static void WriteToFile(string message) {
            try {
                File.AppendAllTextAsync(path, message);
            }
            catch (Exception e) {
                Console.WriteLine($"Error writing to file: {e.Message}");
            }
        }

        public static void ResetFile() {
            string path = "log.txt";
            File.WriteAllText(path, string.Empty);
            //logFile = new StreamWriter(path, append: true);
        }

        public static void Log(string update, List<GameInstance> instances, bool writeToFile = false) {
            StringBuilder sb = new();
            sb.Append($"{PreciseTime.TimeStamp()}{update}\n\t\t\t\t\tInstances:");
            foreach (GameInstance instance in instances) {
                sb.Append("\n");
                sb.Append($"\t\t\t\t\tInstance {instance.GetId()}: {instance.GetStatus()}");
            }
            if (writeToFile) { WriteToFile(sb.ToString()); }
            else { Console.WriteLine(sb.ToString()); }
        }

        public static void LogEnd(string update, List<GameInstance> instances, bool writeToFile = false) {
            StringBuilder sb = new();
            sb.Append($"{PreciseTime.TimeStamp()}{update}\n\t\t\t\t\tInstances:");
            foreach (GameInstance instance in instances) {
                sb.Append("\n");
                sb.Append($"\t\t\t\t\tInstance {instance.GetId()}: Hosted {instance.GetPartiesRan()} parties for a total of {instance.GetTotalPartyTime()} seconds.");
            }
            if (writeToFile) { WriteToFile(sb.ToString()); }
            else { Console.WriteLine(sb.ToString()); }
        }
    }
}