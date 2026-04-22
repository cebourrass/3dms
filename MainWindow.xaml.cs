using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.WPF;
using Xceed.Wpf.AvalonDock.Layout;
using System.Collections.Generic;

namespace Analyzer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new ViewModels.MainViewModel();
            DataContext = vm;
            
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            vm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(vm.IsLapsVisible): SyncPane(LapsPane, vm.IsLapsVisible); break;
                    case nameof(vm.IsSessionInfoVisible): SyncPane(SessionInfoPane, vm.IsSessionInfoVisible); break;
                    case nameof(vm.IsMapVisible): SyncPane(MapPane, vm.IsMapVisible); break;
                    case nameof(vm.IsChartsVisible): SyncPane(ChartsPane, vm.IsChartsVisible); break;
                    case nameof(vm.IsExplorerVisible): SyncPane(ExplorerPane, vm.IsExplorerVisible); break;
                }
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // Restaurer la géométrie de la fenêtre
                var settings = new Services.SettingsService().LoadSettings();
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;
                if (System.Enum.TryParse<WindowState>(settings.WindowState, out var state))
                    this.WindowState = state;

                // Restaurer le layout AvalonDock
                string layoutPath = new Services.SettingsService().GetLayoutPath();
                if (System.IO.File.Exists(layoutPath))
                {
                    try
                    {
                        var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(DockManager);
                        serializer.LayoutSerializationCallback += (s, args) =>
                        {
                            // On récupère le contenu UI défini dans le XAML pour l'injecter dans le nouveau layout
                            var existingPane = this.FindName(args.Model.ContentId) as Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable;
                            if (existingPane != null)
                            {
                                args.Content = existingPane.Content;
                            }
                        };
                        serializer.Deserialize(layoutPath);

                        // IMPORTANT : Après désérialisation, les objets Anchorable ont été recréés.
                        // On doit remettre à jour nos références locales pour que le SyncPane fonctionne toujours.
                        var allPanes = DockManager.Layout.Descendents().OfType<LayoutAnchorable>().ToList();
                        ExplorerPane = allPanes.FirstOrDefault(p => p.ContentId == "ExplorerPane") ?? ExplorerPane;
                        LapsPane = allPanes.FirstOrDefault(p => p.ContentId == "LapsPane") ?? LapsPane;
                        SessionInfoPane = allPanes.FirstOrDefault(p => p.ContentId == "SessionInfoPane") ?? SessionInfoPane;
                        MapPane = allPanes.FirstOrDefault(p => p.ContentId == "MapPane") ?? MapPane;
                        ChartsPane = allPanes.FirstOrDefault(p => p.ContentId == "ChartsPane") ?? ChartsPane;
                    }
                    catch 
                    { 
                        // En cas d'erreur de désérialisation, on supprime le fichier corrompu
                        System.IO.File.Delete(layoutPath);
                    }
                }
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                // Sauvegarder les paramètres dans le JSON
                vm.SaveSettings(this.ActualWidth, this.ActualHeight, this.WindowState.ToString());

                // Sauvegarder le layout AvalonDock dans le XML
                try
                {
                    var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(DockManager);
                    string layoutPath = new Services.SettingsService().GetLayoutPath();
                    serializer.Serialize(layoutPath);
                }
                catch { }
            }
        }

        private void SyncPane(Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable pane, bool visible)
        {
            if (pane == null) return;
            if (visible && !pane.IsVisible) pane.Show();
            else if (!visible && pane.IsVisible) pane.Hide();
        }

        private void OpenSession_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Fichiers session 3DMS (*.ra1)|*.ra1|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionner une session 3DMS"
            };

            if (dialog.ShowDialog() == true)
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.LoadSessionCommand.Execute(dialog.FileName);
                }
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.SelectedExplorerItem = e.NewValue as Models.ExplorerItem;
            }
        }

        private void Pane_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string paneName)
            {
                var pane = this.FindName(paneName) as Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable;
                pane?.Show();
            }
        }

        private void Pane_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string paneName)
            {
                var pane = this.FindName(paneName) as Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable;
                pane?.Hide();
            }
        }

        private void Pane_IsVisibleChanged(object sender, System.EventArgs e)
        {
            if (sender is Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable pane && DataContext is ViewModels.MainViewModel vm)
            {
                bool visible = pane.IsVisible;
                switch (pane.ContentId)
                {
                    case "LapsPane": vm.IsLapsVisible = visible; break;
                    case "SessionInfoPane": vm.IsSessionInfoVisible = visible; break;
                    case "MapPane": vm.IsMapVisible = visible; break;
                    case "ChartsPane": vm.IsChartsVisible = visible; break;
                    case "ExplorerPane": vm.IsExplorerVisible = visible; break;
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dg && DataContext is ViewModels.MainViewModel vm)
            {
                vm.ComparisonLaps.Clear();
                foreach (var item in dg.SelectedItems)
                {
                    if (item is Models.LapData lap)
                    {
                        vm.ComparisonLaps.Add(lap);
                    }
                }
                vm.UpdateTelemetryCharts();
            }
        }

        private void Chart_PointerMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var chart = (CartesianChart)sender;
            var mousePoint = e.GetPosition(chart);
            
            // On convertit directement la position X de la souris en valeur sur l'axe X (temps ou distance)
            // C'est beaucoup plus performant que GetPointsAt qui doit scanner tous les points de toutes les séries.
            var dataPoint = chart.ScalePixelsToData(new LiveChartsCore.Drawing.LvcPointD(mousePoint.X, mousePoint.Y));
            
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.UpdateCursor(dataPoint.X);
            }
        }
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string propertyName && DataContext is ViewModels.MainViewModel vm)
            {
                var dialog = new System.Windows.Forms.ColorDialog();
                
                // Récupérer la couleur actuelle depuis le ViewModel
                string currentHex = propertyName switch
                {
                    "SpeedColor" => vm.SpeedColor,
                    "AngleColor" => vm.AngleColor,
                    "AngleRightColor" => vm.AngleRightColor,
                    "AccelColor" => vm.AccelColor,
                    "DecelColor" => vm.DecelColor,
                    "RefColor" => vm.RefColor,
                    _ => "#FFFFFF"
                };

                try {
                    var color = System.Drawing.ColorTranslator.FromHtml(currentHex);
                    dialog.Color = color;
                } catch { }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string newHex = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                    
                    // Appliquer au ViewModel
                    switch (propertyName)
                    {
                        case "SpeedColor": vm.SpeedColor = newHex; break;
                        case "AngleColor": vm.AngleColor = newHex; break;
                        case "AngleRightColor": vm.AngleRightColor = newHex; break;
                        case "AccelColor": vm.AccelColor = newHex; break;
                        case "DecelColor": vm.DecelColor = newHex; break;
                        case "RefColor": vm.RefColor = newHex; break;
                    }
                }
            }
        }
    }
}
