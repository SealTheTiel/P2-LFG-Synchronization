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
        public Logger() { }
        public void Log(string update, List<GameInstance> instances) {
            Console.Write(PreciseTime.TimeStamp() + update);
            foreach (GameInstance instance in instances) {
                Console.Write("\n");
                Console.Write($"\t\t\t\tInstance {instance.GetId()}: {instance.GetStatus()}");
            }
            Console.Write("\n");
        }
    }
}