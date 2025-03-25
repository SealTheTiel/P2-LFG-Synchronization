using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P2 {
    class Party {
        private readonly uint id;
        private uint timeLimit;
        private readonly uint minTime;
        private readonly uint maxTime;

        public Party(uint id, uint minTime, uint maxTime) {
            this.id = id;
            this.minTime = minTime;
            this.maxTime = maxTime;
        }

        public async Task StartPartyAsync(SemaphoreSlim semaphore) {
            PartyManager PM = PartyManager.Instance;
            timeLimit = (uint)PM.random.Next((int)minTime, (int)maxTime);

            PM.NotifyPartyStart(this);
            Console.WriteLine($"Party {id} has started with a time limit of {timeLimit} seconds.");

            await Task.Delay((int)timeLimit * 1000);

            Console.WriteLine($"Party {id} has finished.");
            PM.NotifyPartyEnd(this);

            semaphore.Release();
        }

        public uint GetId() => id;
    }

    class PartyManager {
        private static PartyManager? _instance;
        private static readonly object _lock = new();
        private readonly ConcurrentQueue<Party> freePartyQueue = new();
        private readonly ConcurrentDictionary<uint, Party> fullPartyList = new();


        private uint partyCount = 0;
        private SemaphoreSlim partySemaphore;
        
        private uint maxParty;
        private uint dpsCount;
        private uint healerCount;
        private uint tankCount;
        private uint maxTime;
        private uint minTime;
        
        public Random random = new Random();
        public bool running = true;
        private PartyManager() { }

        public static PartyManager Instance {
            get {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new PartyManager();
                    }
                    return _instance;
                }
            }
        }

        public void NewParty() {
            uint id = partyCount++;
            Party party = new(id, minTime, maxTime);
            freePartyQueue.Enqueue(party);
        }

        public void NotifyPartyStart(Party party) {
            fullPartyList.TryAdd(party.GetId(), party);
        }

        public void NotifyPartyEnd(Party party) {
            if (fullPartyList.TryRemove(party.GetId(), out _)) {
                freePartyQueue.Enqueue(party);
            }
        }

        public void SetParameters(uint maxParty, uint dpsCount, uint healerCount, uint tankCount, uint minTime, uint maxTime) {
            this.maxParty = maxParty;
            this.dpsCount = dpsCount;
            this.healerCount = healerCount;
            this.tankCount = tankCount;
            this.minTime = minTime;
            this.maxTime = maxTime;

            if (this.minTime > this.maxTime) { this.minTime = this.maxTime; }

            this.partySemaphore = new SemaphoreSlim((int) maxParty);
        }
        public void Initialize() {
            for (uint i = 0; i < maxParty; i++) { // Pre-generate some parties
                NewParty();
            }

            Task.Run(async () => {
                while (true) {
                    await partySemaphore.WaitAsync(); // Ensures only 'maxParty' parties run concurrently

                    lock (_lock) {
                        if (dpsCount < 3 || healerCount < 1 || tankCount < 1) {
                            Console.WriteLine("Not enough members to start a new party. Exiting...");
                            running = false;
                            return;
                        }
                        if (freePartyQueue.TryDequeue(out Party? party)) {
                            dpsCount -= 3;
                            healerCount--;
                            tankCount--;
                            Task.Run(async () => await party.StartPartyAsync(partySemaphore));
                        }
                        else {
                            partySemaphore.Release(); // If no parties are available, release the lock
                        }
                    }
                }
            });
        }
    }
}
