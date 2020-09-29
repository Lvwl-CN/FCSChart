using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace FCSChart
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        readonly Action<object> ExecuteMethod;
        readonly Func<object, bool> CanExecuteMethod;

        public DelegateCommand(Action<object> executeMethod)
        {
            ExecuteMethod = executeMethod;
        }
        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            ExecuteMethod = executeMethod;
            CanExecuteMethod = canExecuteMethod;
        }


        public bool CanExecute(object parameter)
        {
            if (CanExecuteMethod == null) return true;
            return CanExecuteMethod.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteMethod?.Invoke(parameter);
        }
    }
}
