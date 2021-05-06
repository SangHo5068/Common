using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using Common.Base;

namespace Common.Views
{
    [ContentProperty(nameof(Content))]
    public class ViewPresenter : Control
    {
        #region Fileds
        //Grid grid; 
        ContentPresenter root;
        #endregion //Fileds

        #region Dependency Properties
        public static readonly DependencyProperty ContentProperty;
        static void RaiseContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ViewPresenter)d).RaiseContentChanged(e.OldValue, e.NewValue);
        }
        void RaiseContentChanged(object oldValue, object newValue)
        {
            if (oldValue is IControlView OldValue)
            {
                RaiseBeforeViewDisappear(OldValue);
                RaiseAfterViewDisappear(OldValue);
            }

            //IControlView NewValue = newValue as IControlView;
            if (this.root != null)
                this.root.Content = newValue;
        }
        #endregion //Dependency Properties

        #region Properties
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        #endregion //Properties



        static ViewPresenter()
        {
            Type ownerType = typeof(ViewPresenter);
            ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), ownerType, new PropertyMetadata(null, RaiseContentChanged));
        }
        public ViewPresenter()
        {
            this.DefaultStyleKey = typeof(ViewPresenter);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = (ContentPresenter)GetTemplateChild("Root");
            BuildVisualTree();
        }

        protected virtual void SubscribeToViewIsReadyToAppearChanged(object view, EventHandler handler)
        {
            if (view is IControlView v)
                v.ViewIsReadyToAppearChanged += handler;
        }
        protected virtual void UnsubscribeFromViewIsReadyToAppearChanged(object view, EventHandler handler)
        {
            if (view is IControlView v)
                v.ViewIsReadyToAppearChanged -= handler;
        }
        protected virtual bool ViewIsReadyToAppear(object view)
        {
            return !(view is IControlView v) ? true : v.ViewIsReadyToAppear;
        }
        protected virtual void SetViewIsVisible(object view, bool value)
        {
            if (view is IControlView v)
                v.SetViewIsVisible(value);
        }
        protected virtual void RaiseBeforeViewDisappear(object view)
        {
            if (view is IControlView v)
                v.RaiseBeforeViewDisappear();
        }
        protected virtual void RaiseAfterViewDisappear(object view)
        {
            if (view is IControlView v)
                v.RaiseAfterViewDisappear();
        }
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            BuildVisualTree();
        }
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ClearVisualTree();
        }

        void BuildVisualTree()
        {
            //if (this.grid == null)
            //{
            //    this.grid = new Grid();
            //    this.root.Content = this.grid;
            //}
            //if (this.grid.Children.Count == 0)
            //{
            //    if (OldContent != null)
            //        this.grid.Children.Add(OldContent);
            //    if (NewContent != null)
            //        this.grid.Children.Add(NewContent);
            //}
        }
        void ClearVisualTree()
        {
            if (this.root != null)
                this.root.Content = null;
            //if (this.grid != null)
            //    this.grid.Children.Clear();
            //this.grid = null;
        }
    }
}
