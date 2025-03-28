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
            await PM.NotifyPartyStart(this.id);
            timeLimit = (uint)PM.random.Next((int)minTime, (int)maxTime + 1);
            partiesRan++;
            await Task.Delay((int)timeLimit * 1000);
            totalPartyTime += timeLimit;
            status = Status.EMPTY;
            await PM.NotifyPartyEnd(this.id);
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
        private static readonly SemaphoreSlim dictionarySemaphore = new(1, 1);
        private readonly ConcurrentDictionary<uint, Status> partyStatusList = new();

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
            partyStatusList[id] = Status.EMPTY;
        }
       public async Task NotifyPartyStart(uint id) {
            if (!verboseLog) { return; }
            await dictionarySemaphore.WaitAsync();
            try {
                partyStatusList[id] = Status.ACTIVE;
                Logger.EnqueueLog(new Log($"Party {id} has started.", new ConcurrentDictionary<uint, Status>(partyStatusList), logToFile));
            }
            finally {
                dictionarySemaphore.Release();
            }
        }

        public async Task NotifyPartyEnd(uint id) {
            if (!verboseLog) { return; }
            await dictionarySemaphore.WaitAsync();
            try {
                partyStatusList[id] = Status.EMPTY;
                Logger.EnqueueLog(new Log($"Party {id} has ended.", new ConcurrentDictionary<uint, Status>(partyStatusList), logToFile));
            }
            finally {
                dictionarySemaphore.Release();
            }
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

        private async void Simulate() {
            uint nextPartyId = 0;
            await Logger.LogStart(logToFile);
            while (true) {
                await dictionarySemaphore.WaitAsync();
                try {
                    if (dpsCount < 3 || healerCount < 1 || tankCount < 1) {
                        break;
                    }
                    GameInstance? party = null;

                    for (uint i = 0; i < maxInstances; i++) { 
                        uint index = (nextPartyId + i) % maxInstances;
                        if (partyStatusList[index] == Status.EMPTY) {
                            party = partyList[(int)index];
                            nextPartyId = (index + 1) % maxInstances;
                            break;
                        }
                    }
                    if (nextPartyId == maxInstances) {
                        nextPartyId = 0;
                    }
                    if (party != null) {
                        dpsCount -= 3;
                        healerCount--;
                        tankCount--;
                        Task runParty = party.StartInstanceAsync();
                    }
                }
                finally { dictionarySemaphore.Release(); }
                await Task.Delay(1);
            }
            while (partyList.FirstOrDefault(p => p.GetStatus() == Status.ACTIVE) != null) { }
            await dictionarySemaphore.WaitAsync();
            try {
                await Logger.LogEnd("All parties have ended.", partyList, logToFile);
            }
            finally {
                dictionarySemaphore.Release();
            }
            running = false;
        }

        public void Initialize() {
            for (uint i = 0; i < maxInstances; i++) { // Pre-generate some parties
                NewParty();
            }
            Task.Run(Simulate);

        }
    }
}
