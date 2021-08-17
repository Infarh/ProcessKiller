using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ProcessKiller.ViewModels;

namespace ProcessKiller
{
    public partial class MainWindow
    {
        public MainWindowViewModel ViewModel => DataContext as MainWindowViewModel
            ?? throw new InvalidOperationException("Контекст данных окна не является его моделью-представления");

        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ViewModel.UpdateProcessesCommand.Execute(null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                e.Cancel = !ViewModel.CanClose;
            base.OnClosing(e);
            Hide();
        }

        private void ProcessList_OnMouseDoubleClick(object Sender, MouseButtonEventArgs E)
        {
            if(Sender is not ListBox { SelectedItem: Process process }) return;
            var command = ViewModel.KillProcessCommand;
            if (command.CanExecute(process))
                command.Execute(process);
        }
    }
}
