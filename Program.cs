namespace P2 {
    class Program {
        public static uint GetValidUintInput(string prompt, uint min, uint max) {
            uint result;
            while (true) {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (uint.TryParse(input, out result) && (result <= max) && (result >= min)) { break; }
                Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.\n");
                Console.WriteLine("--------------------------------------------------");
            }
            Console.WriteLine("--------------------------------------------------");
            return result;
        }
        static void Main(string[] args) {
            Logger.ResetFile();

            uint maxInstances = GetValidUintInput("[Max Instances]\nMax amount of active dungeons.\n\nEnter Max Instances: ", 1, 5000);
            uint dpsCount = GetValidUintInput("[DPS Count]\nTotal DPS players in the queue.\n\nEnter DPS Count: ", 1, int.MaxValue);
            uint healerCount = GetValidUintInput("[Healer Count]\nTotal Healers in the queue.\n\nEnter Healer Count: ", 1, int.MaxValue);
            uint tankCount = GetValidUintInput("[Tank Count]\nTotal Tanks in the queue.\n\nEnter Tank Count: ", 1, int.MaxValue);
            uint minTime = GetValidUintInput("[Minimum Time]\nMinimum time to complete a dungeon.\n\nEnter Min Time: ", 0, int.MaxValue);
            uint maxTime = GetValidUintInput("[Maximum Time]\nMaximum time to complete a dungeon. Must not be less than minimum time\n\nEnter Max Time: ", minTime, int.MaxValue);
            uint logToFile = GetValidUintInput("[Log Medium]\nMedium where logs will be printed\n[0] Console (recommended for low number of instances)\n[1] File (recommended for many instances)\n\nEnter Log to File: ", 0, 1);
            uint verboseLog = GetValidUintInput("[Verbose Logging]\nPrint logs every event\n[0] No\n[1] Yes\n\nEnter Verbose Logging: ", 0, 1);

            PartyManager pm = PartyManager.Instance;
            pm.SetParameters(maxInstances, dpsCount, healerCount, tankCount, minTime, maxTime, logToFile, verboseLog);
            pm.Initialize();
            while (pm.running && Logger.isLogging) { }
            Task.Delay(1000).Wait();
            Console.WriteLine($"{PreciseTime.TimeStamp()}End of Program");
        }
    }
}