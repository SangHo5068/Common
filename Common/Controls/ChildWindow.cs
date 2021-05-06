using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Common.Notify;
using Common.Utilities;

namespace Common.Controls
{
    public enum WindowState
    {
        Closed,
        Open
    }

    public enum WindowStartupLocation
    {
        Center,
        Manual
    }

    [TemplatePart(Name = PART_WindowRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_Root, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_WindowControl, Type = typeof(ContentControl))]
    public class ChildWindow : ContentControl, IDisposable
    {
        private const string PART_WindowRoot = "PART_WindowRoot";
        private const string PART_Root = "PART_Root";
        private const string PART_WindowControl = "PART_WindowControl";

        #region Private Members

        private Canvas _CanvasRoot;
        //private TranslateTransform _moveTransform = new TranslateTransform();
        //private bool _startupPositionInitialized;
        //private FrameworkElement _parentContainer;
        //private Rectangle _modalLayer = new Rectangle();
        //private Canvas _modalLayerPanel = new Canvas();
        private FrameworkElement _windowRoot;
        private ContentControl _windowControl;
        private bool _ignorePropertyChanged;
        //private bool _hasWindowContainer;

        #endregion //Private Members

        #region Public Properties

        #region DialogResult

        private bool? _dialogResult;
        /// <summary>
        /// Gets or sets a value indicating whether the ChildWindow was accepted or canceled.
        /// </summary>
        /// <value>
        /// True if the child window was accepted; false if the child window was
        /// canceled. The default is null.
        /// </value>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    this.Close();
                }
            }
        }

        #endregion //DialogResult


        #region DesignerWindowState

        public static readonly DependencyProperty DesignerWindowStateProperty = DependencyProperty.Register("DesignerWindowState", typeof(WindowState), typeof(ChildWindow), new PropertyMetadata(WindowState.Closed, OnDesignerWindowStatePropertyChanged));
        public WindowState DesignerWindowState
        {
            get { return (WindowState)GetValue(DesignerWindowStateProperty); }
            set { SetValue(DesignerWindowStateProperty, value); }
        }

        private static void OnDesignerWindowStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChildWindow childWindow)
                childWindow.OnDesignerWindowStatePropertyChanged((WindowState)e.OldValue, (WindowState)e.NewValue);
        }

        protected virtual void OnDesignerWindowStatePropertyChanged(WindowState oldValue, WindowState newValue)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Visibility = newValue == WindowState.Open ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion //DesignerWindowState

        #region FocusedElement

        public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.Register("FocusedElement", typeof(FrameworkElement), typeof(ChildWindow), new UIPropertyMetadata(null));
        public FrameworkElement FocusedElement
        {
            get { return (FrameworkElement)GetValue(FocusedElementProperty); }
            set { SetValue(FocusedElementProperty, value); }
        }

        #endregion //FocusedElement


        #region WindowStartupLocation

        public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register("WindowStartupLocation", typeof(WindowStartupLocation), typeof(ChildWindow), new UIPropertyMetadata(WindowStartupLocation.Manual, OnWindowStartupLocationChanged));
        public WindowStartupLocation WindowStartupLocation
        {
            get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }

        private static void OnWindowStartupLocationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ChildWindow childWindow)
                childWindow.OnWindowStartupLocationChanged((WindowStartupLocation)e.OldValue, (WindowStartupLocation)e.NewValue);
        }

        protected virtual void OnWindowStartupLocationChanged(WindowStartupLocation oldValue, WindowStartupLocation newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //WindowStartupLocation

        #region WindowState

        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(ChildWindow), new FrameworkPropertyMetadata(WindowState.Closed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnWindowStatePropertyChanged));
        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        private static void OnWindowStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChildWindow childWindow)
                childWindow.OnWindowStatePropertyChanged((WindowState)e.OldValue, (WindowState)e.NewValue);
        }

        protected virtual void OnWindowStatePropertyChanged(WindowState oldValue, WindowState newValue)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (!_ignorePropertyChanged)
                    SetWindowState(newValue);
            }
            else
            {
                Visibility = DesignerWindowState == WindowState.Open ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion //WindowState

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(ChildWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaptionPropertyChanged));

        private static void OnCaptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChildWindow childWindow)
            { }
        }

        public string Caption2
        {
            get { return (string)GetValue(Caption2Property); }
            set { SetValue(Caption2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Caption2Property =
            DependencyProperty.Register("Caption2", typeof(string), typeof(ChildWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double ChildWidth
        {
            get { return (double)GetValue(ChildWidthProperty); }
            set { SetValue(ChildWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChildWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildWidthProperty =
            DependencyProperty.Register("ChildWidth", typeof(double), typeof(ChildWindow), new FrameworkPropertyMetadata((double)0));

        public double ChildHeight
        {
            get { return (double)GetValue(ChildHeightProperty); }
            set { SetValue(ChildHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChildHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChildHeightProperty =
            DependencyProperty.Register("ChildHeight", typeof(double), typeof(ChildWindow), new FrameworkPropertyMetadata((double)0));

        #endregion //Public Properties

        #region Constructors

        static ChildWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindow), new FrameworkPropertyMetadata(typeof(ChildWindow)));
        }

        public ChildWindow()
        {
            DesignerWindowState = WindowState.Open;
        }


        #endregion //Constructors

        #region Base Class Overrides

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
            RoutedEventArgs args = new RoutedEventArgs(CloseButtonClickedEvent, this);
            this.RaiseEvent(args);
            e.Handled = true;
        }

        Button _closeButton;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _CanvasRoot = this.GetTemplateChild(PART_Root) as Canvas;
            if (_CanvasRoot != null) _CanvasRoot.SizeChanged += CanvasRoot_SizeChanged;
            _windowRoot = this.GetTemplateChild(PART_WindowRoot) as FrameworkElement;

            try
            {
                SetLayout();

                if (_closeButton != null)
                {
                    _closeButton.Click -= CloseButton_OnClick;
                }

                _closeButton = this.GetTemplateChild("CloseButton") as Button;

                if (_closeButton != null)
                {
                    _closeButton.Click += CloseButton_OnClick;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private void CanvasRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetLayout();
        }

        private void SetLayout()
        {
            if (_windowRoot != null && _CanvasRoot != null)
            {
                //TODO: Don't moved
                //if (_windowRoot != null)
                //{
                //    _windowRoot.MouseLeftButtonDown -= _windowRoot_MouseLeftButtonDown;
                //    _windowRoot.MouseMove -= _windowRoot_MouseMove;
                //    _windowRoot.MouseLeftButtonUp -= _windowRoot_MouseLeftButtonUp;

                //    _windowRoot.MouseLeftButtonDown += _windowRoot_MouseLeftButtonDown;
                //    _windowRoot.MouseMove += _windowRoot_MouseMove;
                //    _windowRoot.MouseLeftButtonUp += _windowRoot_MouseLeftButtonUp;
                //} 

                double screenCenterX = _CanvasRoot.ActualWidth / 2;
                double screenCenterY = _CanvasRoot.ActualHeight / 2;
                //double screenCenterX = (this.Parent as FrameworkElement).ActualWidth / 2;
                //double screenCenterY = (this.Parent as FrameworkElement).ActualHeight / 2;
                double halfWidth = _windowRoot.Width / 2;
                double halfHeight = _windowRoot.Height / 2;

                Canvas.SetLeft(_windowRoot, screenCenterX - halfWidth);
                Canvas.SetTop(_windowRoot, screenCenterY - halfHeight);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SetLayout();
            //ChangeVisualState(true);
        }

        #region Data

        //// Stores a reference to the UIElement currently being dragged by the user.
        //private UIElement elementBeingDragged;

        //// Keeps track of where the mouse cursor was when a drag operation began.		
        //private Point origCursorLocation;

        //// The offsets from the DragCanvas' edges when the drag operation began.
        //private double origHorizOffset, origVertOffset;

        //// Keeps track of which horizontal and vertical offset should be modified for the drag element.
        //private bool modifyLeftOffset, modifyTopOffset;

        //// True if a drag operation is underway, else false.
        //private bool isDragInProgress;

        #endregion // Data

        #region ElementBeingDragged

        /// <summary>
        /// Returns the UIElement currently being dragged, or null.
        /// </summary>
        /// <remarks>
        /// Note to inheritors: This property exposes a protected 
        /// setter which should be used to modify the drag element.
        /// </remarks>
        //public UIElement ElementBeingDragged
        //{
        //    get;
        //    set;
        //}

        #endregion // ElementBeingDragged

        #region ResolveOffset

        /// <summary>
        /// Determines one component of a UIElement's location 
        /// within a Canvas (either the horizontal or vertical offset).
        /// </summary>
        /// <param name="side1">
        /// The value of an offset relative to a default side of the 
        /// Canvas (i.e. top or left).
        /// </param>
        /// <param name="side2">
        /// The value of the offset relative to the other side of the 
        /// Canvas (i.e. bottom or right).
        /// </param>
        /// <param name="useSide1">
        /// Will be set to true if the returned value should be used 
        /// for the offset from the side represented by the 'side1' 
        /// parameter.  Otherwise, it will be set to false.
        /// </param>
        //private static double ResolveOffset(double side1, double side2, out bool useSide1)
        //{
        //    // If the Canvas.Left and Canvas.Right attached properties 
        //    // are specified for an element, the 'Left' value is honored.
        //    // The 'Top' value is honored if both Canvas.Top and 
        //    // Canvas.Bottom are set on the same element.  If one 
        //    // of those attached properties is not set on an element, 
        //    // the default value is Double.NaN.
        //    useSide1 = true;
        //    double result;
        //    if (Double.IsNaN(side1))
        //    {
        //        if (Double.IsNaN(side2))
        //        {
        //            // Both sides have no value, so set the
        //            // first side to a value of zero.
        //            result = 0;
        //        }
        //        else
        //        {
        //            result = side2;
        //            useSide1 = false;
        //        }
        //    }
        //    else
        //    {
        //        result = side1;
        //    }
        //    return result;
        //}

        #endregion // ResolveOffset

        #region Window Moving - 사용안함

        //void _windowRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.isDragInProgress = false;

        //    // Cache the mouse cursor location.
        //    this.origCursorLocation = e.GetPosition(this);

        //    // Walk up the visual tree from the element that was clicked, 
        //    // looking for an element that is a direct child of the Canvas.
        //    this.ElementBeingDragged = (UIElement)sender;
        //    if (this.ElementBeingDragged == null)
        //        return;

        //    // Get the element's offsets from the four sides of the Canvas.
        //    double left = Canvas.GetLeft(this.ElementBeingDragged);
        //    double right = Canvas.GetRight(this.ElementBeingDragged);
        //    double top = Canvas.GetTop(this.ElementBeingDragged);
        //    double bottom = Canvas.GetBottom(this.ElementBeingDragged);

        //    // Calculate the offset deltas and determine for which sides
        //    // of the Canvas to adjust the offsets.
        //    this.origHorizOffset = ResolveOffset(left, right, out this.modifyLeftOffset);
        //    this.origVertOffset = ResolveOffset(top, bottom, out this.modifyTopOffset);

        //    // Set the Handled flag so that a control being dragged 
        //    // does not react to the mouse input.
        //    e.Handled = true;

        //    this.isDragInProgress = true;
        //}

        //void _windowRoot_MouseMove(object sender, MouseEventArgs e)
        //{
        //    // If no element is being dragged, there is nothing to do.
        //    if (this.ElementBeingDragged == null || !this.isDragInProgress || e.LeftButton == MouseButtonState.Released)
        //        return;


        //    // Get the position of the mouse cursor, relative to the Canvas.
        //    Point cursorLocation = e.GetPosition(this);

        //    // These values will store the new offsets of the drag element.
        //    double newHorizontalOffset, newVerticalOffset;

        //    #region Calculate Offsets

        //    // Determine the horizontal offset.
        //    if (this.modifyLeftOffset)
        //        newHorizontalOffset = this.origHorizOffset + (cursorLocation.X - this.origCursorLocation.X);
        //    else
        //        newHorizontalOffset = this.origHorizOffset - (cursorLocation.X - this.origCursorLocation.X);

        //    // Determine the vertical offset.
        //    if (this.modifyTopOffset)
        //        newVerticalOffset = this.origVertOffset + (cursorLocation.Y - this.origCursorLocation.Y);
        //    else
        //        newVerticalOffset = this.origVertOffset - (cursorLocation.Y - this.origCursorLocation.Y);

        //    #endregion // Calculate Offsets

        //    #region Move Drag Element

        //    if (this.modifyLeftOffset)
        //        Canvas.SetLeft(this.ElementBeingDragged, newHorizontalOffset);
        //    else
        //        Canvas.SetRight(this.ElementBeingDragged, newHorizontalOffset);

        //    if (this.modifyTopOffset)
        //        Canvas.SetTop(this.ElementBeingDragged, newVerticalOffset);
        //    else
        //        Canvas.SetBottom(this.ElementBeingDragged, newVerticalOffset);

        //    #endregion // Move Drag Element
        //}

        //void _windowRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // Reset the field whether the left or right mouse button was 
        //    // released, in case a context menu was opened on the drag element.
        //    this.ElementBeingDragged = null;
        //}

        #endregion //Window Moving

        #region override

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            Action action = () =>
            {
                if (FocusedElement != null)
                    FocusedElement.Focus();
            };

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, action);
        }

        #endregion //override

        #endregion //Base Class Overrides

        #region Event Handlers

        public static readonly RoutedEvent CloseButtonClickedEvent = EventManager.RegisterRoutedEvent("CloseButtonClicked", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ChildWindow));

        // Provide CLR accessors for the event 
        public event RoutedEventHandler CloseButtonClicked
        {
            add { AddHandler(CloseButtonClickedEvent, value); }
            remove { RemoveHandler(CloseButtonClickedEvent, value); }
        }

        protected virtual void OnCloseButtonClicked(RoutedEventArgs e)
        {
            //if (!this.IsCurrentWindow(e.OriginalSource))
            //    return;

            e.Handled = true;

            RoutedEventArgs args = new RoutedEventArgs(CloseButtonClickedEvent, this);
            this.RaiseEvent(args);

            if (!args.Handled)
            {
                this.Close();
            }
        }

        #endregion //Event Handlers

        #region Methods

        #region Private

        private void SetWindowState(WindowState state)
        {
            switch (state)
            {
                case WindowState.Closed:
                    {
                        ExecuteClose();
                        break;
                    }
                case WindowState.Open:
                    {
                        ExecuteOpen();
                        break;
                    }
            }
        }

        private void ExecuteClose()
        {
            CancelEventArgs e = new CancelEventArgs();
            OnClosing(e);

            if (!e.Cancel)
            {
                if (!_dialogResult.HasValue)
                    _dialogResult = false;

                OnClosed(EventArgs.Empty);
            }
            else
            {
                CancelClose();
            }
        }

        private void CancelClose()
        {
            _dialogResult = null; //when the close is cancelled, DialogResult should be null

            _ignorePropertyChanged = true;
            WindowState = WindowState.Open; //now reset the window state to open because the close was cancelled
            _ignorePropertyChanged = false;
        }

        private void ExecuteOpen()
        {
            SetLayout();
        }

        #endregion //Private

        #region Public

        public void Show()
        {
            WindowState = WindowState.Open;
        }

        public void Close()
        {
            WindowState = WindowState.Closed;
        }

        public virtual void Dispose()
        {
            _dialogResult = null;
            if (_CanvasRoot != null) _CanvasRoot = null;
            if (_windowRoot != null) _windowRoot = null;
            if (_windowControl != null) _windowControl = null;
            //_moveTransform = null;
            //_parentContainer = null;
            //_modalLayer = null;
            //_modalLayerPanel = null;
        }

        #endregion //Public

        #endregion //Methods

        #region Events

        /// <summary>
        /// Occurs when the ChildWindow is closed.
        /// </summary>
        public event EventHandler Closed;
        protected virtual void OnClosed(EventArgs e)
        {
            Closed?.Invoke(this, e);
            if (this.DataContext is BindableAndDisposable bindable)
                bindable.Dispose();
        }

        /// <summary>
        /// Occurs when the ChildWindow is closing.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;
        protected virtual void OnClosing(CancelEventArgs e)
        {
            Closing?.Invoke(this, e);
        }

        #endregion //Events
    }
}
