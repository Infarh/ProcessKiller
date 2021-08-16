using System.Collections.Generic;
using System.Diagnostics;
using ProcessKiller.ViewModels.Base;

namespace ProcessKiller.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            if (App.IsInDesign) return;


        }

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Процессы";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        public IEnumerable<Process> Processes => Process.GetProcesses();
    }
}
