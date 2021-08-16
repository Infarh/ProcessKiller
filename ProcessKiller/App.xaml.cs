using System.Windows;

namespace ProcessKiller
{
    public partial class App
    {
        public static bool IsInDesign { get; private set; } = true;

        protected override void OnStartup(StartupEventArgs e)
        {
            IsInDesign = false;
            base.OnStartup(e);
        }
    }
}
