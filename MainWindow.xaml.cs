using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.WPF;

namespace Analyzer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new ViewModels.MainViewModel();
            DataContext = vm;

            vm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(vm.IsLapsVisible): SyncPane(LapsPane, vm.IsLapsVisible); break;
                    case nameof(vm.IsSessionInfoVisible): SyncPane(SessionInfoPane, vm.IsSessionInfoVisible); break;
                    case nameof(vm.IsMapVisible): SyncPane(MapPane, vm.IsMapVisible); break;
                    case nameof(vm.IsChartsVisible): SyncPane(ChartsPane, vm.IsChartsVisible); break;
                }
            };
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
            
            // On récupère les points situés sous la souris (le mode par défaut sera utilisé)
            var dataPoints = chart.GetPointsAt(new LiveChartsCore.Drawing.LvcPoint((float)mousePoint.X, (float)mousePoint.Y));
            
            var firstPoint = dataPoints.FirstOrDefault();
            if (firstPoint != null && DataContext is ViewModels.MainViewModel vm)
            {
                // SecondaryValue correspond généralement à l'axe X (le temps dans notre cas)
                vm.UpdateCursor(firstPoint.Coordinate.SecondaryValue);
            }
        }
    }
}
