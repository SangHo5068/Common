using System;
using System.Windows.Input;

using Common.Utilities;

namespace Common.Command
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }



        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
            //this._execute = execute;
        }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this._execute = execute ?? new Action<object>(ShowMessageBox);
            //this._execute = execute ?? throw new ArgumentNullException("execute");
            this._canExecute = canExecute;
        }



        protected virtual void ShowMessageBox(object obj) { }

        public virtual bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public virtual void Execute(object parameter)
        {
            Logger.WriteLog(LogTypes.Info, string.Format("Command Execute Method : {0}, param : {1}", _execute.Method.Name, parameter));
            this._execute(parameter);
        }
    }
}
