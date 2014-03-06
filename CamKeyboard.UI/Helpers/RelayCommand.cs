using System;
using System.Windows.Input;

namespace CamKeyboard.UI.Helpers
{
    public delegate void ExecuteDelegate();
    public delegate bool CanExecuteDelegate();

    class RelayCommand : ICommand
    {
        private ExecuteDelegate execute;
        private CanExecuteDelegate canExecute;

        public RelayCommand(ExecuteDelegate execute)
            : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, CanExecuteDelegate canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null)
            {
                return true;
            }
            return this.canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            this.execute();
        }
    }
}
