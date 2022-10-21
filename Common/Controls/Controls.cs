using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

using Common.Utilities;

namespace Common.Controls
{
    #region Border
    public class BorderMainHeader : Control
    {
        #region DependencyProperties
        public static readonly DependencyProperty CloseCommandProperty;
        public static readonly DependencyProperty CommandLogoutProperty;
        public static readonly DependencyProperty CommandWindowMinProperty;
        public static readonly DependencyProperty CommandWindowMaxProperty;

        public static readonly DependencyProperty HeaderProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public static readonly DependencyProperty ImageProperty;
        public static readonly DependencyProperty ImageWidthProperty;
        public static readonly DependencyProperty ImageHeightProperty;

        public static readonly DependencyProperty IsShowLogoutProperty;
        public static readonly DependencyProperty IsShowMinProperty;
        public static readonly DependencyProperty IsShowMaxProperty;
        #endregion //DependencyProperties

        #region Properties
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
        public ICommand CommandLogout
        {
            get { return (ICommand)GetValue(CommandLogoutProperty); }
            set { SetValue(CommandLogoutProperty, value); }
        }
        public ICommand CommandWindowMin
        {
            get { return (ICommand)GetValue(CommandWindowMinProperty); }
            set { SetValue(CommandWindowMinProperty, value); }
        }
        public ICommand CommandWindowMax
        {
            get { return (ICommand)GetValue(CommandWindowMaxProperty); }
            set { SetValue(CommandWindowMaxProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public Visibility IsShowLogout
        {
            get { return (Visibility)GetValue(IsShowLogoutProperty); }
            set { SetValue(IsShowLogoutProperty, value); }
        }
        public Visibility IsShowMin
        {
            get { return (Visibility)GetValue(IsShowMinProperty); }
            set { SetValue(IsShowMinProperty, value); }
        }
        public Visibility IsShowMax
        {
            get { return (Visibility)GetValue(IsShowMaxProperty); }
            set { SetValue(IsShowMaxProperty, value); }
        }
        #endregion //Properties



        static BorderMainHeader()
        {
            Type owner = typeof(BorderMainHeader);
            CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), owner, new PropertyMetadata(null));
            CommandLogoutProperty = DependencyProperty.Register(nameof(CommandLogout), typeof(ICommand), owner, new PropertyMetadata(null));
            CommandWindowMinProperty = DependencyProperty.Register(nameof(CommandWindowMin), typeof(ICommand), owner, new PropertyMetadata(null));
            CommandWindowMaxProperty = DependencyProperty.Register(nameof(CommandWindowMax), typeof(ICommand), owner, new PropertyMetadata(null));

            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), owner, new PropertyMetadata(null));
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), owner, new PropertyMetadata(null));
            ImageWidthProperty = DependencyProperty.Register(nameof(ImageWidth), typeof(double), owner, new PropertyMetadata(36D));
            ImageHeightProperty = DependencyProperty.Register(nameof(ImageHeight), typeof(double), owner, new PropertyMetadata(36D));

            IsShowLogoutProperty = DependencyProperty.Register(nameof(IsShowLogout), typeof(Visibility), owner, new PropertyMetadata(Visibility.Collapsed));
            IsShowMinProperty = DependencyProperty.Register(nameof(IsShowMin), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
            IsShowMaxProperty = DependencyProperty.Register(nameof(IsShowMax), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
        }
        public BorderMainHeader()
        {

        }


        //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseLeftButtonUp(e);
        //    e.Handled = true;
        //}
    }
    public class MainHeader : Control
    {
        #region DependencyProperties
        public static readonly DependencyProperty CloseCommandProperty;
        public static readonly DependencyProperty HeaderProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public static readonly DependencyProperty ImageProperty;
        public static readonly DependencyProperty ImageWidthProperty;
        public static readonly DependencyProperty ImageHeightProperty;

        public static readonly DependencyProperty IsShowMinProperty;
        public static readonly DependencyProperty IsShowMaxProperty;
        #endregion //DependencyProperties

        #region Properties
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public Visibility IsShowMin
        {
            get { return (Visibility)GetValue(IsShowMinProperty); }
            set { SetValue(IsShowMinProperty, value); }
        }
        public Visibility IsShowMax
        {
            get { return (Visibility)GetValue(IsShowMaxProperty); }
            set { SetValue(IsShowMaxProperty, value); }
        }
        #endregion //Properties



        static MainHeader()
        {
            Type owner = typeof(MainHeader);
            CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), owner, new PropertyMetadata(null));
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), owner, new PropertyMetadata(null));
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), owner, new PropertyMetadata(null));
            ImageWidthProperty = DependencyProperty.Register(nameof(ImageWidth), typeof(double), owner, new PropertyMetadata(36D));
            ImageHeightProperty = DependencyProperty.Register(nameof(ImageHeight), typeof(double), owner, new PropertyMetadata(36D));
            IsShowMinProperty = DependencyProperty.Register(nameof(IsShowMin), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
            IsShowMaxProperty = DependencyProperty.Register(nameof(IsShowMax), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
        }
        public MainHeader()
        {

        }
    }

    public class BorderContentHeader : ContentControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty CloseCommandProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public static readonly DependencyProperty IsShowMinProperty;
        public static readonly DependencyProperty IsShowMaxProperty;
        #endregion //DependencyProperties

        #region Properties
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Visibility IsShowMin
        {
            get { return (Visibility)GetValue(IsShowMinProperty); }
            set { SetValue(IsShowMinProperty, value); }
        }
        public Visibility IsShowMax
        {
            get { return (Visibility)GetValue(IsShowMaxProperty); }
            set { SetValue(IsShowMaxProperty, value); }
        }
        #endregion //Properties



        static BorderContentHeader()
        {
            Type owner = typeof(BorderContentHeader);
            CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), owner, new PropertyMetadata(null));
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            IsShowMinProperty = DependencyProperty.Register(nameof(IsShowMin), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
            IsShowMaxProperty = DependencyProperty.Register(nameof(IsShowMax), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
        }
    }
    public class BorderMainFooter : ContentControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty CornerRadiusProperty;
        public static readonly DependencyProperty ImageProperty;
        #endregion //DependencyProperties

        #region Properties
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        #endregion //Properties



        static BorderMainFooter()
        {
            Type owner = typeof(BorderMainFooter);
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), owner, new PropertyMetadata(null));
        }
    }

    public class StrokeBorder : Border
    {
        #region DependencyProperties
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty FillProperty;
        public static readonly DependencyProperty StrokeDashArrayProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        #endregion //DependencyProperties

        #region Properties
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }
        public Thickness StrokeThickness
        {
            get { return (Thickness)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion //Properties


        static StrokeBorder()
        {
            Type owner = typeof(StrokeBorder);
            StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), owner, new PropertyMetadata(null));
            FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), owner, new PropertyMetadata(null));
            StrokeDashArrayProperty = DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), owner, new PropertyMetadata(new DoubleCollection()));
            StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(Thickness), owner, new PropertyMetadata(new Thickness(0)));
        }
    }
    #endregion //Border

    #region Button
    public class CommonButton : Button { }
    public class PopupCommonButton : Button { }

    public class InitialButton : Button { }
    public class IconButton : Button
    {
        #region DependencyProperty
        public static readonly DependencyProperty CornerRadiusProperty;
        public static readonly DependencyProperty IconProperty;
        public static readonly DependencyProperty IconWidthProperty;
        public static readonly DependencyProperty IconHeightProperty;
        #endregion //DependencyProperty

        #region Property

        #region CornerRadius
        /// <summary>
        /// CornerRadius
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        #endregion //CornerRadius

        #endregion //Property

        static IconButton()
        {
            Type owner = typeof(IconButton);
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(default));

            IconProperty       = DependencyProperty.Register(nameof(Icon), typeof(ImageSource), owner, new PropertyMetadata(null));
            IconWidthProperty  = DependencyProperty.Register(nameof(IconWidth), typeof(double), owner, new PropertyMetadata(32D));
            IconHeightProperty = DependencyProperty.Register(nameof(IconHeight), typeof(double), owner, new PropertyMetadata(32D));
        }
    }

    public class LogoutButton : Button { }
    public class RadiusButton : Button
    {
        #region DependencyProperty
        public static DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(RadiusButton), new PropertyMetadata(null));
        #endregion //DependencyProperty

        #region Property

        #region CornerRadius
        /// <summary>
        /// CornerRadius
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        #endregion //CornerRadius

        #endregion //Property
    }
    public class RemoveTextButton : Button
    {
        /// <summary>
        /// Initializes the <see cref="CrossButton"/> class.
        /// </summary>
        static RemoveTextButton()
        {
            //  Set the style key, so that our control template is used.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RemoveTextButton), new FrameworkPropertyMetadata(typeof(RemoveTextButton)));
        }
    }
    public class SearchButton : Button { }

    #region Window Button
    public class WindowMinButton : Button { }
    public class WindowMaxButton : Button { }
    public class WindowCloseButton : Button { }
    #endregion //Window Button

    #region Paging
    public class PagingCommon : Button { }
    public class PagingFirst : Button { }
    public class PagingPrev : Button { }
    public class PagingNext : Button { }
    public class PagingLast : Button { }
    #endregion //Paging
    #endregion //Button

    #region ComboBox
    public class PopupComboBox : ComboBox { }
    public class PopupMultiComboBox : ComboBox { }
    #endregion //ComboBox

    #region Popup
    /// <summary>
    /// Popup with code to not be the topmost control
    /// </summary>
    public class NotTopPopup : Popup
    {
        #region P/Invoke imports & definitions
#pragma warning disable 1591 //Xml-doc
#pragma warning disable 169 //Never used-warning
        // ReSharper disable InconsistentNaming
        // Imports etc. with their naming rules

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOZORDER = 0x0004;
        private const UInt32 SWP_NOREDRAW = 0x0008;
        private const UInt32 SWP_NOACTIVATE = 0x0010;

        private const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
        private const UInt32 SWP_SHOWWINDOW = 0x0040;
        private const UInt32 SWP_HIDEWINDOW = 0x0080;
        private const UInt32 SWP_NOCOPYBITS = 0x0100;
        private const UInt32 SWP_NOOWNERZORDER = 0x0200; /* Don’t do owner Z ordering */
        private const UInt32 SWP_NOSENDCHANGING = 0x0400; /* Don’t send WM_WINDOWPOSCHANGING */

        private const UInt32 TOPMOST_FLAGS = SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;

        // ReSharper restore InconsistentNaming
#pragma warning restore 1591
#pragma warning restore 169
        #endregion //P/Invoke imports & definitions

        /// <summary>
        /// Is Topmost dependency property
        /// </summary>
        public static readonly DependencyProperty IsTopmostProperty =
            DependencyProperty.Register("IsTopmost", typeof(bool), typeof(NotTopPopup), new FrameworkPropertyMetadata(false, OnIsTopmostChanged));

        private bool? _appliedTopMost;
        private bool _alreadyLoaded;
        private Window _parentWindow;
        //private bool _IsMouseDown = false;

        /// <summary>
        /// Get/Set IsTopmost
        /// </summary>
        public bool IsTopmost
        {
            get { return (bool)GetValue(IsTopmostProperty); }
            set { SetValue(IsTopmostProperty, value); }
        }


        /// <summary>
        /// ctor
        /// </summary>
        public NotTopPopup()
        {
            Loaded += OnPopupLoaded;
            Unloaded += OnPopupUnloaded;
        }


        void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            if (_alreadyLoaded)
                return;

            _alreadyLoaded = true;

            if (Child != null)
            {
                Child.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown), true);
            }

            _parentWindow = Window.GetWindow(this);

            if (_parentWindow == null)
                return;

            _parentWindow.Activated += OnParentWindowActivated;
            _parentWindow.Deactivated += OnParentWindowDeactivated;
            _parentWindow.LocationChanged += _parentWindow_LocationChanged;
        }

        void _parentWindow_LocationChanged(object sender, EventArgs e)
        {
            var offset = this.HorizontalOffset;
            this.HorizontalOffset = offset + 1;
            this.HorizontalOffset = offset;
        }

        private void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow == null)
                return;
            _parentWindow.Activated -= OnParentWindowActivated;
            _parentWindow.Deactivated -= OnParentWindowDeactivated;
            _parentWindow.LocationChanged -= _parentWindow_LocationChanged;
            if (Child != null)
                Child.RemoveHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown));
        }

        void OnParentWindowActivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Parent Window Activated");
            SetTopmostState(true);
        }

        void OnParentWindowDeactivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Parent Window Deactivated");

            if (IsTopmost == false)
                SetTopmostState(IsTopmost);
        }

        void OnChildPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Child Mouse Left Button Down");

            SetTopmostState(true);

            if (!_parentWindow.IsActive && IsTopmost == false)
            {
                _parentWindow.Activate();
                Debug.WriteLine("Activating Parent from child Left Button Down");
            }
        }

        private static void OnIsTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var thisobj = (NotTopPopup)obj;

            thisobj.SetTopmostState(thisobj.IsTopmost);
        }

        protected override void OnOpened(EventArgs e)
        {
            SetTopmostState(IsTopmost);
            base.OnOpened(e);
        }

        private void SetTopmostState(bool isTop)
        {
            // Don’t apply state if it’s the same as incoming state
            if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
                return;

            if (Child == null)
                return;

            var hwndSource = (PresentationSource.FromVisual(Child)) as HwndSource;

            if (hwndSource == null)
                return;
            var hwnd = hwndSource.Handle;

            RECT rect;

            if (!GetWindowRect(hwnd, out rect))
                return;

            Debug.WriteLine("setting z-order " + isTop);

            if (isTop)
            {
                SetWindowPos(hwnd, HWND_TOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }
            else
            {
                // Z-Order would only get refreshed/reflected if clicking the
                // the titlebar (as opposed to other parts of the external
                // window) unless I first set the popup to HWND_BOTTOM
                // then HWND_TOP before HWND_NOTOPMOST
                SetWindowPos(hwnd, HWND_BOTTOM, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_TOP, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_NOTOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }

            _appliedTopMost = isTop;
        }
    }
    #endregion //Popup

    #region MessageBox
    public class MessageGrid : Grid { }
    public class MessagePanel : StackPanel { }
    public class MessageBorder : Border { }
    #endregion //MessageBox

    #region Panels
    public class PopupBottomPanel : StackPanel { }

    public class HorizonPanel : StackPanel { }
    public class VerticalPanel : StackPanel { }
    public class RightPanel : StackPanel { }
    #endregion //Panels

    #region PasswordBox
    public class WaterMarkPassword : ContentControl
    {
        private const string PART_Remove = "PART_Remove";
        private Button _Remove;

        #region DependencyProperty
        public static DependencyProperty WaterMarkProperty = DependencyProperty.Register(nameof(WaterMark), typeof(String), typeof(WaterMarkPassword), new PropertyMetadata(null));
        public static DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(String), typeof(WaterMarkPassword), new PropertyMetadata(null));
        public static DependencyProperty PasswordCharProperty = DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(WaterMarkPassword), new PropertyMetadata(null));
        public static DependencyProperty IsEnabledRemoveProperty = DependencyProperty.Register(nameof(IsEnabledRemove), typeof(Boolean), typeof(WaterMarkPassword), new PropertyMetadata(true));
        #endregion //DependencyProperty

        #region Property

        #region WaterMark
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String WaterMark
        {
            get { return (String)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        #endregion //WaterMark

        #region Password
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String Password
        {
            get { return (String)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        #endregion //WaterMark

        #region PasswordChar
        /// <summary>
        /// PasswordChar
        /// </summary>
        public char PasswordChar
        {
            get { return (char)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }
        #endregion //PasswordChar

        #region IsEnabledRemove
        /// <summary>
        /// 텍스트 삭제 버튼 표시 여부
        /// </summary>
        public Boolean IsEnabledRemove
        {
            get { return (Boolean)GetValue(IsEnabledRemoveProperty); }
            set { SetValue(IsEnabledRemoveProperty, value); }
        }
        #endregion //IsEnabledRemove

        #endregion //Property

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild(PART_Remove) is Button btn)
            {
                _Remove = btn;
                _Remove.Click += (s, e) => { this.Password = string.Empty; };
            }
        }
    }
    #endregion //PasswordBox

    #region ScrollBar
    public class PopupScrollBar : ScrollBar { }
    #endregion //ScrollBar
    #region ScrollViewer
    public class PopupScrollViewer : ScrollViewer { }
    #endregion //ScrollViewer

    #region TextBlock
    /// <summary>
    /// TextBlock의 TextTrimed를 확인하기 위한 서비스
    /// </summary>
    public class TextBlockService
    {
        static TextBlockService()
        {
            // Register for the SizeChanged event on all TextBlocks, even if the event was handled.
            EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTextBlockSizeChanged), true);
        }

        public static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsTextTrimmed",
            typeof(bool),
            typeof(TextBlockService),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Boolean GetIsTextTrimmed(TextBlock target)
        {
            return (Boolean)target.GetValue(IsTextTrimmedProperty);
        }

        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is TextBlockEx textBlock))
                return;
            if (textBlock.IsTrimmingToolTip)
                textBlock.SetValue(IsTextTrimmedKey, CalculateIsTextTrimmed(textBlock));
        }

        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            double width = textBlock.ActualWidth;
            if (textBlock.TextTrimming == TextTrimming.None)
                return false;
            if (textBlock.TextWrapping != TextWrapping.NoWrap)
                return false;
            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
            double totalWidth = textBlock.DesiredSize.Width;
            return width < totalWidth;
        }
    }

    /// <summary>
    /// TextTrimming 속성이 CharacterEllipsis 일 때 IsTrimmingToolTip="True" 이면 ToolTip를 표시한다.
    /// </summary>
    public class TextBlockEx : TextBlock
    {
        /// <summary>
        /// The ThreadBarrier's captured SynchronizationContext
        /// </summary>
        private readonly SynchronizationContext _synchronizationContext;

        #region IsTrimmingToolTip
        public static readonly DependencyProperty IsTrimmingToolTipProperty =
            DependencyProperty.Register("IsTrimmingToolTip", typeof(Boolean), typeof(TextBlockEx), new PropertyMetadata(false));

        /// <summary>
        /// TextTrimming 속성이 CharacterEllipsis 일 때 툴팁 표시 여부
        /// </summary>
        public Boolean IsTrimmingToolTip
        {
            get { return (Boolean)this.GetValue(IsTrimmingToolTipProperty); }
            set
            {
                if (this._synchronizationContext != null)
                    this._synchronizationContext.Send(delegate { this.SetValue(IsTrimmingToolTipProperty, value); }, null);
                else
                    this.SetValue(IsTrimmingToolTipProperty, value);
            }
        }
        #endregion //IsTrimmingToolTip


        public TextBlockEx()
            : base()
        {
            this._synchronizationContext = AsyncOperationManager.SynchronizationContext;
        }
        static TextBlockEx()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlockEx), new FrameworkPropertyMetadata(typeof(TextBlockEx)));
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(TextBlockEx), new FrameworkPropertyMetadata { DefaultValue = 20 });
        }


        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            if (TextTrimming != TextTrimming.None && ToolTip != null)
                e.Handled = !IsTextTrimmed();
        }

        private bool IsTextTrimmed()
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, Foreground);
            return formattedText.Width > ActualWidth;
        }
    }
    public class PopupTextBlockHeader : TextBlockEx { }

    public class TextBlockMainHeader : TextBlock { }
    public class TextBlockPopupLabel : TextBlock { }

    #region Paging
    public class PagingTextBlock : TextBlockEx { }
    public class PagingCountTextBlock : TextBlockEx { }
    #endregion //Paging
    #endregion //TextBlock

    #region TextBox
    public class PopupTextBox : TextBox { }
    public class NumericTextBox : TextBox
    {
        private bool IsControlV = false;

        #region DependencyProperty
        public static DependencyProperty MinProperty;
        public static DependencyProperty MaxProperty;
        #endregion //DependencyProperty

        #region Property
        public int? Min
        {
            get { return (int?)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public int? Max
        {
            get { return (int?)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        /// <summary>
        /// TextBox Text 최초 설정시 값을 Backup 한다.
        /// </summary>
        public string OrignalValue { get; private set; }
        #endregion //Property


        static NumericTextBox()
        {
            Type type = typeof(NumericTextBox);
            MinProperty = DependencyProperty.Register(nameof(Min), typeof(int?), type, new PropertyMetadata(null));
            MaxProperty = DependencyProperty.Register(nameof(Max), typeof(int?), type, new PropertyMetadata(null));
        }
        public NumericTextBox()
            : base()
        {
            //한글 사용 불가
            InputMethod.SetIsInputMethodEnabled(this, false);
        }


        #region Override
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
            else if (ModifierKeys.Shift == Keyboard.Modifiers && e.Key == Key.Tab)
            {
                e.Handled = true;
                KeysEnter.KeyEvent(this, Key.Tab);
            }
            //else if (e.Key == Key.Delete || e.Key == Key.Back) { }
            //else if (!DataValidation.IsIPv4Key(e.Key))
            //    e.Handled = true;

            // 텍스트 박스에 붙여넣기가 가능.
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                IsControlV = true;

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            char c = e.Text.ToCharArray().First();
            // 한글 및 특수문자 입력 제한
            if (DataValidation.IsValidHangul(c) || DataValidation.IsValidSpecialLetters(c))
                e.Handled = true;

            if (!int.TryParse(e.Text.Trim(), out int _))
            {
                e.Handled = true;
                if (e.Text == "\r")
                {
                    e.Handled = true;
                    KeysEnter.KeyEvent(this, Key.Tab);
                }
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            bool bReturn = true;
            if (string.IsNullOrEmpty(OrignalValue))
                OrignalValue = (string.IsNullOrEmpty(Text)) ? string.Empty : Text;

            int? value;
            if (IsControlV)
            {
                if (Text == String.Empty) { }
                else
                {
                    if (!DataValidation.IsValidRegexMatch(1, Text))
                    {
                        //Text = string.Empty;
                        //e.Handled = true;
                        bReturn = false;
                        ////return;
                    }
                    else
                    {
                        if (Min != null || Max != null)
                        {
                            value = Convert.ToInt32(Text.ToString());
                            if (value == null) { }
                            else
                            {
                                if ((Min != 0 && Min > value) || (Max != 0 && value > Max))
                                    bReturn = false;
                            }
                        }
                    }
                }
                IsControlV = false;
                if (!bReturn)
                {
                    ShowErrorMessgae();
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Text)) { }
                else
                {
                    if (Min != null || Max != null)
                    {
                        value = Convert.ToInt32(Text.ToString());
                        if (value == null)
                            return;

                        //if ((Min != 0 && Min > value) || (Max != 0 && value > Max))
                        if (Max != 0 && value > Max)
                        {
                            ShowErrorMessgae();
                            return;
                        }
                    }
                }
            }
            base.OnTextChanged(e);
        }

        /// <summary>
        /// 범위 값이 유효하지 않을 때 Message를 표시하고 _OrignalValue 값으로 되돌린다.
        /// </summary>
        private void ShowErrorMessgae()
        {
            //Text: "{0}" 값이 유효하지 않습니다.\n값에 대한 유효범위를 확인하시기 바랍니다.  //Text: 확인
            //MessageBox.Show("M900017", ConvertLanguage.GetCodeText("T23003"), MessageBoxButton.OK, MessageBoxImage.Warning, Text);
            Text = OrignalValue;
            Focus();
            SelectAll();
        }
        #endregion //Override
    }
    /// <summary>
    /// Watermark TextBox
    /// </summary>
    [TemplatePart(Name = PART_WaterMark, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    public class WaterMarkTextBox : TextBox
    {
        private const string PART_WaterMark = "PART_WaterMark";
        private const string PART_TextBox = "PART_TextBox";
        private const string PART_Remove = "PART_Remove";
        private TextBlock _WaterMarkTextBlock;
        private TextBox _TextBox;
        private Button _Remove;

        #region DependencyProperty
        public static DependencyProperty WaterMarkProperty = DependencyProperty.Register(nameof(WaterMark), typeof(String), typeof(WaterMarkTextBox), new PropertyMetadata(null));
        public static DependencyProperty IsEnabledRemoveProperty = DependencyProperty.Register(nameof(IsEnabledRemove), typeof(Boolean), typeof(WaterMarkTextBox), new PropertyMetadata(true));
        #endregion //DependencyProperty

        #region Property

        #region WaterMark
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String WaterMark
        {
            get { return (String)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        #endregion //WaterMark

        #region IsEnabledRemove
        /// <summary>
        /// 텍스트 삭제 버튼 표시 여부
        /// </summary>
        public Boolean IsEnabledRemove
        {
            get { return (Boolean)GetValue(IsEnabledRemoveProperty); }
            set { SetValue(IsEnabledRemoveProperty, value); }
        }
        #endregion //IsEnabledRemove

        #endregion //Property


        public WaterMarkTextBox()
            : base()
        {
        }


        #region Override
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _WaterMarkTextBlock = GetTemplateChild(PART_WaterMark) as TextBlock;
            _TextBox = GetTemplateChild(PART_TextBox) as TextBox;
            if (GetTemplateChild(PART_Remove) is Button btn)
            {
                _Remove = btn;
                _Remove.Click += (s, e) => { this.Text = string.Empty; };
            }
        }
        #endregion //Override
    }
    #endregion //TextBox

    #region Toggle
    public class EyeToggleButton : ToggleButton { }
    #endregion //Toggle
}
