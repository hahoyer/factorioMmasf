using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    public sealed class ViewConfiguration : DumpableObject
    {
        internal interface IData
        {
            Window CreateView(ViewConfiguration viewConfiguration);
            string Name { get; }
        }

        internal readonly string Name;
        internal readonly IData Data;
        readonly ValueCache<Persister> PersisterCache;
        readonly ValueCache<Window> ViewCache;
        internal static readonly IData Saves = new SavesType();
        internal static readonly IData Mods = new ModsType();
        internal static readonly IData ModDictionary = new ModDictionaryType();

        sealed class SavesType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new SavesView(viewConfiguration);

            string IData.Name => "Saves";
        }

        sealed class ModsType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new ModsView(viewConfiguration);

            string IData.Name => "Mods";
        }

        sealed class ModDictionaryType : DumpableObject, IData
        {
            Window IData.CreateView(ViewConfiguration viewConfiguration)
                => new ModDictionaryView(viewConfiguration);

            string IData.Name => "ModDictionary";
        }

        internal ViewConfiguration(string name, IData data)
        {
            Name = name;
            Data = data;
            PersisterCache = new ValueCache<Persister>(() => new Persister(ItemFile("View")));
            ViewCache = new ValueCache<Window>(CreateAndConnectView);
        }

        internal string Status
        {
            get { return ItemFile("Status").String; }
            private set { ItemFile("Status").String = value; }
        }

        internal DateTime? LastUsed
        {
            get { return FromDateTime(ItemFile("LastUsed").String); }
            private set { ItemFile("LastUsed").String = value?.ToString("O"); }
        }

        static DateTime? FromDateTime(string value)
        {
            if(value == null)
                return null;

            DateTime result;
            if(DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        internal Window View => ViewCache.Value;

        Window CreateAndConnectView()
        {
            var result = CreateView();
            ConnectToWindow(result);
            return result;
        }

        Window CreateView() => Data.CreateView(this);

        internal void ConnectToWindow(Window window)
        {
            Status = "Open";
            window.Closing += (a, s) => OnClosing();
            window.Activated += (a, s) => OnActivated();
        }

        Persister ViewPersister => PersisterCache.Value;

        File ItemFile(string itemName) => ItemFileName(itemName).FileHandle();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(Name + "." + Data.Name)
                .PathCombine(itemName);

        void OnClosing()
        {
            Status = "Closed";
            ViewCache.IsValid = false;
        }

        void OnActivated() { LastUsed = DateTime.Now; }

        internal string PositionPath => ItemFileName("Position");

        internal void ShowAndActivate()
        {
            View.Show();
            View.Activate();
        }
    }
}