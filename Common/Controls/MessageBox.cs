using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Controls
{
    internal static partial class VisualStates
    {
        public const string MessageBoxButtonsGroup = "MessageBoxButtonsGroup";
        public const string OK = "OK";
        public const string OKCancel = "OKCancel";
        public const string YesNo = "YesNo";
        public const string YesNoCancel = "YesNoCancel";
    }

    [TemplateVisualState(Name = VisualStates.OK, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.OKCancel, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.YesNo, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.YesNoCancel, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplatePart(Name = PART_CancelButton, Type = typeof(CommonButton))]
    [TemplatePart(Name = PART_NoButton, Type = typeof(CommonButton))]
    [TemplatePart(Name = PART_OkButton, Type = typeof(CommonButton))]
    [TemplatePart(Name = PART_YesButton, Type = typeof(CommonButton))]
    [TemplatePart(Name = PART_WindowControl, Type = typeof(ContentControl))]
    public class MessageBox : ContentControl
    {
        private const string PART_CancelButton = "PART_CancelButton";
        private const string PART_NoButton = "PART_NoButton";
        private const string PART_OkButton = "PART_OkButton";
        private const string PART_YesButton = "PART_YesButton";
        private const string PART_CloseButton = "PART_CloseButton";
        private const string PART_WindowControl = "PART_WindowControl";

        #region Private Members

        /// <summary>
        /// The ThreadBarrier's captured SynchronizationContext
        /// </summary>
        private readonly SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;

        /// <summary>
        /// Tracks the MessageBoxButon value passed into the InitializeContainer method
        /// </summary>
        private MessageBoxButton _button = MessageBoxButton.OK;

        /// <summary>
        /// Tracks the MessageBoxResult to set as the default and focused button
        /// </summary>
        private MessageBoxResult _defaultResult = MessageBoxResult.None;

        /// <summary>
        /// Will contain the result when the messagebox is shown inside a WindowContainer
        /// </summary>
        private MessageBoxResult _dialogResult = MessageBoxResult.None;

        /// <summary>
        /// Tracks the owner of the MessageBox
        /// </summary>
        private Window _owner;

        private ContentControl _windowControl;

        #endregion //Private Members

        #region DependencyProperty

        public ICommand CommandClose
        {
            get { return (ICommand)GetValue(CommandCloseProperty); }
            set { SetValue(CommandCloseProperty, value); }
        }

        public static DependencyProperty CommandCloseProperty =
            DependencyProperty.Register("CommandClose", typeof(ICommand), typeof(MessageBox),
            new PropertyMetadata(null, OnAddCommand));

        private static void OnAddCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MessageBox).OnAddCommand(e.OldValue, e.NewValue, e.Property);
        }

        private void OnAddCommand(object oldValue, object newValue, DependencyProperty property)
        {
            if (newValue != null)
                CommandBindings.Add(new CommandBinding((ICommand)newValue, new ExecutedRoutedEventHandler(OnClose)));
        }

        #endregion //DependencyProperty

        /// <summary>
        /// MessageBox 표시 여부
        /// </summary>
        public static bool IsShow { get; set; }

        #region Constructors

        static MessageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBox), new FrameworkPropertyMetadata(typeof(MessageBox)));
        }

        public MessageBox()
        {
            this.Visibility = Visibility.Collapsed;
            IsShow = false;

            SetLanguageButtonsContent();
            this.InitHandlers();
        }

        /// <summary>
        /// 메시지박스 ButtonContent 다국어 지원
        /// </summary>
        private void SetLanguageButtonsContent()
        {
            YesButtonContent = "Yes";
            NoButtonContent = "No";
            OkButtonContent = "Ok";
            CancelButtonContent = "Cancel";
        }

        #endregion //Constructors

        #region Properties

        #region Protected Properties

        protected Window Container
        {
            get { return (this.Parent as Window); }
        }

        #endregion //Protected Properties

        #region Dependency Properties

        #region ButtonRegionBackground

        public static readonly DependencyProperty ButtonRegionBackgroundProperty = DependencyProperty.Register("ButtonRegionBackground", typeof(Brush), typeof(MessageBox), new PropertyMetadata(null));
        public Brush ButtonRegionBackground
        {
            get { return (Brush)GetValue(ButtonRegionBackgroundProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(ButtonRegionBackgroundProperty, value);
                else this._syncContext.Send(delegate { SetValue(ButtonRegionBackgroundProperty, value); }, null);
            }
        }

        #endregion //ButtonRegionBackground

        #region CancelButtonContent

        public static readonly DependencyProperty CancelButtonContentProperty = DependencyProperty.Register("CancelButtonContent", typeof(object), typeof(MessageBox), new UIPropertyMetadata("Cancel"));
        public object CancelButtonContent
        {
            get { return (object)GetValue(CancelButtonContentProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(CancelButtonContentProperty, value);
                else this._syncContext.Send(delegate { SetValue(CancelButtonContentProperty, value); }, null);
            }
        }

        #endregion //CancelButtonContent

        #region CancelButtonStyle

        public static readonly DependencyProperty CancelButtonStyleProperty = DependencyProperty.Register("CancelButtonStyle", typeof(Style), typeof(MessageBox), new PropertyMetadata(null));
        public Style CancelButtonStyle
        {
            get { return (Style)GetValue(CancelButtonStyleProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(CancelButtonStyleProperty, value);
                else this._syncContext.Send(delegate { SetValue(CancelButtonStyleProperty, value); }, null);
            }
        }

        #endregion //CancelButtonStyle

        #region ImageSource

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MessageBox), new UIPropertyMetadata(default(ImageSource)));
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(ImageSourceProperty, value);
                else this._syncContext.Send(delegate { SetValue(ImageSourceProperty, value); }, null);
            }
        }

        #endregion //ImageSource

        #region OkButtonContent

        public static readonly DependencyProperty OkButtonContentProperty = DependencyProperty.Register("OkButtonContent", typeof(object), typeof(MessageBox), new UIPropertyMetadata("OK"));
        public object OkButtonContent
        {
            get { return (object)GetValue(OkButtonContentProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(OkButtonContentProperty, value);
                else this._syncContext.Send(delegate { SetValue(OkButtonContentProperty, value); }, null);
            }
        }

        #endregion //OkButtonContent

        #region OkButtonStyle

        public static readonly DependencyProperty OkButtonStyleProperty = DependencyProperty.Register("OkButtonStyle", typeof(Style), typeof(MessageBox), new PropertyMetadata(null));
        public Style OkButtonStyle
        {
            get { return (Style)GetValue(OkButtonStyleProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(OkButtonStyleProperty, value);
                else this._syncContext.Send(delegate { SetValue(OkButtonStyleProperty, value); }, null);
            }
        }

        #endregion //OkButtonStyle

        #region MessageBoxResult

        /// <summary>
        /// Gets the MessageBox result, which is set when the "Closed" event is raised.
        /// </summary>
        public MessageBoxResult MessageBoxResult
        {
            get { return _dialogResult; }
        }

        #endregion //MessageBoxResult

        #region NoButtonContent

        public static readonly DependencyProperty NoButtonContentProperty = DependencyProperty.Register("NoButtonContent", typeof(object), typeof(MessageBox), new UIPropertyMetadata("No"));
        public object NoButtonContent
        {
            get { return (object)GetValue(NoButtonContentProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(NoButtonContentProperty, value);
                else this._syncContext.Send(delegate { SetValue(NoButtonContentProperty, value); }, null);
            }
        }

        #endregion //NoButtonContent

        #region NoButtonStyle

        public static readonly DependencyProperty NoButtonStyleProperty = DependencyProperty.Register("NoButtonStyle", typeof(Style), typeof(MessageBox), new PropertyMetadata(null));
        public Style NoButtonStyle
        {
            get { return (Style)GetValue(NoButtonStyleProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(NoButtonStyleProperty, value);
                else this._syncContext.Send(delegate { SetValue(NoButtonStyleProperty, value); }, null);
            }
        }

        #endregion //NoButtonStyle

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MessageBox), new UIPropertyMetadata(String.Empty));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(TextProperty, value);
                else this._syncContext.Send(delegate { SetValue(TextProperty, value); }, null);
            }
        }

        #endregion //Text

        #region Caption

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(MessageBox), new UIPropertyMetadata(String.Empty));
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(CaptionProperty, value);
                else this._syncContext.Send(delegate { SetValue(CaptionProperty, value); }, null);
            }
        }

        #endregion //Caption

        #region YesButtonContent

        public static readonly DependencyProperty YesButtonContentProperty = DependencyProperty.Register("YesButtonContent", typeof(object), typeof(MessageBox), new UIPropertyMetadata("Yes"));
        public object YesButtonContent
        {
            get { return (object)GetValue(YesButtonContentProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(YesButtonContentProperty, value);
                else this._syncContext.Send(delegate { SetValue(YesButtonContentProperty, value); }, null);
            }
        }

        #endregion //YesButtonContent

        #region YesButtonStyle

        public static readonly DependencyProperty YesButtonStyleProperty = DependencyProperty.Register("YesButtonStyle", typeof(Style), typeof(MessageBox), new PropertyMetadata(null));
        public Style YesButtonStyle
        {
            get { return (Style)GetValue(YesButtonStyleProperty); }
            set
            {
                if (this._syncContext == null)
                    SetValue(YesButtonStyleProperty, value);
                else this._syncContext.Send(delegate { SetValue(YesButtonStyleProperty, value); }, null);
            }
        }

        #endregion //YesButtonStyle

        #endregion //Dependency Properties

        #endregion //Properties

        #region Base Class Overrides

        //internal override bool AllowPublicIsActiveChange
        //{
        //    get { return false; }
        //}

        /// <summary>
        /// Overrides the OnApplyTemplate method.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //if (_windowControl != null)
            //{
            //    _windowControl.HeaderDragDelta -= (o, e) => this.OnHeaderDragDelta(e);
            //    _windowControl.HeaderIconDoubleClicked -= (o, e) => this.OnHeaderIconDoubleClicked(e);
            //    _windowControl.CloseButtonClicked -= (o, e) => this.OnCloseButtonClicked(e);
            //}
            _windowControl = this.GetTemplateChild(PART_WindowControl) as ContentControl;
            //if (_windowControl != null)
            //{
            //    _windowControl.HeaderDragDelta += (o, e) => this.OnHeaderDragDelta(e);
            //    _windowControl.HeaderIconDoubleClicked += (o, e) => this.OnHeaderIconDoubleClicked(e);
            //    _windowControl.CloseButtonClicked += (o, e) => this.OnCloseButtonClicked(e);
            //}

            //this.UpdateBlockMouseInputsPanel();

            ChangeVisualState(_button.ToString(), true);

            Button closeButton = GetMessageBoxButton(PART_CloseButton);
            if (closeButton != null)
                closeButton.IsEnabled = !object.Equals(_button, MessageBoxButton.YesNo);

            Button okButton = GetMessageBoxButton(PART_OkButton);
            if (okButton != null)
                okButton.IsCancel = object.Equals(_button, MessageBoxButton.OK);

            SetDefaultResult();
        }

        //protected override object OnCoerceCloseButtonVisibility(Visibility newValue)
        //{
        //    if (newValue != Visibility.Visible)
        //        throw new InvalidOperationException("Close button on MessageBox is always Visible.");
        //    return newValue;
        //}

        //protected override object OnCoerceWindowStyle(WindowStyle newValue)
        //{
        //    if (newValue != WindowStyle.SingleBorderWindow)
        //        throw new InvalidOperationException("Window style on MessageBox is not available.");
        //    return newValue;
        //}

        //internal override void UpdateBlockMouseInputsPanel()
        //{
        //    if (_windowControl != null)
        //    {
        //        _windowControl.IsBlockMouseInputsPanelActive = this.IsBlockMouseInputsPanelActive;
        //    }
        //}


        #endregion //Base Class Overrides

        #region Methods

        #region Public Static

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText)
        {
            return Show(messageText, string.Empty, MessageBoxButton.OK, (Style)null);
        }

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner of the MessageBox</param>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string messageText)
        {
            return Show(owner, messageText, string.Empty, MessageBoxButton.OK, (Style)null);
        }

        /// <summary>
        /// Displays a message box that has a message and title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption)
        {
            return Show(messageText, caption, MessageBoxButton.OK, (Style)null);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption)
        {
            return Show(owner, messageText, caption, (Style)null);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, Style messageBoxStyle)
        {
            return Show(owner, messageText, caption, MessageBoxButton.OK, messageBoxStyle);
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
        {
            return Show(messageText, caption, button, (Style)null);
        }

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, Style messageBoxStyle)
        {
            return ShowCore(null, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle);
        }

        public static MessageBoxResult Show(object content, string caption, DataTemplate contentTemplate, MessageBoxButton button, MessageBoxImage icon)
        {
            return ShowCore(null, content, caption, contentTemplate, button, icon, MessageBoxResult.None, (Style)null);
        }


        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button)
        {
            return Show(owner, messageText, caption, button, (Style)null);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle)
        {
            return ShowCore(owner, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle);
        }


        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(messageText, caption, button, icon, (Style)null);
        }

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="image"> A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            return ShowCore(null, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(owner, messageText, caption, button, icon, (Style)null);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            return ShowCore(owner, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle);
        }


        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(messageText, caption, button, icon, defaultResult, (Style)null);
        }
        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="image"> A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A System.Windows.MessageBoxResult value that specifies the default result of the MessageBox.</param>
        /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            return ShowCore(null, messageText, caption, button, icon, defaultResult, messageBoxStyle);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(owner, messageText, caption, button, icon, defaultResult, (Style)null);
        }


        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            return ShowCore(owner, messageText, caption, button, icon, defaultResult, messageBoxStyle);
        }

        /// <summary>
        /// XmlCode를 읽어서 MessageBox를 표시한다.
        /// </summary>
        /// <param name="code">Xml에서 읽어올 코드</param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="param">XmlCode내의 메시지에 들어갈 파레마타</param>
        /// <returns></returns>
        public static MessageBoxResult XmlMessageShow(string code, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, params object[] param)
        {
            //string msg = ConvertLanguage.GetCodeMessage(code, param);
            string msg = code;
            return Show(msg, caption, button, icon, (Style)null);
        }

        #endregion //Public Static

        #region Public Methods
        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox()
        {
            if (this.Container != null || this.Parent == null)
                throw new InvalidOperationException(
                  "This method is not intended to be called while displaying a MessageBox outside of a WindowContainer. Use ShowDialog() instead in that case.");

            //if (!(this.Parent is WindowContainer))
            //    throw new InvalidOperationException(
            //      "The MessageBox instance is not intended to be displayed in a container other than a WindowContainer.");

            _dialogResult = System.Windows.MessageBoxResult.None;
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox(string messageText)
        {
            this.ShowMessageBoxCore(messageText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None);
        }

        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox(string messageText, string caption)
        {
            this.ShowMessageBoxCore(messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None);
        }

        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox(string messageText, string caption, MessageBoxButton button)
        {
            this.ShowMessageBoxCore(messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None);
        }

        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            this.ShowMessageBoxCore(messageText, caption, button, icon, MessageBoxResult.None);
        }

        /// <summary>
        /// Displays this message box when embedded in a WindowContainer parent.
        /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
        /// </summary>
        public void ShowMessageBox(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            this.ShowMessageBoxCore(messageText, caption, button, icon, defaultResult);
        }

        /// <summary>
        /// Display the MessageBox window and returns only when this MessageBox closes.
        /// </summary>
        public bool? ShowDialog()
        {
            if (this.Parent != null)
                throw new InvalidOperationException(
                  "This method is not intended to be called while displaying a Message Box inside a WindowContainer. Use 'ShowMessageBox()' instead.");

            _dialogResult = System.Windows.MessageBoxResult.None;
            this.Visibility = Visibility.Visible;
            this.CreateContainer();

            return this.Container.ShowDialog();
        }

        #endregion

        #region Protected
        /// <summary>
        /// Initializes the MessageBox.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="image">The image.</param>
        protected void InitializeMessageBox(Window owner, string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
        {
            Text = text;
            Caption = caption;
            _button = button;
            _defaultResult = defaultResult;
            _owner = owner;
            SetImageSource(image);
        }

        protected void InitializeMessageBox(Window owner, object content, string caption, DataTemplate contentTemplate, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
        {
            Content = content;
            Caption = caption;
            ContentTemplate = contentTemplate;
            _button = button;
            _defaultResult = defaultResult;
            _owner = owner;
            SetImageSource(image);
        }

        /// <summary>
        /// Changes the control's visual state(s).
        /// </summary>
        /// <param name="name">name of the state</param>
        /// <param name="useTransitions">True if state transitions should be used.</param>
        protected void ChangeVisualState(string name, bool useTransitions)
        {
            VisualStateManager.GoToState(this, name, useTransitions);
        }

        #endregion //Protected

        #region Private

        private bool IsCurrentWindow(object windowtoTest)
        {
            return object.Equals(_windowControl, windowtoTest);
        }

        /// <summary>
        /// Closes the MessageBox.
        /// </summary>
        private void Close()
        {
            if (this.Container != null)
            {
                // The Window.Closed event callback will call "OnClose"
                this.Container.Close();
            }
            else
            {
                this.OnClose();
            }
        }

        /// <summary>
        /// Sets the button that represents the _defaultResult to the default button and gives it focus.
        /// </summary>
        private void SetDefaultResult()
        {
            var defaultButton = GetDefaultButtonFromDefaultResult();
            if (defaultButton != null)
            {
                defaultButton.IsDefault = true;
                defaultButton.Focus();
            }
        }

        /// <summary>
        /// Gets the default button from the _defaultResult.
        /// </summary>
        /// <returns>The default button that represents the defaultResult</returns>
        private Button GetDefaultButtonFromDefaultResult()
        {
            Button defaultButton = null;
            switch (_defaultResult)
            {
                case MessageBoxResult.Cancel:
                    defaultButton = GetMessageBoxButton(PART_CancelButton);
                    break;
                case MessageBoxResult.No:
                    defaultButton = GetMessageBoxButton(PART_NoButton);
                    break;
                case MessageBoxResult.OK:
                    defaultButton = GetMessageBoxButton(PART_OkButton);
                    break;
                case MessageBoxResult.Yes:
                    defaultButton = GetMessageBoxButton(PART_YesButton);
                    break;
                case MessageBoxResult.None:
                    defaultButton = GetDefaultButton();
                    break;
            }
            return defaultButton;
        }

        /// <summary>
        /// Gets the default button.
        /// </summary>
        /// <remarks>Used when the _defaultResult is set to None</remarks>
        /// <returns>The button to use as the default</returns>
        private Button GetDefaultButton()
        {
            Button defaultButton = null;
            switch (_button)
            {
                case MessageBoxButton.OK:
                case MessageBoxButton.OKCancel:
                    defaultButton = GetMessageBoxButton(PART_OkButton);
                    break;
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    defaultButton = GetMessageBoxButton(PART_YesButton);
                    break;
            }
            return defaultButton;
        }

        /// <summary>
        /// Gets a message box button.
        /// </summary>
        /// <param name="name">The name of the button to get.</param>
        /// <returns>The button</returns>
        private Button GetMessageBoxButton(string name)
        {
            Button button = GetTemplateChild(name) as Button;
            return button;
        }

        private void ShowMessageBoxCore(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            this.InitializeMessageBox(null, messageText, caption, button, icon, defaultResult);
            this.ShowMessageBox();
        }

        private void InitHandlers()
        {
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.Button_Click));

            //CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(ExecuteCopy)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(OnClose)));
        }

        /// <summary>
        /// Shows the MessageBox.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <returns></returns>
        private static MessageBoxResult ShowCore(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
                throw new InvalidOperationException("Static methods for MessageBoxes are not available in XBAP. Use the instance ShowMessageBox methods instead.");

            MessageBox msgBox = null;
            var dispatcher = Application.Current.Dispatcher;
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    msgBox = ShowBox(owner, messageText, caption, button, icon, defaultResult, messageBoxStyle);
                });
            }
            else
                msgBox = ShowBox(owner, messageText, caption, button, icon, defaultResult, messageBoxStyle);

            return msgBox.MessageBoxResult;
        }

        private static MessageBox ShowBox(Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            MessageBox msgBox = new MessageBox();
            msgBox.InitializeMessageBox(owner, messageText, caption, button, icon, defaultResult);

            // Setting the style to null will inhibit any implicit styles      
            if (messageBoxStyle != null)
                msgBox.Style = messageBoxStyle;

            IsShow = true;
            msgBox.ShowDialog();
            return msgBox;
        }

        private static MessageBoxResult ShowCore(Window owner, object content, string caption, DataTemplate contentTemplate, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            if (System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted)
                throw new InvalidOperationException("Static methods for MessageBoxes are not available in XBAP. Use the instance ShowMessageBox methods instead.");

            MessageBox msgBox = new MessageBox();
            msgBox.InitializeMessageBox(owner, content, caption, contentTemplate, button, icon, defaultResult);

            // Setting the style to null will inhibit any implicit styles      
            if (messageBoxStyle != null)
                msgBox.Style = messageBoxStyle;

            IsShow = true;
            msgBox.ShowDialog();
            return msgBox.MessageBoxResult;
        }

        /// <summary>
        /// Resolves the owner Window of the MessageBox.
        /// </summary>
        /// <returns>the owner Window</returns>
        private static Window ComputeOwnerWindow()
        {
            Window owner = null;
            if (Application.Current != null)
            {
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.IsActive)
                    {
                        owner = w;
                        break;
                    }
                }
            }
            return owner;
        }

        /// <summary>
        /// Sets the message image source.
        /// </summary>
        /// <param name="image">The image to show.</param>
        private void SetImageSource(MessageBoxImage image)
        {
            String iconName = String.Empty;

            switch (image)
            {
                case MessageBoxImage.Error:
                    {
                        iconName = "Error48.png";
                        break;
                    }
                case MessageBoxImage.Information:
                    {
                        iconName = "Information48.png";
                        break;
                    }
                case MessageBoxImage.Question:
                    {
                        iconName = "Question48.png";
                        break;
                    }
                case MessageBoxImage.Warning:
                    {
                        iconName = "Warning48.png";
                        break;
                    }
                case MessageBoxImage.None:
                default:
                    {
                        return;
                    }
            }

            this.ImageSource = new BitmapImage(new Uri(String.Format("../Resource/Image/{0}", iconName), UriKind.Relative));
        }

        /// <summary>
        /// Creates the container which will host the MessageBox control.
        /// </summary>
        /// <returns></returns>
        private Window CreateContainer()
        {
            var newWindow = new Window();
            newWindow.AllowsTransparency = true;
            newWindow.Background = Brushes.Transparent;
            newWindow.Content = this;
            newWindow.Owner = _owner ?? ComputeOwnerWindow();

            if (newWindow.Owner != null)
                newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            else
                newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            newWindow.ShowInTaskbar = false;
            newWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            newWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
            newWindow.WindowStyle = System.Windows.WindowStyle.None;
            newWindow.Closed += new EventHandler(OnContainerClosed);
            newWindow.MouseLeftButtonDown += newWindow_MouseLeftButtonDown;
            newWindow.Topmost = true;
            return newWindow;
        }

        void newWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window window = sender as Window;
            window.DragMove();
        }

        #endregion //Private

        #endregion //Methods

        #region Event Handlers

        /// <summary>
        /// Processes the move of a drag operation on the header.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        protected virtual void OnHeaderDragDelta(DragDeltaEventArgs e)
        {
            if (!this.IsCurrentWindow(e.OriginalSource))
                return;

            e.Handled = true;

            //DragDeltaEventArgs args = new DragDeltaEventArgs(e.HorizontalChange, e.VerticalChange);
            //args.RoutedEvent = HeaderDragDeltaEvent;
            //args.Source = this;
            //this.RaiseEvent(args);

            //if (!args.Handled)
            //{
            if (this.Container == null)
            {
                double left = 0.0;

                if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                    left = Container.Left - e.HorizontalChange;
                else
                    left = Container.Left + e.HorizontalChange;

                Container.Left = left;
                Container.Top += e.VerticalChange;
            }
            else
            {
                double left = 0.0;

                if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                    left = Container.Left - e.HorizontalChange;
                else
                    left = Container.Left + e.HorizontalChange;

                Container.Left = left;
                Container.Top += e.VerticalChange;
            }
            //}
        }

        /// <summary>
        /// Processes the double-click on the header.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.Primitives.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected virtual void OnHeaderIconDoubleClicked(MouseButtonEventArgs e)
        {
            if (!this.IsCurrentWindow(e.OriginalSource))
                return;

            e.Handled = true;

            //MouseButtonEventArgs args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
            //args.RoutedEvent = HeaderIconDoubleClickedEvent;
            //args.Source = this;
            //this.RaiseEvent(args);

            //if (!args.Handled)
            //{
            //    this.Close();
            //}
        }

        /// <summary>
        /// Processes the close button click.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloseButtonClicked(RoutedEventArgs e)
        {
            if (!this.IsCurrentWindow(e.OriginalSource))
                return;

            e.Handled = true;

            _dialogResult = object.Equals(_button, MessageBoxButton.OK)
                                   ? MessageBoxResult.OK
                                   : MessageBoxResult.Cancel;

            //RoutedEventArgs args = new RoutedEventArgs(CloseButtonClickedEvent, this);
            //this.RaiseEvent(args);

            //if (!args.Handled)
            //{
            //    this.Close();
            //}
        }

        /// <summary>
        /// Sets the MessageBoxResult according to the button pressed and then closes the MessageBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            if (button == null)
                return;

            switch (button.Name)
            {
                case PART_NoButton:
                    _dialogResult = MessageBoxResult.No;
                    this.Close();
                    break;
                case PART_YesButton:
                    _dialogResult = MessageBoxResult.Yes;
                    this.Close();
                    break;
                case PART_CancelButton:
                    _dialogResult = MessageBoxResult.Cancel;
                    this.Close();
                    break;
                case PART_OkButton:
                    _dialogResult = MessageBoxResult.OK;
                    this.Close();
                    break;
            }

            IsShow = false;
            e.Handled = true;
        }

        /// <summary>
        /// Callack to the Container.Closed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContainerClosed(object sender, EventArgs e)
        {
            this.Container.Closed -= this.OnContainerClosed;
            this.Container.Content = null;
            Debug.Assert(this.Container == null);
            this.OnClose();
        }

        private void OnClose()
        {
            this.Visibility = Visibility.Collapsed;
            this.OnClosed(EventArgs.Empty);
        }

        #endregion //Event Handlers

        #region Events

        /// <summary>
        /// Occurs when the MessageBox is closed.
        /// </summary>
        public event EventHandler Closed;
        protected virtual void OnClosed(EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }


        #endregion

        #region COMMANDS

        private void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("---------------------------");
            sb.AppendLine();
            //sb.Append(Caption);
            sb.AppendLine();
            sb.Append("---------------------------");
            sb.AppendLine();
            sb.Append(Text);
            sb.AppendLine();
            sb.Append("---------------------------");
            sb.AppendLine();
            switch (_button)
            {
                case MessageBoxButton.OK:
                    sb.Append(OkButtonContent.ToString());
                    break;
                case MessageBoxButton.OKCancel:
                    sb.Append(OkButtonContent + "     " + CancelButtonContent);
                    break;
                case MessageBoxButton.YesNo:
                    sb.Append(YesButtonContent + "     " + NoButtonContent);
                    break;
                case MessageBoxButton.YesNoCancel:
                    sb.Append(YesButtonContent + "     " + NoButtonContent + "     " + CancelButtonContent);
                    break;
            }
            sb.AppendLine();
            sb.Append("---------------------------");

            try
            {
                new UIPermission(UIPermissionClipboard.AllClipboard).Demand();
                Clipboard.SetText(sb.ToString());
            }
            catch (SecurityException)
            {
                throw new SecurityException();
            }
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            _dialogResult = e.Parameter == null ? _dialogResult : (MessageBoxResult)e.Parameter;
            this.CommandClose?.Execute(null);
            this.Close();
        }

        #endregion COMMANDS
    }
}
