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

        internal DataConfiguration(string fileName)
        {
            IniFile = new IniFile(fileName.PathCombine(ConfigurationIniFileName), ";");
            RootUserConfigurationPath = fileName.ToSmbFile().DirectoryName;
        }

        public string CurrentUserConfigurationPath
        {
            get => FactorioStyleCurrentUserConfigurationPath.PathFromFactorioStyle();
            set => FactorioStyleCurrentUserConfigurationPath = value;
        }

        string FactorioStyleCurrentUserConfigurationPath
        {
            get => IniFile[PathSectionName][WriteDataTag];
            set
            {
                IniFile[PathSectionName][WriteDataTag] = value;
                IniFile.Persist();
            }
        }

        public string RootUserConfigurationPath { get; }
    }
}