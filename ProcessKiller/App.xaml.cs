using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using ProcessKiller.Service;

namespace ProcessKiller
{
    public partial class App : Application
    {
        private static HotKey? __ActivationHotKey;

        protected override void OnStartup(StartupEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            __ActivationHotKey = HotKey.Register(Key.Oem3, KeyModifier.Ctrl | KeyModifier.Shift)
                ?? throw new InvalidOperationException("Не удалось зарегистрировать сочетание клавиш активации окна");
            __ActivationHotKey.Pressed += ActivationHotKey_OnPressed;
            base.OnStartup(e);
        }

        private void ActivationHotKey_OnPressed(object? Sender, EventArgs E) => MainWindow?.Show();
    }
}
