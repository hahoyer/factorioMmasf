﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace ManageModsAndSavefiles
{
    public sealed class DataConfiguration
    {
        const string PathSectionName = "path";
        const string WriteDataTag = "write-data";
        const string ConfigurationIniFileName = "config.ini";

        readonly IniFile IniFile;
        readonly ValueCache<string> PathCache;

        internal DataConfiguration(string fileName)
        {
            IniFile = new IniFile(fileName.PathCombine(ConfigurationIniFileName));
            PathCache = new ValueCache<string>(GetPath);
        }

        string GetPath() => IniFile[PathSectionName][WriteDataTag]
            .PathFromFactorioStyle();

        internal string Path => PathCache.Value;
    }
}