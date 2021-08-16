using System;
using System.Windows;
using ProcessKiller.ViewModels;

namespace ProcessKiller
{
    public partial class MainWindow
    {
        public MainWindow() => InitializeComponent();

        private void MainWindow_OnActivated(object? Sender, EventArgs E)
        {
            if (DataContext is MainWindowViewModel model)
                model.UpdateProcessesCommand.Execute(null);
        }
    }
}
