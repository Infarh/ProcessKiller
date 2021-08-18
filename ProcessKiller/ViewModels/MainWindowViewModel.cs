using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32.SafeHandles;

using ProcessKiller.Commands;
using ProcessKiller.Service;
using ProcessKiller.ViewModels.Base;

namespace ProcessKiller.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        //public MainWindowViewModel()
        //{
        //    PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorTraceListener(this));
        //    PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
        //}

        private readonly HashSet<int> _BadPID = new();

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Процессы";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region CanClose : bool - Возможность завершения работы приложения

        /// <summary>Возможность завершения работы приложения</summary>
        private bool _CanClose;

        /// <summary>Возможность завершения работы приложения</summary>
        public bool CanClose { get => _CanClose; set => Set(ref _CanClose, value); }

        #endregion

        #region SelectedProcess : Process - Выбранный процесс

        /// <summary>Выбранный процесс</summary>
        private Process? _SelectedProcess;

        /// <summary>Выбранный процесс</summary>
        public Process? SelectedProcess
        {
            get => _SelectedProcess;
            set
            {
                if (value is null && _Processes is { Count: > 0 } processes) 
                    value = processes[0];
                Set(ref _SelectedProcess, value);
            }
        }

        #endregion

        private ObservableCollection<Process>? _Processes;

        public ObservableCollection<Process>? Processes
        {
            get
            {
                if (_Processes is null) UpdateProcesses();
                return _Processes;
            }
            private set
            {
                var old = _Processes;
                if (!Set(ref _Processes, value)) return;
                if (old != null) foreach (var process in old) process.Dispose();
                var selected_process = value?.FirstOrDefault();
                SelectedProcess = selected_process;
                Title = $"({value?.Count ?? 0}) Process Killer";
            }
        }


        #region Command UpdateProcessesCommand - Обновление списка процессов

        /// <summary>Обновление списка процессов</summary>
        private LambdaCommand? _UpdateProcessesCommand;

        /// <summary>Обновление списка процессов</summary>
        public ICommand UpdateProcessesCommand => _UpdateProcessesCommand ??= new(OnUpdateProcessesCommandExecuted);

        /// <summary>Логика выполнения - Обновление списка процессов</summary>
        private void OnUpdateProcessesCommandExecuted(object? p) => UpdateProcesses();

        private async void UpdateProcesses()
        {
            await Task.Yield().ConfigureAwait();
            Processes = new ObservableCollection<Process>(Process.GetProcesses().Where(CheckProcess).OrderByDescending(p => p.StartTime));
        }

        /// <summary>Обновление списка процессов, упорядоченных по объёму памяти</summary>
        private LambdaCommand? _UpdateProcessesByWorkingSetCommand;

        /// <summary>Обновление списка процессов, упорядоченных по объёму памяти</summary>
        public ICommand UpdateProcessesByWorkingSetCommand => _UpdateProcessesByWorkingSetCommand ??= new(OnUpdateProcessesByWorkingSetCommandExecuted);

        /// <summary>Логика выполнения - Обновление списка процессов, упорядоченных по объёму памяти</summary>
        private void OnUpdateProcessesByWorkingSetCommandExecuted(object? p) => UpdateProcessesByWorkingSet();

        private async void UpdateProcessesByWorkingSet()
        {
            await Task.Yield().ConfigureAwait();
            Processes = new ObservableCollection<Process>(Process.GetProcesses().Where(CheckProcess).OrderByDescending(p => p.WorkingSet64));
        }

        /// <summary>Обновление списка процессов, упорядоченных по расходу процессорного времени</summary>
        private LambdaCommand? _UpdateProcessesByProcessorTimeCommand;

        /// <summary>Обновление списка процессов, упорядоченных по расходу процессорного времени</summary>
        public ICommand UpdateProcessesByProcessorTimeCommand => _UpdateProcessesByProcessorTimeCommand ??= new(OnUpdateProcessesByProcessorTimeCommandExecuted);

        /// <summary>Логика выполнения - Обновление списка процессов, упорядоченных по расходу процессорного времени</summary>
        private void OnUpdateProcessesByProcessorTimeCommandExecuted(object? p) => UpdateProcessesByProcessorTime();

        private async void UpdateProcessesByProcessorTime()
        {
            await Task.Yield().ConfigureAwait();
            Processes = new ObservableCollection<Process>(Process.GetProcesses().Where(CheckProcess).OrderByDescending(p => p.TotalProcessorTime.TotalSeconds / (DateTime.Now - p.StartTime).TotalSeconds));
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            //All = 0x001F0FFF,
            //Terminate = 0x00000001,
            //CreateThread = 0x00000002,
            //VirtualMemoryOperation = 0x00000008,
            //VirtualMemoryRead = 0x00000010,
            //VirtualMemoryWrite = 0x00000020,
            //DuplicateHandle = 0x00000040,
            //CreateProcess = 0x000000080,
            //SetQuota = 0x00000100,
            //SetInformation = 0x00000200,
            //QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeProcessHandle OpenProcess(
            ProcessAccessFlags Access,
            bool InheritHandle,
            int ProcessId
        );

        private bool CheckProcess(Process process)
        {
            if (_BadPID.Contains(process.Id)) return false;
            const ProcessAccessFlags flags = ProcessAccessFlags.Synchronize | ProcessAccessFlags.QueryLimitedInformation;

            using var handle = OpenProcess(flags, false, process.Id);
            var success = Marshal.GetLastWin32Error() == 0 && !process.HasExited;
            if (!success)
                _BadPID.Add(process.Id);
            return success;
        }

        #endregion

        #region Command KillProcessCommand - Убить процесс

        /// <summary>Убить процесс</summary>
        private LambdaCommand? _KillProcessCommand;

        /// <summary>Убить процесс</summary>
        public ICommand KillProcessCommand => _KillProcessCommand ??= new(OnKillProcessCommandExecuted, OnKillProcessCommandCanExecuted);

        private bool OnKillProcessCommandCanExecuted(object? p)
        {
            if (p is not Process process) return false;
            return true;
            //try
            //{
            //    if (_BadPID.Contains(process.Id)) return false;
            //    return !process.HasExited;
            //}
            //catch (Win32Exception)
            //{
            //    _BadPID.Add(process.Id);
            //    return false;
            //}
            //catch (InvalidOperationException)
            //{
            //    Debug.WriteLine("Strange process");
            //    return false;
            //}
        }

        /// <summary>Логика выполнения - Убить процесс</summary>
        private void OnKillProcessCommandExecuted(object? p)
        {
            if (p is not Process { HasExited: false } process || _Processes is not { } processes) return;
            process.CloseMainWindow();
            try
            {
                process.Kill(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            }
            catch (Win32Exception e)
            {
                _BadPID.Add(process.Id);
                _Processes.Remove(process);
                MessageBox.Show(e.Message, "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateProcesses();
        }

        #endregion
    }
}
