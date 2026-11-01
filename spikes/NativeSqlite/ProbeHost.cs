using System;
using System.IO;
using System.Text.Json;
using VirtualStem.Phase0;

internal static class ProbeHost
{
    private static int Main(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: seed|interrupt|verify databasePath [schemaPath]");
        if (args[0] == "seed")
        {
            var report = NativeSqliteProbe.RunFresh(args[1], File.ReadAllText(args[2]));
            Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true }));
        }
        else if (args[0] == "interrupt")
        {
            var activeWrite = NativeSqliteProbe.BeginInterruptedWrite(args[1]);
            Console.WriteLine("Intentionally exiting without disposing active transaction.");
            GC.KeepAlive(activeWrite);
            Environment.Exit(86);
        }
        else if (args[0] == "verify")
        {
            NativeSqliteProbe.VerifyInterruptedWrite(args[1]);
            Console.WriteLine("PASS abrupt-process-exit recovery");
        }
        else throw new ArgumentException("Unknown probe mode.");
        return 0;
    }
}
