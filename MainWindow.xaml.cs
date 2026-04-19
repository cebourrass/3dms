using System.Windows;

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
    }
}
