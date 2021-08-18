using System;
using System.Windows.Input;

namespace ProcessKiller.Commands.Base
{
    public abstract class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        bool ICommand.CanExecute(object? parameter) => CanExecute(parameter);
        protected virtual bool CanExecute(object? parameter) => true;

        void ICommand.Execute(object? parameter)
        {
            if (((ICommand)this).CanExecute(parameter))
                Execute(parameter);
        }
        protected abstract void Execute(object? parameter);
    }
}
