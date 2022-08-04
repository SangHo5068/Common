using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

using Common.Command;
using Common.Notify;
using Common.Utilities;

namespace Common.Views
{
    public abstract class BaseWindow : Window
    {
        #region DependencyProperty
        public static DependencyProperty MonitorPositionProperty = 
            DependencyProperty.Register(nameof(MonitorPosition), typeof(int), typeof(BaseWindow), new PropertyMetadata(1));
        #endregion //DependencyProperty

        #region Properties
        public int MonitorPosition
        {
            get => (int)GetValue(MonitorPositionProperty);
            set => SetValue(MonitorPositionProperty, value);
        }

        public Storyboard UnloadedStoryboard { get; private set; }
        public bool IsUnloadedStoryboardBegin { get; private set; } = false;
        #endregion //Properties

        #region ICommand
        public ICommand CommandMouseLeftButtonDown { get; private set; }
        public ICommand CommandMouseLeftDoubleClick { get; private set; }

        public ICommand CommandWindowMin { get; private set; }
        public ICommand CommandWindowMax { get; private set; }
        public ICommand CommandWindowClose { get; private set; }
        #endregion //ICommand


        public BaseWindow()
        {
            CommandMouseLeftButtonDown = new RelayCommand(OnMouseLeftButtonDown);
            CommandMouseLeftDoubleClick = new RelayCommand(OnMouseLeftDoubleClick);

            CommandWindowMin = new RelayCommand(OnWindowMin);
            CommandWindowMax = new RelayCommand(OnWindowMax);
            CommandWindowClose = new RelayCommand(OnWindowClose);

            this.Loaded += BaseWindow_Loaded;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //if (TryFindResource("UnloadedWindow") is Storyboard storyboard)
            if (Resources["UnloadedWindow"] is Storyboard storyboard)
                UnloadedStoryboard = storyboard;
        }


        #region WindowAction

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.Content != null && this.DataContext != null)
            {
                (this.DataContext as BindableAndDisposable)?.Dispose();
            }

            if (UnloadedStoryboard != null && !IsUnloadedStoryboardBegin)
            {
                e.Cancel = true;
                IsUnloadedStoryboardBegin = true;
            }

            //팝업창 닫은 후 메인화면이 최소화 되지 않도록 Focus
            if (this.Owner != null)
                this.Owner.Focus();
            base.OnClosing(e);

            if (UnloadedStoryboard != null)
            {
                var begin = new BeginStoryboard {
                    Storyboard = UnloadedStoryboard
                };
                begin.Storyboard.Completed += (s, _) => {
                    e.Cancel = false;
                    this.Close();
                };
                begin.Storyboard.Begin(this);
                //var animation = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.5));
                //this.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }
        private void OnWindowClose(object obj)
        {
            this.Close();
        }

        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (WindowState == WindowState.Maximized)
            {
                WindowStateHelper.SetWindowMaximized(this);
                WindowStateHelper.BlockStateChange = true;

                //var screen = ScreenFinder.FindAppropriateScreen(this);
                //if (screen != null)
                //{
                //    Top = screen.WorkingArea.Top;
                //    Left = screen.WorkingArea.Left;
                //    Width = screen.WorkingArea.Width;
                //    Height = screen.WorkingArea.Height;
                //}
                ScreenFinder.SetMonitorScreen(this, MonitorPosition);
            }
            else
            {
                if (WindowStateHelper.BlockStateChange)
                {
                    WindowStateHelper.BlockStateChange = false;
                    return;
                }

                WindowStateHelper.UpdateLastKnownNormalSize(Width, Height);
                WindowStateHelper.UpdateLastKnownLocation(Top, Left);
            }
        }
        private void OnWindowMax(object obj)
        {
            if (!WindowStateHelper.IsMaximized)
                WindowState = WindowState.Maximized;
            else
            {
                WindowStateHelper.SetWindowSizeToNormal(this);
                //WindowState = WindowState.Normal;
            }
        }
        private void OnWindowMin(object obj)
        {
            Window_Minimize(this, null);
        }

        private void OnMouseLeftButtonDown(object obj)
        {
            if (!(obj is MouseButtonEventArgs mouseArgs))
                return;

            this.Title_MouseLeftButtonDown(this, mouseArgs);
        }

        protected virtual void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            ////if (WindowState is WindowState.Normal)
            if (!WindowStateHelper.IsMaximized)
                DragMove();

            //if (WindowStateHelper.IsMaximized && e.LeftButton == MouseButtonState.Pressed)
            //{
            //    WindowStateHelper.SetWindowSizeToNormal(this, true);
            //    WindowStateHelper.UpdateLastKnownLocation(Top, Left);
            //}

            //DragMove();
        }

        protected virtual void Window_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Header Click
        /// </summary>
        /// <param name="obj"></param>
        private void OnMouseLeftDoubleClick(object obj)
        {
            if (!(obj is MouseButtonEventArgs e))
                return;

            if (e.ClickCount == 2)
            {
                OnWindowMax(e);
                e.Handled = true;
                return;
            }

            this.Title_MouseLeftButtonDown(this, e);
        }

        #endregion //WindowAction



        #region Resize Custom Window

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;
            base.OnInitialized(e);
        }

        private HwndSource _hwndSource;
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        protected virtual void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }
        protected virtual void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    break;
            }
        }
        protected virtual void ResizeRectangle_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }
        }

        #endregion //Resize Custom Window
    }
}
