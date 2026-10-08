using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Analyzer.Services;
using Analyzer.Models;
using System.Collections.Generic;
using System.IO;

namespace Analyzer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // Ancien emplacement codé en dur, utilisé tant qu'aucun dossier n'a été configuré
        private const string LegacyDataFolderPath = @"C:\dev\3DMS-CED\3DMS Evo (38.39.8F.DC.D1.31)";

        private string _dataFolderPath = LegacyDataFolderPath;
        public string DataFolderPath
        {
            get => _dataFolderPath;
            set
            {
                if (SetProperty(ref _dataFolderPath, value?.Trim().Trim('"') ?? string.Empty))
                {
                    _settings.DataFolderPath = _dataFolderPath;
                    LoadExplorer();
                }
            }
        }

        private ExplorerItem? _selectedExplorerItem;
        public ExplorerItem? SelectedExplorerItem
        {
            get => _selectedExplorerItem;
            set
            {
                if (SetProperty(ref _selectedExplorerItem, value) && value is SessionItem session)
                {
                    LoadSession(session.FilePath);
                }
            }
        }

        private ObservableCollection<ExplorerItem> _explorerItems = new ObservableCollection<ExplorerItem>();
        public ObservableCollection<ExplorerItem> ExplorerItems
        {
            get => _explorerItems;
            set => SetProperty(ref _explorerItems, value);
        }

        private void LoadExplorer()
        {
            ExplorerItems.Clear();

            string dataPath = DataFolderPath;
            if (string.IsNullOrWhiteSpace(dataPath) || !Directory.Exists(dataPath)) return;

            var rootFolder = new FolderItem { Name = Path.GetFileName(Path.TrimEndingDirectorySeparator(dataPath)) };
            try
            {
                // Un sous-dossier par circuit/journée
                foreach (var dir in Directory.GetDirectories(dataPath).OrderBy(d => d))
                {
                    var files = Directory.GetFiles(dir, "*.ra1").OrderBy(f => f).ToList();
                    if (files.Count == 0) continue;

                    var trackFolder = new FolderItem { Name = Path.GetFileName(dir) };
                    foreach (var file in files)
                    {
                        trackFolder.Children.Add(new SessionItem { Name = Path.GetFileName(file), FilePath = file });
                    }
                    rootFolder.Children.Add(trackFolder);
                }

                // Sessions posées directement à la racine (ex. : dossier d'une seule journée)
                foreach (var file in Directory.GetFiles(dataPath, "*.ra1").OrderBy(f => f))
                {
                    rootFolder.Children.Add(new SessionItem { Name = Path.GetFileName(file), FilePath = file });
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Error reading data folder: {ex.Message}");
            }

            ExplorerItems.Add(rootFolder);
        }

        private SessionItem? FindFirstSession(IEnumerable<ExplorerItem> items)
        {
            foreach (var item in items)
            {
                if (item is SessionItem session) return session;
                if (item is FolderItem folder)
                {
                    var found = FindFirstSession(folder.Children);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}
