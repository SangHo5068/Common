using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

using Common.Controls;
using Common.Views;

namespace Common.Base
{
    public class BaseUserControls : System.Windows.Controls.UserControl, IControlView, IModuleView, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// The ThreadBarrier's captured SynchronizationContext
        /// </summary>
        private SynchronizationContext _synchronizationContext;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ViewIsReadyToAppearChanged;
        public event EventHandler ViewIsVisibleChanged;
        public event EventHandler BeforeViewDisappear;
        public event EventHandler AfterViewDisappear;

        #region Dependency Properties
        public static readonly DependencyProperty ViewIsReadyToAppearProperty;
        public static readonly DependencyProperty ViewIsVisibleProperty;

        static void RaiseViewIsReadyToAppearChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseUserControls)d).RaiseViewIsReadyToAppearChanged(e);
        }
        static void RaiseViewIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseUserControls)d).RaiseViewIsVisibleChanged(e);
        }
        #endregion //Dependency

        #region <Properties>

        public System.Windows.Controls.UserControl UserControl
        {
            get { return this; }
        }

        public bool ViewIsReadyToAppear
        {
            get { return (bool)GetValue(ViewIsReadyToAppearProperty); }
            private set
            {
                if (this._synchronizationContext != null)
                    this._synchronizationContext.Send(delegate { SetValue(ViewIsReadyToAppearProperty, value); }, null);
                else
                    SetValue(ViewIsReadyToAppearProperty, value);
            }
        }
        public bool ViewIsVisible
        {
            get { return (bool)GetValue(ViewIsVisibleProperty); }
            private set
            {
                if (this._synchronizationContext != null)
                    this._synchronizationContext.Send(delegate { SetValue(ViewIsVisibleProperty, value); }, null);
                else
                    SetValue(ViewIsVisibleProperty, value);
            }
        }

        #endregion //Properties


        public BaseUserControls() : base()
        {
            this._synchronizationContext = AsyncOperationManager.SynchronizationContext;
        }

        static BaseUserControls()
        {
            Type ownerType = typeof(BaseUserControls);

            ViewIsReadyToAppearProperty = DependencyProperty.Register("ViewIsReadyToAppear", typeof(bool), ownerType, new PropertyMetadata(false, RaiseViewIsReadyToAppearChanged));
            ViewIsVisibleProperty = DependencyProperty.Register("ViewIsVisible", typeof(bool), ownerType, new PropertyMetadata(false, RaiseViewIsVisibleChanged));
        }

        public virtual void Dispose()
        {
            if (this._synchronizationContext != null) this._synchronizationContext = null;
            for (int i = 0; i < this.VisualChildrenCount; i++)
            {
                if (!(this.GetVisualChild(i) is System.Windows.Controls.Border el)) continue;

                if (!(el.Child is System.Windows.Controls.ContentPresenter cp)) continue;

                if (!(cp.Content is System.Windows.Controls.Grid gd)) continue;
                foreach (var item in gd.Children)
                {
                    if (item.GetType() != typeof(ChildWindow)) continue;
                    if (item is ChildWindow childwindow)
                        childwindow.Dispose();
                }
            }
        }


        #region <Events>
        void RaiseViewIsReadyToAppearChanged(DependencyPropertyChangedEventArgs e)
        {
            ViewIsReadyToAppearChanged?.Invoke(this, EventArgs.Empty);
        }
        void RaiseViewIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            ViewIsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
        public void OnPropertyChanged(String Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }
        #endregion //Events


        #region <Interface>
        void IControlView.SetViewIsVisible(bool v)
        {
            ViewIsVisible = v;
        }
        void IModuleView.SetViewIsReadyToAppear(bool v)
        {
            ViewIsReadyToAppear = v;
        }
        void IControlView.RaiseBeforeViewDisappear()
        {
            BeforeViewDisappear?.Invoke(this, EventArgs.Empty);
        }
        void IControlView.RaiseAfterViewDisappear()
        {
            AfterViewDisappear?.Invoke(this, EventArgs.Empty);
        }
        #endregion //Interface
    }
}
