﻿using System;
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
        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title = "Процессы";

        /// <summary>Заголовок окна</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region SelectedProcess : Process - Выбранный процесс

        /// <summary>Выбранный процесс</summary>
        private Process? _SelectedProcess;

        /// <summary>Выбранный процесс</summary>
        public Process? SelectedProcess { get => _SelectedProcess; set => Set(ref _SelectedProcess, value); }

        #endregion

        private ObservableCollection<Process>? _Processes;

        public ObservableCollection<Process>? Processes
        {
            get
            {
                if (_Processes is null) UpdateProcesses();
                return _Processes;
            }
            private set => Set(ref _Processes, value);
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
            SelectedProcess = _Processes?.FirstOrDefault();
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

        private static bool CheckProcess(Process process)
        {
            const ProcessAccessFlags flags = ProcessAccessFlags.Synchronize | ProcessAccessFlags.QueryLimitedInformation;

            using var handle = OpenProcess(flags, false, process.Id);
            return Marshal.GetLastWin32Error() == 0 && !process.HasExited;
        }

        #endregion

        #region Command KillProcessCommand - Убить процесс

        /// <summary>Убить процесс</summary>
        private LambdaCommand? _KillProcessCommand;

        /// <summary>Убить процесс</summary>
        public ICommand KillProcessCommand => _KillProcessCommand ??= new(OnKillProcessCommandExecuted, p => p is Process { HasExited: false });

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
                MessageBox.Show(e.Message, "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            processes.Remove(process);
            SelectedProcess = processes.FirstOrDefault();
        }

        #endregion
    }
}
