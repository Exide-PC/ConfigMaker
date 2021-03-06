﻿using System;
using System.Windows.Input;

namespace ConfigMaker.Mvvm
{
    public class DelegateCommand: ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
        {
            this._execute = execute;
        }

        public bool CanExecute(object parameter) => _canExecute != null ? _canExecute(parameter) : true;

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
