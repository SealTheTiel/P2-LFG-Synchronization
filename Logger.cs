using System;
using System.Collections.Concurrent;
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

    class Log {
        private readonly String timeStamp = PreciseTime.TimeStamp();
        private readonly String message;
        private readonly ConcurrentDictionary<uint, Status> ?dictionary;
        private readonly List<GameInstance> ?instances;
        public readonly bool writeToFile;
        
        public Log(string message, ConcurrentDictionary<uint, Status> dictionary, bool writeToFile = false) {
            this.message = message;
            this.dictionary = dictionary;
            this.writeToFile = writeToFile;
        }

        public Log(string message, List<GameInstance> instances, bool writeToFile = false) {
            this.message = message;
            this.instances = instances;
            this.writeToFile = writeToFile;
        }

        public Log(string message, bool writeToFile = false) {
            this.message = message;
            this.writeToFile = writeToFile;
        }

        public override string ToString() {
            string indent = "\t\t\t\t\t";
            if (writeToFile) { indent = "\t\t\t\t\t\t\t\t\t"; }
            StringBuilder sb = new();
            sb.Append($"{timeStamp}{message}");
            if (instances != null) {
                sb.Append($"\n{indent}Instances:");
                foreach (GameInstance instance in instances) { sb.Append($"\n{indent}Instance {instance.GetId()}: Hosted {instance.GetPartiesRan()} parties, and ran for a total of {instance.GetTotalPartyTime()} seconds."); }
            }
            else if (dictionary != null) {
                sb.Append($"\n{indent}Instances:");
                foreach (KeyValuePair<uint, Status> instance in dictionary) { sb.Append($"\n{indent}Instance {instance.Key}: {instance.Value}"); }
            }
            sb.Append("\n");
            return sb.ToString();
        }
    }

   class Logger {
        private static string path = "log.txt";
        private static List<Task> logTasks = new();
        public static bool isLogging = true;

        private static readonly ConcurrentQueue<Log> logQueue = new();
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private static bool isProcessing = false;

        private static async Task ProcessLogQueue() {
            isProcessing = true;
            while (isLogging) {
                if (logQueue.TryDequeue(out Log log)) { await WriteLog(log); }
                else { await Task.Delay(10); }
            }
            isProcessing = false;
        }

        public static void EnqueueLog(Log log) { logQueue.Enqueue(log); }

        private static async Task WriteLog(Log log) {
            try {
                await semaphore.WaitAsync();
                if (log.writeToFile) { await File.AppendAllTextAsync(path, log.ToString()); }
                else { Console.WriteLine(log.ToString()); }
            }
            finally { semaphore.Release(); }
        }

        public static void ResetFile() {
            string path = "log.txt";
            File.WriteAllText(path, string.Empty);
        }

        public static async Task LogStart(bool writeToFile = false) {
            Log log = new("Simulation Started", writeToFile);
            EnqueueLog(log);
            Console.Write(log.ToString());
            Task.Run(() => ProcessLogQueue());
        }

        public static async Task LogEnd(string update, List<GameInstance> instances, bool writeToFile = false) {
            Log log = new(update, instances, writeToFile);
            EnqueueLog(log);
            Log ending = new Log("Simulation Finished.", writeToFile);
            EnqueueLog(ending);
            if (writeToFile) { Console.Write(ending.ToString()); }
            while (logQueue.Count > 0) { await Task.Delay(10); }
            Log finish = new Log("Logger Finished.", writeToFile);
            EnqueueLog(finish);
            if (writeToFile) { Console.Write(finish.ToString()); }

            isLogging = false;
        }
    }
}