using System;
using System.IO;
using System.Text;
using CommonHouse.Content;
using CommonHouse.Domain;
using UnityEngine;

namespace CommonHouse.Persistence
{
    public sealed class CaseSaveLoadResult
    {
        public CaseState State { get; }
        public bool RecoveredFromBackup { get; }
        public string SourcePath { get; }

        internal CaseSaveLoadResult(CaseState state, bool recoveredFromBackup, string sourcePath)
        {
            State = state;
            RecoveredFromBackup = recoveredFromBackup;
            SourcePath = sourcePath;
        }
    }

    // The caller supplies a development root. Tests use a unique temporary root;
    // game integration chooses a local development root explicitly rather than a user save slot.
    public sealed class CaseSaveStore
    {
        private readonly CaseContent content;
        public string SavePath { get; }
        public string BackupPath => SavePath + ".bak";

        public CaseSaveStore(CaseContent content, string developmentRoot, string fileName = "case-01.json")
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(developmentRoot)) throw new ArgumentException("An explicit development save root is required.", nameof(developmentRoot));
            if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || Path.GetFileName(fileName) != fileName)
                throw new ArgumentException("A plain save file name is required.", nameof(fileName));
            SavePath = Path.Combine(Path.GetFullPath(developmentRoot), fileName);
        }

        public void Save(CaseState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (File.Exists(SavePath) && !TryLoad(SavePath, content, out _, out var error))
                throw new InvalidOperationException("Refusing to replace an invalid primary save. Recover or preserve " + SavePath + " first: " + error);
            var snapshot = state.CaptureSnapshot();
            if (snapshot.CaseId != content.Id)
                throw new InvalidOperationException("Refusing to write state for a different case content asset.");
            // Validate the captured state against this store's content before it can replace a good primary.
            CaseState.RestoreSnapshot(content, snapshot);
            var json = JsonUtility.ToJson(snapshot, true);
            var directory = Path.GetDirectoryName(SavePath);
            Directory.CreateDirectory(directory);
            var temporaryPath = SavePath + ".tmp";
            WriteDurable(temporaryPath, json);
            try
            {
                if (File.Exists(SavePath))
                    File.Replace(temporaryPath, SavePath, BackupPath);
                else
                    File.Move(temporaryPath, SavePath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public CaseSaveLoadResult Load()
        {
            if (TryLoad(SavePath, content, out var state, out var primaryError))
                return new CaseSaveLoadResult(state, false, SavePath);
            if (TryLoad(BackupPath, content, out state, out var backupError))
                return new CaseSaveLoadResult(state, true, BackupPath);
            throw new InvalidOperationException("Could not load Case 01 save. Primary: " + primaryError + " Backup: " + backupError + " Files were preserved for inspection.");
        }

        private static bool TryLoad(string path, CaseContent content, out CaseState state, out string error)
        {
            state = null;
            if (!File.Exists(path))
            {
                error = "missing " + path;
                return false;
            }
            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) throw new InvalidOperationException("empty file");
                var snapshot = JsonUtility.FromJson<CaseStateSnapshot>(json);
                if (snapshot == null) throw new InvalidOperationException("malformed JSON");
                state = CaseState.RestoreSnapshot(content, snapshot);
                error = null;
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException || exception is InvalidOperationException)
            {
                error = path + ": " + exception.Message;
                return false;
            }
        }

        private static void WriteDurable(string path, string contents)
        {
            var bytes = Encoding.UTF8.GetBytes(contents);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }
    }
}
