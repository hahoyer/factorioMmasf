﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ManageModsAndSavefiles.Mods;
using MmasfUI.Common;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        [STAThread]
        public static void Main() => Instance.Run();

        internal static class Command
        {
            internal const string ViewModDictionary = "ViewModDictionary";
        }

        internal static readonly MainContainer Instance = new MainContainer();

        static Menu CreateMainMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            "_Select".MenuItem(UserConfigurationTile.Command.Select),
                            "_Exit".MenuItem("Exit")
                        }
                    },
                    new MenuItem
                    {
                        Header = "_View",
                        Items =
                        {
                            "_Saves".MenuItem(UserConfigurationTile.Command.ViewSaves),
                            "_Mods".MenuItem(UserConfigurationTile.Command.ViewMods),
                            "Mod _dictionary".MenuItem(Command.ViewModDictionary)
                        }
                    }
                }
            };

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShowContextView();
            SystemConfiguration.OpenActiveViews();
        }

        void ShowContextView()
        {
            var view = new ContextView();
            var main = new Window
            {
                Content = view,
                Title = "MmasfContext"
            };

            view.Selection.RegisterKeyboardHandler(main);
            main.InstallPositionPersister("Main");
            main.InstallMainMenu(CreateMainMenu());
            CommandManager.Activate(this);
            main.Show();
        }

        ViewConfiguration ModDescriptions => FindViewConfiguration(new[] {"ModDescriptions"});

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary()
        {
            ((ModDictionaryView) ModDescriptions.View).RefreshData();
            ModDescriptions.ShowAndActivate();
        }

        [Command(Command.ViewModDictionary)]
        public void ViewModDictionary(ModDescription currentItem)
        {
            ((ModDictionaryView) ModDescriptions.View).Select(currentItem);
            ViewModDictionary();
        }

        [Command("Exit")]
        public void OnExit() => Shutdown();

        internal readonly CommandManager CommandManager = new CommandManager
            (typeof(MainContainer).Namespace);

        internal void RemoveViewConfiguration(ViewConfiguration viewConfiguration)
        {
            ViewConfigurations = ViewConfigurations.Where(item => item != viewConfiguration).ToArray();
        }

        internal void AddViewConfiguration(ViewConfiguration viewConfiguration)
        {
            if(ViewConfigurations.All(item => item != viewConfiguration))
                ViewConfigurations = ViewConfigurations.Concat(new[] {viewConfiguration}).ToArray();
        }

        ViewConfiguration[] ViewConfigurations;

        internal ViewConfiguration FindViewConfiguration(string[] identifier)
            => ViewConfigurations.FirstOrDefault(item => item.Identifier.SequenceEqual(identifier))
               ?? new ViewConfiguration(identifier);
    }
}