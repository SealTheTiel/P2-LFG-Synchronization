// See https://aka.ms/new-console-template for more information
using P2;

Console.WriteLine("Hello, World!");
PartyManager pm = PartyManager.Instance;

pm.SetParameters(1, 30, 10, 10, 1, 1);
pm.Initialize();
while (pm.running) { }