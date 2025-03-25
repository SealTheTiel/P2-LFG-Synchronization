using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public bool addCharacter(string type) {
            if (dps + healer + tank == 5) {
                return false;
            }
            if (type == "Damage" && dps < 3) {
                dps++;
            }
            else if (type == "Healer" && healer < 1) {
                healer++;
            }
            else if (type == "Tank" && tank < 1) {
                tank++;
            }
            return true;
        }

        public uint GetId() => id;
    }

    class PartyManager {
        //singleton that houses all parties
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
        }
    }
}
