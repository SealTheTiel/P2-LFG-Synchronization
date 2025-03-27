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

        public async Task StartInstanceAsync() {
            status = Status.ACTIVE;
            PartyManager PM = PartyManager.Instance;
            PM.NotifyPartyStart(this);
            timeLimit = (uint)PM.random.Next((int)minTime, (int)maxTime + 1);
            partiesRan++;
            await Task.Delay((int)timeLimit * 1000);
            totalPartyTime += timeLimit;
            status = Status.EMPTY;
            PM.NotifyPartyEnd(this);
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
        private bool logToFile = false;
        private bool verboseLog = true;
        
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
       public void NotifyPartyStart(GameInstance party) {
            if (!verboseLog) { return; }
            Logger.Log("Party " + party.GetId() + " has started.", partyList, logToFile);
        }

        public void NotifyPartyEnd(GameInstance party) {
            if (!verboseLog) { return; }
            Logger.Log("Party " + party.GetId() + " has ended.", partyList, logToFile);
        }

        public void SetParameters(uint maxInstances, uint dpsCount, uint healerCount, uint tankCount, uint minTime, uint maxTime, uint logToFile, uint verboseLog) {
            this.maxInstances = maxInstances;
            this.dpsCount = dpsCount;
            this.healerCount = healerCount;
            this.tankCount = tankCount;
            this.minTime = minTime;
            this.maxTime = maxTime;

            if (logToFile == 1) { this.logToFile = true; }
            if (verboseLog == 0) { this.verboseLog = false; }
        }
        public void Initialize() {
            for (uint i = 0; i < maxInstances; i++) { // Pre-generate some parties
                NewParty();
            }
            int nextPartyId = 0;
            Task.Run(async () => {
                while (true) {
                    lock (_lock) {
                        if (dpsCount < 3 || healerCount < 1 || tankCount < 1) {
                            break;
                        }
                        int checkedCount = 0;
                        GameInstance? party = null;
                        for (int i = nextPartyId; i < maxInstances; i ++) {
                            GameInstance candidate = partyList[i];
                            if (candidate.GetStatus() == Status.EMPTY) {
                                party = candidate;
                                nextPartyId = i ;
                                break;
                            }
                        }
                        if (nextPartyId == maxInstances) {
                            nextPartyId = 0;
                        }
                        while (checkedCount < partyList.Count) {
                            GameInstance candidate = partyList[nextPartyId];
                            nextPartyId = (nextPartyId + 1) % partyList.Count;

                            if (candidate.GetStatus() == Status.EMPTY) {
                                party = candidate;
                                break;
                            }
                            checkedCount++;
                        }
                        if (party != null) {
                            dpsCount -= 3;
                            healerCount--;
                            tankCount--;
                            Task runParty = party.StartInstanceAsync();
                        }
                    }
                }
                while (partyList.FirstOrDefault(p => p.GetStatus() == Status.ACTIVE) != null) { }
                Logger.LogEnd("All parties have ended.", partyList, logToFile);
                running = false;
            });
        }
    }
}
