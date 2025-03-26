using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P2 {
    enum Status {
        ACTIVE,
        EMPTY
    }
    class GameInstance {
        private readonly uint id;
        private uint timeLimit;
        private readonly uint minTime;
        private readonly uint maxTime;
        private uint partiesRan;
        private ulong totalPartyTime;
        private Status status = Status.EMPTY;

        public GameInstance(uint id, uint minTime, uint maxTime) {
            this.id = id;
            this.minTime = minTime;
            this.maxTime = maxTime;
        }

        public async Task StartInstanceAsync(SemaphoreSlim semaphore) {
            status = Status.ACTIVE;
            PartyManager PM = PartyManager.Instance;
            timeLimit = (uint)PM.random.Next((int)minTime, (int)maxTime);
            partiesRan++;
            await Task.Delay((int)timeLimit * 1000);
            totalPartyTime += timeLimit;
            status = Status.EMPTY;
            PM.NotifyPartyEnd(this);

            semaphore.Release();
        }

        public uint GetId() => id;
        public Status GetStatus() => status;
        public uint GetPartiesRan() => partiesRan;
        public ulong GetTotalPartyTime() => totalPartyTime;
    }

    class PartyManager {
        private static PartyManager? _instance;
        private static readonly object _lock = new();
        private readonly List<GameInstance> partyList = new();
        private uint partyCount = 0;
        private SemaphoreSlim partySemaphore;
        
        private uint maxInstances;
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
                    if (_instance == null) { _instance = new PartyManager(); }
                    return _instance;
                }
            }
        }

        public void NewParty() {
            uint id = partyCount++;
            GameInstance party = new(id, minTime, maxTime);
            partyList.Add(party);
        }
        public void NotifyPartyEnd(GameInstance party) {
            Logger.Log("Party " + party.GetId() + " has ended.", partyList);
        }

        public void SetParameters(uint maxInstances, uint dpsCount, uint healerCount, uint tankCount, uint minTime, uint maxTime) {
            this.maxInstances = maxInstances;
            this.dpsCount = dpsCount;
            this.healerCount = healerCount;
            this.tankCount = tankCount;
            this.minTime = minTime;
            this.maxTime = maxTime;

            if (this.minTime > this.maxTime) { this.minTime = this.maxTime; }

            this.partySemaphore = new SemaphoreSlim((int) maxInstances);
        }
        public void Initialize() {
            for (uint i = 0; i < maxInstances; i++) { // Pre-generate some parties
                NewParty();
            }

            Task.Run(async () => {
                while (true) {
                    await partySemaphore.WaitAsync(); // Ensures only 'maxParty' parties run concurrently

                    lock (_lock) {
                        if (dpsCount < 3 || healerCount < 1 || tankCount < 1) {
                            break;
                        }
                        GameInstance? party = partyList.FirstOrDefault(p => p.GetStatus() == Status.EMPTY);
                        if (party != null) {
                            dpsCount -= 3;
                            healerCount--;
                            tankCount--;
                            Task runParty = party.StartInstanceAsync(partySemaphore);
                            Logger.Log("Party " + party.GetId() + " has started.", partyList);
                        }
                        else {
                            partySemaphore.Release(); // If no parties are available, release the lock
                        }
                    }
                }
                while (partyList.FirstOrDefault(p => p.GetStatus() == Status.ACTIVE) != null) { }
                Logger.LogEnd("All parties have ended.", partyList);
                running = false;
            });
        }
    }
}
