using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CamKeyboard.UI.Helpers
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;
        private bool isExecuting;

        public AsyncCommand(Func<Task> execute) : this(execute, () => true) { }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            // if the command is not executing, execute the users' can execute logic
            return !isExecuting && canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            // tell the button that we're now executing...
            isExecuting = true;
            OnCanExecuteChanged();
            try
            {
                // execute user code
                await execute();
            }
            finally
            {
                // tell the button we're done
                isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
    }
}
