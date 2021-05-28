using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

using Common.Command;
using Common.Notify;
using Common.Utilities;

namespace Common.Views
{
    public abstract class BaseWindow : Window
    {
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
        }


        #region WindowAction

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.Content != null && this.DataContext != null)
            {
                (this.DataContext as BindableAndDisposable).Dispose();
            }

            //팝업창 닫은 후 메인화면이 최소화 되지 않도록 Focus
            if (this.Owner != null)
            {
                this.Owner.Focus();
                base.OnClosing(e);
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

                var screen = ScreenFinder.FindAppropriateScreen(this);
                if (screen != null)
                {
                    Top = screen.WorkingArea.Top;
                    Left = screen.WorkingArea.Left;
                    Width = screen.WorkingArea.Width;
                    Height = screen.WorkingArea.Height;
                }
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

        private HwndSource _hwndSource;
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }
        protected void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
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
        protected void ResizeRectangle_PreviewMouseDown(object sender, MouseEventArgs e)
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

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;
            base.OnInitialized(e);
        }

        #endregion //Resize Custom Window
    }
}
