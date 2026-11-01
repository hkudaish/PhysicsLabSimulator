using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualStem.Phase0
{
    // Architecture spike only: not a general repository, encryption provider or scientific implementation.
    public static class NativeSqliteProbe
    {
#if STEM_WINDOWS || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string Library = "winsqlite3";
#else
        private const string Library = "libsqlite3.so.0";
#endif
        public const string ExpectedState = "{\"label\":\"محلول 'أزرق'\",\"volumeL\":0.2,\"moles\":0.1,\"wireIds\":[\"w-1\"]}";

        [Serializable]
        public sealed class Result
        {
            public string provider = Library;
            public string sqliteVersion = "";
            public string runtime = "";
            public string[] passed = Array.Empty<string>();
            public bool unityIl2cppVerified;
        }

        public static Result RunFresh(string path, string schema)
        {
            if (File.Exists(path)) throw new IOException("Probe refuses to overwrite an existing database.");
            var passed = new List<string>();
            string version;
            using (var db = new Connection(path))
            {
                version = db.Scalar("SELECT sqlite_version();");
                Require(db.Scalar("PRAGMA foreign_keys;") == "1", "foreign keys enabled", passed);
                Require(db.Scalar("PRAGMA journal_mode=WAL;") == "wal", "WAL available", passed);
                db.Exec(schema);
                db.Exec("INSERT INTO profile VALUES ('p1','tenant-a','learner-a'); INSERT INTO profile VALUES ('p2','tenant-b','learner-b');");
                db.Exec("INSERT INTO installed_content VALUES ('dilution','1','model-1',1,'hash','key');");
                db.Exec("INSERT INTO snapshot VALUES ('s1','p1','dilution','1','model-1',1,0,'{}','2026-11-01T00:00:00Z');");
                db.Exec("BEGIN IMMEDIATE;");
                try
                {
                    db.ExecuteBound("UPDATE snapshot SET revision=1,state_json=? WHERE id='s1';", ExpectedState);
                    db.ExecuteBound("INSERT INTO journal VALUES ('e1','p1','s1',1,?,'hash','2026-11-01T00:00:00Z');", ExpectedState);
                    db.Exec("INSERT INTO outbox(event_id) VALUES ('e1'); COMMIT;");
                }
                catch { db.Exec("ROLLBACK;"); throw; }
                Require(db.Scalar("SELECT state_json FROM snapshot WHERE id='s1';") == ExpectedState,
                    "bound Arabic and quoted JSON roundtrip", passed);
                Require(db.Scalar("SELECT count(*) FROM journal JOIN outbox USING(event_id);") == "1",
                    "snapshot journal and outbox committed", passed);
                ExpectConstraint(db, "INSERT INTO journal VALUES ('cross','p2','s1',2,'{}','hash','now');", "cross-profile event rejected", passed);
                ExpectConstraint(db, "INSERT INTO journal VALUES ('e1','p1','s1',2,'{}','hash','now');", "duplicate event rejected", passed);
                ExpectConstraint(db, "UPDATE outbox SET status='acknowledged' WHERE event_id='e1';", "ack requires receipt", passed);
                db.Exec("BEGIN IMMEDIATE;");
                try
                {
                    db.Exec("UPDATE snapshot SET revision=2,state_json='uncommitted' WHERE id='s1';");
                    db.Exec("INSERT INTO journal VALUES ('rollback','p1','s1',2,'{}','hash','now');");
                    db.Exec("INSERT INTO outbox(event_id) VALUES ('missing-event');");
                    throw new InvalidOperationException("Expected foreign-key failure did not occur.");
                }
                catch (SqliteFailure error) when (error.Code == 19)
                {
                    db.Exec("ROLLBACK;");
                }
                Require(db.Scalar("SELECT revision FROM snapshot WHERE id='s1';") == "1" &&
                    db.Scalar("SELECT count(*) FROM journal WHERE event_id='rollback';") == "0",
                    "failed transaction rolls back all writes", passed);
            }
            using (var reopened = new Connection(path))
            {
                Require(reopened.Scalar("SELECT state_json FROM snapshot WHERE id='s1';") == ExpectedState,
                    "close reopen preserves full JSON", passed);
                Require(reopened.Scalar("PRAGMA integrity_check;") == "ok", "database integrity", passed);
            }
            return new Result { sqliteVersion = version, runtime = RuntimeInformation.FrameworkDescription,
                passed = passed.ToArray(), unityIl2cppVerified = false };
        }

        // Host intentionally exits without disposal after this returns; used only on its own synthetic DB.
        public static IDisposable BeginInterruptedWrite(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Seed probe database first.", path);
            var db = new Connection(path);
            try
            {
                db.Exec("BEGIN IMMEDIATE; UPDATE snapshot SET revision=99,state_json='interrupted' WHERE id='s1';");
                db.Exec("INSERT INTO journal VALUES ('interrupted','p1','s1',99,'{}','hash','now');");
                return db;
            }
            catch { db.Dispose(); throw; }
        }

        public static void VerifyInterruptedWrite(string path)
        {
            using (var db = new Connection(path))
            {
                if (db.Scalar("SELECT revision FROM snapshot WHERE id='s1';") != "1" ||
                    db.Scalar("SELECT state_json FROM snapshot WHERE id='s1';") != ExpectedState ||
                    db.Scalar("SELECT count(*) FROM journal WHERE event_id='interrupted';") != "0" ||
                    db.Scalar("PRAGMA integrity_check;") != "ok")
                    throw new InvalidOperationException("Interrupted-write recovery failed.");
            }
        }

        private static void Require(bool condition, string name, List<string> passed)
        {
            if (!condition) throw new InvalidOperationException("Probe failed: " + name);
            passed.Add(name);
        }

        private static void ExpectConstraint(Connection db, string sql, string name, List<string> passed)
        {
            try { db.Exec(sql); }
            catch (SqliteFailure error) when (error.Code == 19) { passed.Add(name); return; }
            throw new InvalidOperationException("Expected constraint failure: " + name);
        }

        private sealed class SqliteFailure : Exception
        {
            public int Code { get; }
            public SqliteFailure(int code, string message) : base(message) { Code = code; }
        }

        private sealed class Connection : IDisposable
        {
            private IntPtr handle;
            public Connection(string path)
            {
                try
                {
                    Check(sqlite3_open_v2(Utf8(path), out handle, 2 | 4 | 0x10000, IntPtr.Zero));
                    Exec("PRAGMA foreign_keys=ON; PRAGMA busy_timeout=2000; PRAGMA synchronous=FULL;");
                }
                catch { if (handle != IntPtr.Zero) { sqlite3_close(handle); handle = IntPtr.Zero; } throw; }
            }
            public void Exec(string sql)
            {
                IntPtr error;
                int code = sqlite3_exec(handle, Utf8(sql), IntPtr.Zero, IntPtr.Zero, out error);
                try { Check(code); }
                finally { if (error != IntPtr.Zero) sqlite3_free(error); }
            }
            public void ExecuteBound(string sql, string value)
            {
                IntPtr statement;
                Check(sqlite3_prepare_v2(handle, Utf8(sql), -1, out statement, IntPtr.Zero));
                try
                {
                    byte[] bytes = Utf8(value);
                    Check(sqlite3_bind_text(statement, 1, bytes, bytes.Length - 1, new IntPtr(-1)));
                    int code = sqlite3_step(statement);
                    if (code != 101) Check(code);
                }
                finally { Check(sqlite3_finalize(statement)); }
            }
            public string Scalar(string sql)
            {
                IntPtr statement;
                Check(sqlite3_prepare_v2(handle, Utf8(sql), -1, out statement, IntPtr.Zero));
                try
                {
                    int code = sqlite3_step(statement);
                    if (code != 100) { Check(code); throw new InvalidOperationException("Expected scalar row."); }
                    IntPtr pointer = sqlite3_column_text(statement, 0);
                    int length = sqlite3_column_bytes(statement, 0);
                    var bytes = new byte[length];
                    if (length > 0) Marshal.Copy(pointer, bytes, 0, length);
                    return Encoding.UTF8.GetString(bytes);
                }
                finally { Check(sqlite3_finalize(statement)); }
            }
            private void Check(int code)
            {
                if (code != 0) throw new SqliteFailure(code & 0xff, "SQLite native error " + code);
            }
            public void Dispose()
            {
                if (handle == IntPtr.Zero) return;
                Check(sqlite3_close(handle));
                handle = IntPtr.Zero;
            }
        }

        private static byte[] Utf8(string text)
        {
            if (text.IndexOf('\0') >= 0) throw new ArgumentException("NUL input is not supported.");
            return Encoding.UTF8.GetBytes(text + "\0");
        }
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_open_v2(byte[] filename, out IntPtr database, int flags, IntPtr vfs);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_close(IntPtr database);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_exec(IntPtr database, byte[] sql, IntPtr callback, IntPtr context, out IntPtr error);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern void sqlite3_free(IntPtr pointer);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_prepare_v2(IntPtr database, byte[] sql, int bytes, out IntPtr statement, IntPtr tail);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_step(IntPtr statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_finalize(IntPtr statement);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_bind_text(IntPtr statement, int index, byte[] text, int bytes, IntPtr destructor);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr sqlite3_column_text(IntPtr statement, int column);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int sqlite3_column_bytes(IntPtr statement, int column);
    }
}
