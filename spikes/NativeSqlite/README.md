# Native SQLite compatibility spike

This narrow Phase 0 spike shares `Assets/Phase0Compatibility/NativeSqliteProbe.cs` between standalone hosts and the Unity diagnostic. It is not the application's persistence adapter and does not encrypt data, assess learners or simulate science.

```sh
python3 tools/run_native_sqlite_spike.py
python3 tools/run_native_sqlite_spike.py --windows
```

Run from the project root. Linux uses isolated SDK 10 and system `libsqlite3.so.0`; the Windows path uses the installed Windows SDK 8.0.427 and OS `winsqlite3`. The fallback SDK is deliberately scoped to the Windows diagnostic, not the production backend. WSL Windows execution requires appropriate permissions. Both hosts compile the same probe source with the appropriate native library selection. `Shared/Shared.csproj` separately checks the source against .NET Standard 2.1/C# 9.

The runner creates its own synthetic database in a Unicode-named directory under artifacts, runs a seed/constraint test, launches a child that exits with code 86 inside an open transaction, and verifies durable state in a third process. It then removes the database directory and writes a result JSON under `artifacts/native-sqlite`. Existing databases are never overwritten. This tests abrupt process exit, not hardware power loss or all filesystem failures.

Twelve assertions passed per host on 2026-11-01: Linux SQLite 3.45.1/.NET 10.0.13 and Windows SQLite 3.53.4/.NET 8.0.33. These version observations apply to this host only. School computers may have different native SQLite versions/API availability. No SQLite binary is redistributed. A maintained packaged provider remains an alternative if Unity/IL2CPP or supported Windows versions fail this probe.

Native interop intentionally uses no managed callbacks or runtime-generated code. Explicit UTF-8 buffers and transient bound parameters exercise AOT-relevant boundaries, but they do not prove IL2CPP compatibility. Windows Unity import/build/run and protection of local sensitive data remain open gates.

Primary references: [Microsoft Windows extension APIs](https://learn.microsoft.com/en-us/uwp/win32-and-com/win32-extension-apis), [SQLite opening/UTF-8 contract](https://sqlite.org/c3ref/open.html). This is an experiment with the OS API; it does not establish a production support policy.
