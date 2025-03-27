namespace P2 {
    class Program {
        public static uint GetValidUintInput(string prompt) {
            uint result;
            while (true) {
                Console.Write(prompt);
                string input = Console.ReadLine();

                if (uint.TryParse(input, out result) && (result <= int.MaxValue) && (result > 0)) { break; }
                Console.WriteLine($"Invalid input. Please enter a number between 1 and {int.MaxValue}.\n");
                Console.WriteLine("--------------------------------------------------");
            }
            Console.WriteLine("--------------------------------------------------");
            return result;
        }
        static void Main(string[] args) {
            Logger.ResetFile();

            uint maxInstances = GetValidUintInput("[Max Instances]\nMax amount of active dungeons.\n\nEnter Max Instances: ");
            uint dpsCount = GetValidUintInput("[DPS Count]\nTotal DPS players in the queue.\n\nEnter DPS Count: ");
            uint healerCount = GetValidUintInput("[Healer Count]\nTotal Healers in the queue.\n\nEnter Healer Count: ");
            uint tankCount = GetValidUintInput("[Tank Count]\nTotal Tanks in the queue.\n\nEnter Tank Count: ");
            uint minTime = GetValidUintInput("[Minimum Time]\nMinimum time to complete a dungeon.\n\nEnter Min Time: ");
            uint maxTime = GetValidUintInput("[Maximum Time]\nMaximum time to complete a dungeon. Must not be less than minimum time\n\nEnter Max Time: ");

            if (minTime > maxTime) {
                maxTime = minTime;
                Console.WriteLine("[INFO] Maximum time has been set to the minimum time");
            }

            PartyManager pm = PartyManager.Instance;
            pm.SetParameters(maxInstances, dpsCount, healerCount, tankCount, minTime, maxTime);
            pm.Initialize();
            while (pm.running) { }
            Task.Delay(1000).Wait();
            Console.WriteLine("Program Finished");
        }
    }
}