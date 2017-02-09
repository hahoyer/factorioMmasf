using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class UserConfigurationWindow : Window
    {
        readonly UserConfiguration Configuration;
        bool IsSaves;
        readonly FileClusterProxy[] Data;
        readonly StatusBar StatusBar = new StatusBar();

        public UserConfigurationWindow(ViewConfiguration viewConfiguration)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations.Single(u => u.Name == viewConfiguration.Name);

            IsSaves = viewConfiguration.Type == ViewConfiguration.SavesType;
            Configuration = configuration;

            Data = Configuration
                .SaveFiles
                .Select(s => new FileClusterProxy(s))
                .ToArray();

            Content = CreateGrid();

            Title = viewConfiguration.Type + " of " + configuration.Name.Quote();
            this.InstallPositionPersister(viewConfiguration.PositionPath);
            this.InstallMainMenu(CreateMenu());
            this.InstallStatusLine(StatusBar);
        }

        ScrollViewer CreateGrid()
        {
            var result = new DataGrid
            {
                IsReadOnly = true
            };
            result.AutoGeneratingColumn += (s, e) => OnAutoGeneratingColumns(e);
            result.AutoGeneratedColumns += (s, e) => OnAutoGeneratedColumns((DataGrid)s);

            result.ItemsSource = Data;

            Task.Factory.StartNew
            (
                () =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                    "waiting".WriteFlaggedLine();
                    100.MilliSeconds().Sleep();

                    Tracer.FlaggedLine("Refreshing");
                    RefreshData();
                    Tracer.FlaggedLine("Refreshed");
                }
            );

            return new ScrollViewer
            {
                Content = result,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        static void OnAutoGeneratedColumns(DataGrid dataGrid)
        {
        }

        static void OnAutoGeneratingColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            var column = (DataGridTextColumn)args.Column;
            var binding = ((Binding) column.Binding);

            if (args.PropertyType == typeof(TimeSpanProxy))
            {
                binding.Path.Path += ".DisplayValue";

                column.SortMemberPath += ".Value";
                column.CanUserSort = true;
            }

            if(args.PropertyType == typeof(DateTime))
            {
                binding.StringFormat = "u";
                column.CanUserSort = true;
            }

            if (args.PropertyName == "Created")
            {
                column.SortDirection = ListSortDirection.Descending;
                column.CanUserSort = true;
            }
        }

        void RefreshData()
        {
            Tracer.FlaggedLine("loop");
            var count = Data.Length;
            var current = 0;
            Parallel.ForEach
            (
                Data,
                proxy =>
                {
                    proxy.Refresh();
                    current++;
                    StatusBar.Text = current + " of " + count;
                });

            StatusBar.Text = count.ToString();
        }

        public sealed class TimeSpanProxy : DumpableObject, INotifyPropertyChanged
        {
            [UsedImplicitly]
            public TimeSpan Value { get; }

            public TimeSpanProxy(TimeSpan value)
            {
                Value = value;
                OnPropertyChanged();
            }

            [UsedImplicitly]
            public string DisplayValue => Value.Format3Digits();

            public override string ToString() => Value.Format3Digits();

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged()
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        sealed class FileClusterProxy : INotifyPropertyChanged
        {
            readonly FileCluster Data;
            FileCluster DataIfRead => Data.IsDataRead ? Data : null;

            [UsedImplicitly]
            public DateTime Created => Data.Created;
            [UsedImplicitly]
            public string Name => Data.Name;
            [UsedImplicitly]
            public TimeSpanProxy Duration { get; set; }
            [UsedImplicitly]
            public Version Version => DataIfRead?.Version;
            [UsedImplicitly]
            public string ScenarioName => DataIfRead?.ScenarioName;
            [UsedImplicitly]
            public string MapName => DataIfRead?.MapName;
            [UsedImplicitly]
            public string CampaignName => DataIfRead?.CampaignName;

            public FileClusterProxy(FileCluster data) { Data = data; }

            public void Refresh()
            {
                Data.IsDataRead = true;
                Duration = new TimeSpanProxy(Data.Duration);
                OnPropertyChanged();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged()
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }


        static Menu CreateMenu()
            => new Menu
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = "_File",
                        Items =
                        {
                            "_Exit".MenuItem("Exit")
                        }
                    }
                }
            };
    }
}