namespace P2 {
    class Program {
        public static uint GetValidUintInput(string prompt, string inputMessage, uint min, uint max) {
            uint result;
            while (true) {
                Console.WriteLine(prompt);
                Console.WriteLine($"[{min}, {max}]\n");
                Console.Write(inputMessage);
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

            uint maxInstances = GetValidUintInput("[Max Instances]\nMax amount of active dungeons.", "Enter Max Instances: ", 1, 5000);
            uint dpsCount = GetValidUintInput("[DPS Count]\nTotal DPS players in the queue.", "Enter DPS Count: ", 1, int.MaxValue);
            uint healerCount = GetValidUintInput("[Healer Count]\nTotal Healers in the queue.", "Enter Healer Count: ", 1, int.MaxValue);
            uint tankCount = GetValidUintInput("[Tank Count]\nTotal Tanks in the queue.", "Enter Tank Count: ", 1, int.MaxValue);
            uint minTime = GetValidUintInput("[Minimum Time]\nMinimum time to complete a dungeon.", "Enter Min Time: ", 0, 15);
            uint maxTime = GetValidUintInput("[Maximum Time]\nMaximum time to complete a dungeon. Must not be less than minimum time", "Enter Max Time: ", minTime, 15);
            uint logToFile = GetValidUintInput("[Log Medium]\nMedium where logs will be printed\n[0] Console (recommended for low number of instances)\n[1] File (recommended for many instances)", "Enter Log to File: ", 0, 1);
            uint verboseLog = GetValidUintInput("[Verbose Logging]\nPrint logs every event\n[0] No\n[1] Yes", "Enter Verbose Logging: ", 0, 1);

            PartyManager pm = PartyManager.Instance;
            pm.SetParameters(maxInstances, dpsCount, healerCount, tankCount, minTime, maxTime, logToFile, verboseLog);
            pm.Initialize();
            while (pm.running && Logger.isLogging) { }
            Task.Delay(1000).Wait();
            Console.WriteLine($"{PreciseTime.TimeStamp()}End of Program");
        }
    }
}