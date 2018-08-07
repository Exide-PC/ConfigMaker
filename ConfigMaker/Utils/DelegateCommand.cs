using System;
using System.Windows.Input;

namespace ConfigMaker.Utils
{
    public class DelegateCommand: ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute)
        {
            this._execute = execute;
        }

        public bool CanExecute(object parameter) => _canExecute != null ? _canExecute(parameter) : true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
