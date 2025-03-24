// See https://aka.ms/new-console-template for more information
using P2;

Console.WriteLine("Hello, World!");
PartyManager pm = PartyManager.Instance;
for (ulong i = 0; i < 5000; i++) {
    pm.NewParty();
}

Console.WriteLine(pm.PartyCount() + " Parties");