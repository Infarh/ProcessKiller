using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProcessKiller.Service
{
    //https://stackoverflow.com/a/9330358/2353975
    public class HotKey : IDisposable
    {
        public event EventHandler? Pressed;

        protected virtual void OnPressed(EventArgs e) => Pressed?.Invoke(this, e);

        private static Dictionary<int, HotKey>? __DictHotKeyToCalBackProc;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr HWnd, int id, uint FsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr HWnd, int id);

        public int Id { get; }

        public Key Key { get; }

        public KeyModifier KeyModifiers { get; }

        private HotKey(int Id, Key Key, KeyModifier KeyModifiers)
        {
            this.Id = Id;
            this.Key = Key;
            this.KeyModifiers = KeyModifiers;
        }

        public static HotKey? Register(Key Key, KeyModifier KeyModifiers)
        {
            var Id = KeyInterop.VirtualKeyFromKey(Key) + (int)KeyModifiers * 0x10000;
            if (!RegisterHotKey(IntPtr.Zero, Id, (uint)KeyModifiers, (uint)KeyInterop.VirtualKeyFromKey(Key)))
                return null;

            if (__DictHotKeyToCalBackProc is null)
            {
                __DictHotKeyToCalBackProc = new();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            var hot_key = new HotKey(Id, Key, KeyModifiers);

            __DictHotKeyToCalBackProc.Add(Id, hot_key);

            Debug.Print($"{Id}, {KeyInterop.VirtualKeyFromKey(Key)}");

            return hot_key;
        }

        public void Unregister()
        {
            if (__DictHotKeyToCalBackProc!.TryGetValue(Id, out _))
                UnregisterHotKey(IntPtr.Zero, Id);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled) return;
            const int wm_hot_key = 0x0312;
            if (msg.message != wm_hot_key) return;

            if (!__DictHotKeyToCalBackProc!.TryGetValue((int)msg.wParam, out var hot_key)) return;
            hot_key.OnPressed(EventArgs.Empty);
            handled = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed) return;

            if (disposing) Unregister();
            _Disposed = true;
        }
    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
