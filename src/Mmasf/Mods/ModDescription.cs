﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using ManageModsAndSaveFiles.Reader;
using ManageModsAndSaveFiles.Saves;

namespace ManageModsAndSaveFiles.Mods
{
    public sealed class ModDescription : DumpableObject
    {
        public sealed class ModsReader : DumpableObject, BinaryRead.IReaderProvider
        {
            object BinaryRead.IReaderProvider.ReadAndAdvance
                (BinaryRead reader, Type type, MemberInfo member)
            {
                // ReSharper disable once UnusedVariable
                var x = reader.LookAhead();
                var isBefore01414 = ((UserContext) reader.UserContext).IsBefore01414;

                var result = isBefore01414
                    ? new
                    {
                        name = reader.GetNextString<int>(),
                        version = new Version
                        (
                            reader.GetNext<short>(),
                            reader.GetNext<short>(),
                            reader.GetNext<short>()
                        )
                    }
                    : new
                    {
                        name = reader.GetNextString<byte>(),
                        version = new Version
                        (
                            reader.GetNext<byte>(),
                            reader.GetNext<byte>(),
                            reader.GetNext<byte>()
                        )
                    };

                if(result.name == "")
                    throw new Exception("Mod not recognized in stream");

                var mod = MmasfContext.Instance.ModDictionary[result.name];
                return mod[result.version];
            }
        }

        static ModConfiguration ModConfiguration => MmasfContext.Instance.ModConfiguration;

        public readonly string Name;
        public readonly Version Version;
        public string FullName => Name + " " + Version;

        public bool HasConfiguration
        {
            get { return Configuration != null; }
            set
            {
                if(HasConfiguration == value)
                    return;

                if(value)
                    ModConfiguration.Add(Name);
                else
                    ModConfiguration.Remove(Name);
            }
        }

        public ModConfiguration.Item Configuration
            => ModConfiguration.Data.SingleOrDefault(data => data.Name == Name);

        InfoJSon InfoJSonValue;

        public ModDescription(string name, Version version)
        {
            Name = name;
            Version = version;
            Tracer.Assert(!string.IsNullOrEmpty(Name));
        }

        public InfoJSon InfoJSon
        {
            get { return InfoJSonValue; }
            set
            {
                if(InfoJSonValue == null)
                {
                    InfoJSonValue = value;
                    return;
                }

                if(InfoJSonValue == value)
                    return;

                NotImplementedMethod(value);
            }
        }

        public bool? IsGameOnlyPossible
        {
            get { return Configuration?.IsGameOnlyPossible; }
            set
            {
                if(IsGameOnlyPossible == value)
                    return;

                HasConfiguration = true;
                Configuration.IsGameOnlyPossible = value;

                if(Configuration.IsEmpty)
                    HasConfiguration = false;
            }
        }

        public bool? IsSaveOnlyPossible
        {
            get { return Configuration?.IsSaveOnlyPossible; }
            set
            {
                if(IsSaveOnlyPossible == value)
                    return;

                HasConfiguration = true;
                Configuration.IsSaveOnlyPossible = value;

                if(Configuration.IsEmpty)
                    HasConfiguration = false;
            }
        }

        public bool IsCompatible(Version saveModVersion) => true;
    }
}