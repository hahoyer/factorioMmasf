﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI
{
    public sealed class MainContainer : Application
    {
        static readonly MainContainer Instance = new MainContainer();

        [STAThread]
        public static void Main() => Instance.Run();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var view = MmasfContext.Instance.CreateView();
            var main = new Window
            {
                Content = view,
                Title = "MmasfContext"
            };

            view.Selection.RegisterKeyBoardHandler(main);
            main.InstallPositionPersister();
            main.InstallMainMenu(CreateMainMenu());
            Tracer.FlaggedLine("XAML: \n" + XDocument.Parse(XamlWriter.Save(main)));
            main.Show();


            //new Task(() => SimulateSelections(view)).Start();
        }

        Menu CreateMainMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            new MenuItem
                            {
                                Header = "_New",
                                Command = this.Command(OnNew)
                            },
                            new MenuItem
                            {
                                Header = "_Select",
                                Command = this.Command(OnSelect)
                            },
                            new MenuItem
                            {
                                Header = "_Exit",
                                Command = this.Command(OnExit)
                            }
                        }
                    }
                }
            };

        void OnExit() => Shutdown();
    }
}