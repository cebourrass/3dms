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
            DataContext = new ViewModels.MainViewModel();
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
