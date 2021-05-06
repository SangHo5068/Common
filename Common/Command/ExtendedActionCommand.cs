using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

namespace Common.Command
{
    public class ActionCommandBase : ICommand
    {
        bool allowExecute = true;
        public ActionCommandBase(Action<object> action, object owner)
        {
            Action = action;
            Owner = owner;
        }
        public bool AllowExecute
        {
            get { return allowExecute; }
            protected set
            {
                allowExecute = value;
                RaiseAllowExecuteChanged();
            }
        }
        public Action<object> Action { get; private set; }
        protected object Owner { get; private set; }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) { return AllowExecute; }
        public void Execute(object parameter)
        {
            Action?.Invoke(parameter);
        }
        void RaiseAllowExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ExtendedActionCommandBase : ActionCommandBase
    {
        readonly string allowExecutePropertyName;
        readonly PropertyInfo allowExecuteProperty;
        public ExtendedActionCommandBase(Action<object> action, INotifyPropertyChanged owner, string allowExecuteProperty)
            : base(action, owner)
        {
            this.allowExecutePropertyName = allowExecuteProperty;
            if (Owner != null)
            {
                this.allowExecuteProperty = Owner.GetType().GetProperty(this.allowExecutePropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (this.allowExecuteProperty == null)
                    throw new ArgumentOutOfRangeException("allowExecuteProperty");
                ((INotifyPropertyChanged)Owner).PropertyChanged += OnOwnerPropertyChanged;
            }
        }
        protected virtual void UpdateAllowExecute()
        {
            AllowExecute = Owner == null ? true : (bool)this.allowExecuteProperty.GetValue(Owner, null);
        }
        void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this.allowExecutePropertyName)
                UpdateAllowExecute();
        }
    }

    public class ExtendedActionCommand : ExtendedActionCommandBase
    {
        readonly Func<object, bool> allowExecuteCallback;
        readonly object id;
        public ExtendedActionCommand(Action<object> action, INotifyPropertyChanged owner, string allowExecuteProperty, Func<object, bool> allowExecuteCallback, object id)
            : base(action, owner, allowExecuteProperty)
        {
            this.allowExecuteCallback = allowExecuteCallback;
            this.id = id;
            UpdateAllowExecute();
        }
        protected override void UpdateAllowExecute()
        {
            AllowExecute = this.allowExecuteCallback == null ? true : this.allowExecuteCallback(this.id);
        }
    }
}
