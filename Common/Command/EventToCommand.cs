using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Common.Command
{
    /// <summary>
	/// The definition of the converter used to convert an EventArgs
	/// in the <see cref="EventToCommand"/> class, if the
	/// <see cref="EventToCommand.PassEventArgsToCommand"/> property is true.
	/// Set an instance of this class to the <see cref="EventToCommand.EventArgsConverter"/>
	/// property of the EventToCommand instance.
	/// </summary>
	public interface IEventArgsConverter
    {
        /// <summary>
        /// The method used to convert the EventArgs instance.
        /// </summary>
        /// <param name="value">An instance of EventArgs passed by the
        /// event that the EventToCommand instance is handling.</param>
        /// <param name="parameter">An optional parameter used for the conversion. Use
        /// the <see cref="EventToCommand.EventArgsConverterParameter"/> property
        /// to set this value. This may be null.</param>
        /// <returns>The converted value.</returns>
        object Convert(object value, object parameter);
    }

    /// <summary>
	/// This <see cref="System.Windows.Interactivity.TriggerAction" /> can be
	/// used to bind any event on any FrameworkElement to an <see cref="ICommand" />.
	/// Typically, this element is used in XAML to connect the attached element
	/// to a command located in a ViewModel. This trigger can only be attached
	/// to a FrameworkElement or a class deriving from FrameworkElement.
	/// <para>To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
	/// and leave the CommandParameter and CommandParameterValue empty!</para>
	/// </summary>
	public partial class EventToCommand : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the EventArgs passed to the
        /// event handler will be forwarded to the ICommand's Execute method
        /// when the event is fired (if the bound ICommand accepts an argument
        /// of type EventArgs).
        /// <para>For example, use a RelayCommand&lt;MouseEventArgs&gt; to get
        /// the arguments of a MouseMove event.</para>
        /// </summary>
        public bool PassEventArgsToCommand { get; set; }

        /// <summary>
        /// Gets or sets a converter used to convert the EventArgs when using
        /// <see cref="PassEventArgsToCommand"/>. If PassEventArgsToCommand is false,
        /// this property is never used.
        /// </summary>
        public IEventArgsConverter EventArgsConverter { get; set; }

        /// <summary>
        /// The <see cref="EventArgsConverterParameter" /> dependency property's name.
        /// </summary>
        public const string EventArgsConverterParameterPropertyName = "EventArgsConverterParameter";

        /// <summary>
        /// Gets or sets a parameters for the converter used to convert the EventArgs when using
        /// <see cref="PassEventArgsToCommand"/>. If PassEventArgsToCommand is false,
        /// this property is never used. This is a dependency property.
        /// </summary>
        public object EventArgsConverterParameter
        {
            get
            {
                return this.GetValue(EventArgsConverterParameterProperty);
            }
            set
            {
                this.SetValue(EventArgsConverterParameterProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="EventArgsConverterParameter" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventArgsConverterParameterProperty = DependencyProperty.Register(
            EventArgsConverterParameterPropertyName,
            typeof(object),
            typeof(EventToCommand),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Provides a simple way to invoke this trigger programatically
        /// without any EventArgs.
        /// </summary>
        public void Invoke()
        {
            this.Invoke(null);
        }

        /// <summary>
        /// Executes the trigger.
        /// <para>To access the EventArgs of the fired event, use a RelayCommand&lt;EventArgs&gt;
        /// and leave the CommandParameter and CommandParameterValue empty!</para>
        /// </summary>
        /// <param name="parameter">The EventArgs of the fired event.</param>
        protected override void Invoke(object parameter)
        {
            if (this.AssociatedElementIsDisabled())
            {
                return;
            }

            var command = this.GetCommand();
            var commandParameter = this.CommandParameterValue;

            if (commandParameter == null && this.PassEventArgsToCommand)
            {
                commandParameter = this.EventArgsConverter == null
                    ? parameter
                    : this.EventArgsConverter.Convert(parameter, this.EventArgsConverterParameter);
            }

            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
        {
            if (element == null)
            {
                return;
            }

            if (e.OldValue != null)
            {
                ((ICommand)e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
            }

            var command = (ICommand)e.NewValue;
            if (command != null)
            {
                command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
            }

            element.EnableDisableElement();
        }

        private bool AssociatedElementIsDisabled()
        {
            var element = this.GetAssociatedObject();
            return this.AssociatedObject == null || (element != null && !element.IsEnabled);
        }

        private void EnableDisableElement()
        {
            var element = this.GetAssociatedObject();

            if (element == null)
            {
                return;
            }

            var command = this.GetCommand();

            if (this.MustToggleIsEnabledValue && command != null)
            {
                element.IsEnabled = command.CanExecute(this.CommandParameterValue);
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.EnableDisableElement();
        }
    }

    public partial class EventToCommand
    {
        /// <summary>
        /// Identifies the <see cref="CommandParameter" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(EventToCommand),
            new PropertyMetadata(
                null,
                (s, e) =>
                {
                    var sender = s as EventToCommand;

                    if (sender?.AssociatedObject == null)
                    {
                        return;
                    }

                    sender.EnableDisableElement();
                }));

        /// <summary>
        /// Identifies the <see cref="Command" /> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(EventToCommand),
            new PropertyMetadata(
                null,
                (s, e) => OnCommandChanged(s as EventToCommand, e)));

        /// <summary>
        /// Identifies the <see cref="MustToggleIsEnabled" /> dependency property
        /// </summary>
        public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register(
            "MustToggleIsEnabled",
            typeof(bool),
            typeof(EventToCommand),
            new PropertyMetadata(
                false,
                (s, e) =>
                {
                    var sender = s as EventToCommand;

                    if (sender?.AssociatedObject == null)
                    {
                        return;
                    }

                    sender.EnableDisableElement();
                }));

        private object _commandParameterValue;

        private bool? _mustToggleValue;

        /// <summary>
        /// Gets or sets the ICommand that this trigger is bound to. This
        /// is a DependencyProperty.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }

            set
            {
                this.SetValue(CommandProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="Command" />
        /// attached to this trigger. This is a DependencyProperty.
        /// </summary>
        public object CommandParameter
        {
            get
            {
                return this.GetValue(CommandParameterProperty);
            }

            set
            {
                this.SetValue(CommandParameterProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets an object that will be passed to the <see cref="Command" />
        /// attached to this trigger. This property is here for compatibility
        /// with the Silverlight version. This is NOT a DependencyProperty.
        /// For databinding, use the <see cref="CommandParameter" /> property.
        /// </summary>
        public object CommandParameterValue
        {
            get
            {
                return this._commandParameterValue ?? this.CommandParameter;
            }

            set
            {
                this._commandParameterValue = value;
                this.EnableDisableElement();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be
        /// disabled when the <see cref="Command" /> property's CanExecuteChanged
        /// event fires. If this property is true, and the command's CanExecute 
        /// method returns false, the element will be disabled. If this property
        /// is false, the element will not be disabled when the command's
        /// CanExecute method changes. This is a DependencyProperty.
        /// </summary>
        public bool MustToggleIsEnabled
        {
            get
            {
                return (bool)this.GetValue(MustToggleIsEnabledProperty);
            }

            set
            {
                this.SetValue(MustToggleIsEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the attached element must be
        /// disabled when the <see cref="Command" /> property's CanExecuteChanged
        /// event fires. If this property is true, and the command's CanExecute 
        /// method returns false, the element will be disabled. This property is here for
        /// compatibility with the Silverlight version. This is NOT a DependencyProperty.
        /// For databinding, use the <see cref="MustToggleIsEnabled" /> property.
        /// </summary>
        public bool MustToggleIsEnabledValue
        {
            get
            {
                return this._mustToggleValue ?? this.MustToggleIsEnabled;
            }

            set
            {
                this._mustToggleValue = value;
                this.EnableDisableElement();
            }
        }

        /// <summary>
        /// Called when this trigger is attached to a DependencyObject.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.EnableDisableElement();
        }

        /// <summary>
        /// This method is here for compatibility
        /// with the WPF version.
        /// </summary>
        /// <returns>The object to which this trigger
        /// is attached casted as a FrameworkElement.</returns>
        private FrameworkElement GetAssociatedObject()
        {
            return this.AssociatedObject as FrameworkElement;
        }

        /// <summary>
        /// This method is here for compatibility
        /// with the WPF version.
        /// </summary>
        /// <returns>The command that must be executed when
        /// this trigger is invoked.</returns>
        private ICommand GetCommand()
        {
            return this.Command;
        }
    }
}
