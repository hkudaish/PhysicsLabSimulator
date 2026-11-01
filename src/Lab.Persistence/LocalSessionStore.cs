using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualStem.Persistence
{
    // Anonymous prototype drafts only. No credentials, trusted grades or student identities.
    public sealed class LocalSessionStore
    {
        private readonly string path;
        public LocalSessionStore(string path) { this.path = Path.GetFullPath(path); }
        public void Save(string sessionId, string modelVersion, string json)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(modelVersion) ||
                string.IsNullOrEmpty(json) || Encoding.UTF8.GetByteCount(json) > 4 * 1024 * 1024)
                throw new ArgumentException("Invalid or oversized local draft.");
            using (var db = new Database(path))
            {
                db.Exec("BEGIN IMMEDIATE;");
                try
                {
                    db.Execute("INSERT INTO lab_session(id,model_version,state_json,saved_at) VALUES(?1,?2,?3,?4) " +
                        "ON CONFLICT(id) DO UPDATE SET model_version=excluded.model_version,state_json=excluded.state_json,saved_at=excluded.saved_at;",
                        sessionId, modelVersion, json, DateTime.UtcNow.ToString("O"));
                    db.Exec("COMMIT;");
                }
                catch { db.Exec("ROLLBACK;"); throw; }
            }
        }
        public string? Load(string sessionId, string expectedModelVersion)
        {
            using (var db = new Database(path))
            {
                string? version = db.Scalar("SELECT model_version FROM lab_session WHERE id=?1;", sessionId);
                if (version == null) return null;
                if (version != expectedModelVersion) throw new InvalidDataException("Saved model version is not supported; original draft preserved.");
                string? data = db.Scalar("SELECT state_json FROM lab_session WHERE id=?1;", sessionId);
                if (data != null && Encoding.UTF8.GetByteCount(data) > 4 * 1024 * 1024) throw new InvalidDataException("Saved draft exceeds the supported size.");
                return data;
            }
        }

        private sealed class Database : IDisposable
        {
            private IntPtr handle;
            public Database(string path)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    Check(Api.Open(Utf8(path), out handle));
                    Exec("PRAGMA busy_timeout=2500; PRAGMA foreign_keys=ON; PRAGMA synchronous=FULL;");
                    string? version = Scalar("PRAGMA user_version;");
                    if (version != "0" && version != "1") throw new InvalidDataException("Newer local schema; database preserved.");
                    Exec("PRAGMA journal_mode=WAL;");
                    Exec("CREATE TABLE IF NOT EXISTS lab_session(id TEXT PRIMARY KEY,model_version TEXT NOT NULL,state_json TEXT NOT NULL,saved_at TEXT NOT NULL);");
                    if (version == "0") Exec("PRAGMA user_version=1;");
                }
                catch { Dispose(); throw; }
            }
            public void Exec(string sql)
            {
                IntPtr error; int code = Api.Exec(handle, Utf8(sql), out error);
                try { Check(code); } finally { if (error != IntPtr.Zero) Api.Free(error); }
            }
            private IntPtr Prepare(string sql, string[] values)
            {
                Check(Api.Prepare(handle, Utf8(sql), out IntPtr statement));
                try
                {
                    for (int i = 0; i < values.Length; i++)
                    { byte[] bytes = Utf8(values[i]); Check(Api.Bind(statement, i + 1, bytes, bytes.Length - 1)); }
                    return statement;
                }
                catch { Api.Finalize(statement); throw; }
            }
            public void Execute(string sql, params string[] values)
            {
                IntPtr statement = Prepare(sql, values);
                try { int code = Api.Step(statement); if (code != 101) Check(code); }
                finally { Api.Finalize(statement); }
            }
            public string? Scalar(string sql, params string[] values)
            {
                IntPtr statement = Prepare(sql, values);
                try
                {
                    int code = Api.Step(statement);
                    if (code == 101) return null;
                    if (code != 100) Check(code);
                    int length = Api.Bytes(statement); var bytes = new byte[length];
                    if (length > 0) Marshal.Copy(Api.Text(statement), bytes, 0, length);
                    return Encoding.UTF8.GetString(bytes);
                }
                finally { Api.Finalize(statement); }
            }
            private static void Check(int code) { if (code != 0) throw new IOException("SQLite error " + code + "; local data was not discarded."); }
            public void Dispose() { if (handle != IntPtr.Zero) { Api.Close(handle); handle = IntPtr.Zero; } }
        }
        private static byte[] Utf8(string value)
        {
            if (value.IndexOf('\0') >= 0) throw new ArgumentException("NUL is not permitted.");
            return Encoding.UTF8.GetBytes(value + "\0");
        }
        private static class Api
        {
            private static readonly bool Windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            public static int Open(byte[] path, out IntPtr db) => Windows ? Win.Open(path, out db, 2 | 4 | 0x10000, IntPtr.Zero) : Linux.Open(path, out db, 2 | 4 | 0x10000, IntPtr.Zero);
            public static int Close(IntPtr db) => Windows ? Win.Close(db) : Linux.Close(db);
            public static int Exec(IntPtr db, byte[] sql, out IntPtr error) => Windows ? Win.Exec(db, sql, IntPtr.Zero, IntPtr.Zero, out error) : Linux.Exec(db, sql, IntPtr.Zero, IntPtr.Zero, out error);
            public static void Free(IntPtr pointer) { if (Windows) Win.Free(pointer); else Linux.Free(pointer); }
            public static int Prepare(IntPtr db, byte[] sql, out IntPtr statement) => Windows ? Win.Prepare(db, sql, -1, out statement, IntPtr.Zero) : Linux.Prepare(db, sql, -1, out statement, IntPtr.Zero);
            public static int Bind(IntPtr statement, int index, byte[] text, int length) => Windows ? Win.Bind(statement, index, text, length, new IntPtr(-1)) : Linux.Bind(statement, index, text, length, new IntPtr(-1));
            public static int Step(IntPtr statement) => Windows ? Win.Step(statement) : Linux.Step(statement);
            public static int Finalize(IntPtr statement) => Windows ? Win.Finalize(statement) : Linux.Finalize(statement);
            public static IntPtr Text(IntPtr statement) => Windows ? Win.Text(statement, 0) : Linux.Text(statement, 0);
            public static int Bytes(IntPtr statement) => Windows ? Win.Bytes(statement, 0) : Linux.Bytes(statement, 0);
        }
        private static class Win
        {
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_open_v2", CallingConvention=CallingConvention.Cdecl)] internal static extern int Open(byte[] path,out IntPtr db,int flags,IntPtr vfs);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_close", CallingConvention=CallingConvention.Cdecl)] internal static extern int Close(IntPtr db);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_exec", CallingConvention=CallingConvention.Cdecl)] internal static extern int Exec(IntPtr db,byte[] sql,IntPtr cb,IntPtr ctx,out IntPtr error);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_free", CallingConvention=CallingConvention.Cdecl)] internal static extern void Free(IntPtr ptr);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_prepare_v2", CallingConvention=CallingConvention.Cdecl)] internal static extern int Prepare(IntPtr db,byte[] sql,int length,out IntPtr stmt,IntPtr tail);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_bind_text", CallingConvention=CallingConvention.Cdecl)] internal static extern int Bind(IntPtr stmt,int index,byte[] text,int length,IntPtr destructor);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_step", CallingConvention=CallingConvention.Cdecl)] internal static extern int Step(IntPtr stmt);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_finalize", CallingConvention=CallingConvention.Cdecl)] internal static extern int Finalize(IntPtr stmt);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_column_text", CallingConvention=CallingConvention.Cdecl)] internal static extern IntPtr Text(IntPtr stmt,int column);
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)] [DllImport("winsqlite3", EntryPoint="sqlite3_column_bytes", CallingConvention=CallingConvention.Cdecl)] internal static extern int Bytes(IntPtr stmt,int column);
        }
        private static class Linux
        {
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_open_v2", CallingConvention=CallingConvention.Cdecl)] internal static extern int Open(byte[] path,out IntPtr db,int flags,IntPtr vfs);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_close", CallingConvention=CallingConvention.Cdecl)] internal static extern int Close(IntPtr db);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_exec", CallingConvention=CallingConvention.Cdecl)] internal static extern int Exec(IntPtr db,byte[] sql,IntPtr cb,IntPtr ctx,out IntPtr error);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_free", CallingConvention=CallingConvention.Cdecl)] internal static extern void Free(IntPtr ptr);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_prepare_v2", CallingConvention=CallingConvention.Cdecl)] internal static extern int Prepare(IntPtr db,byte[] sql,int length,out IntPtr stmt,IntPtr tail);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_bind_text", CallingConvention=CallingConvention.Cdecl)] internal static extern int Bind(IntPtr stmt,int index,byte[] text,int length,IntPtr destructor);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_step", CallingConvention=CallingConvention.Cdecl)] internal static extern int Step(IntPtr stmt);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_finalize", CallingConvention=CallingConvention.Cdecl)] internal static extern int Finalize(IntPtr stmt);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_column_text", CallingConvention=CallingConvention.Cdecl)] internal static extern IntPtr Text(IntPtr stmt,int column);
            [DllImport("libsqlite3.so.0", EntryPoint="sqlite3_column_bytes", CallingConvention=CallingConvention.Cdecl)] internal static extern int Bytes(IntPtr stmt,int column);
        }
    }
}
