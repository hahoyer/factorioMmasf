﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using log4net;

namespace ManageModsAndSavefiles
{
    sealed class UserConfiguration : DumpableObject
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const string SaveDirectoryName = "saves";
        internal const string ModDirectoryName = "mods";
        const string PlayerDataFileName = "player-data.json";
        const string ReadDataTag = "read-data";

        internal static string[] Paths
            => Extension
                .SystemWriteDataDir
                .FileHandle()
                .RecursiveItems()
                .Where(IsRelevantPathCandidate)
                .Select(item => item.FullName)
                .ToArray();

        static bool IsRelevantPathCandidate(File item)
            =>
            item.IsDirectory
            && IsExistent(item, PlayerDataFileName, false)
            && IsExistent(item, SaveDirectoryName, true)
            && IsExistent(item, ModDirectoryName, true);

        static bool IsExistent(File item, string fileName, bool isDictionary)
        {
            var fileHandle = item.FullName.PathCombine(fileName).FileHandle();
            return fileHandle.Exists && fileHandle.IsDirectory == isDictionary;
        }

        internal static UserConfiguration Create(string item) => new UserConfiguration(item);

        readonly string Path;
        readonly ValueCache<SaveFile[]> SaveFilesCache;
        readonly ValueCache<ModFile[]> ModFilesCache;

        UserConfiguration(string path)
        {
            Path = path;
            ModFilesCache = new ValueCache<ModFile[]>(GetModFiles);
            SaveFilesCache = new ValueCache<SaveFile[]>(GetSaveFiles);
        }

        string FilesPath(string item) => Path.PathCombine(item);

        SaveFile[] GetSaveFiles()
        {
            var fileHandle = FilesPath(SaveDirectoryName).FileHandle();
            if(!fileHandle.Exists)
                return new SaveFile[0];

            return fileHandle
                .Items
                .Where(item => !item.IsDirectory && item.Extension.ToLower() == ".zip")
                .Select(item => new SaveFile(item.FullName))
                .ToArray();
        }

        ModFile[] GetModFiles()
        {
            var fileHandle = FilesPath(ModDirectoryName).FileHandle();
            if(!fileHandle.Exists)
                return new ModFile[0];

            return fileHandle
                .Items
                .Where(item => item.IsDirectory || item.Extension.ToLower() == ".zip")
                .Select(item => ModFile.Create(item.FullName, null))
                .ToArray();
        }

        IEnumerable<ModFile> ModFiles => ModFilesCache.Value;
        IEnumerable<SaveFile> SaveFiles => SaveFilesCache.Value;

        public void InitializeFrom(UserConfiguration source)
        {
            Log.Debug("InitializeFrom");
            source.FilesPath(PlayerDataFileName).FileHandle().CopyTo(FilesPath(PlayerDataFileName));
            Synchronize(SaveFiles, source.SaveFiles, SaveDirectoryName, source);
            Synchronize(ModFiles, source.ModFiles, ModDirectoryName, source);
        }

        void Synchronize<T>
        (
            IEnumerable<T> currentFiles,
            IEnumerable<T> masterFiles,
            string itemName,
            UserConfiguration master
        )
            where T : class, INameProvider
        {
            var pathOfCurrent = FilesPath(itemName);
            var pathOfMaster = master.FilesPath(itemName);
            var merge = currentFiles
                .Merge(masterFiles, item => item.Name)
                .ToArray();

            var fileNamesToGet = merge
                .Where(item => item.Item2 == null && item.Item3 != null)
                .Select(item => item.Item1)
                .ToArray();

            pathOfCurrent.FileHandle().EnsureIsExistentDirectory();

            foreach(var fileName in fileNamesToGet)
            {
                var sourceFileName = pathOfMaster.PathCombine(fileName);
                var destFileName = pathOfCurrent.PathCombine(fileName);

                Tracer.LinePart("Copying " + sourceFileName + " to " + destFileName + " ... ");
                sourceFileName.FileHandle().CopyTo(destFileName);
                Tracer.Line("complete");
            }

            var fileNamesToPut = merge
                .Where(item => item.Item2 != null && item.Item3 == null)
                .Select(item => item.Item1)
                .ToArray();

            foreach(var fileName in fileNamesToPut)
            {
                var sourceFileName = pathOfCurrent.PathCombine(fileName);
                var destFileName = pathOfMaster.PathCombine(fileName);

                Tracer.LinePart("Copying " + sourceFileName + " to " + destFileName + " ... ");
                sourceFileName.FileHandle().CopyTo(destFileName);
                Tracer.Line("complete");
            }
        }

        internal interface INameProvider
        {
            string Name { get; }
        }
    }
}