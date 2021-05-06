using System;
using System.Windows;
using System.Windows.Controls;

using Common.Models;

namespace Common.Views
{
    public interface IViewsListProvider
    {
        Type GetViewType(Type moduleType);
    }

    public interface IModuleView : IControlView
    {
        void SetViewIsReadyToAppear(bool v);
    }

    public interface IViewsManager
    {
        void CreateView(IModule module);
        void ShowView(IModule module);
        IModule GetModule(object view);
    }


    public class ViewsManager : IViewsManager
    {
        readonly IViewsListProvider viewListProvider;
        public ViewsManager(IViewsListProvider viewListProvider)
        {
            this.viewListProvider = viewListProvider;
        }
        public void CreateView(IModule module)
        {
            FrameworkElement view = (FrameworkElement)module.View;
            if (view == null)
            {
                Type viewType = viewListProvider.GetViewType(module.GetType());
                view = (FrameworkElement)Activator.CreateInstance(viewType);
            }
            view.Opacity = 0.0;
            if (view as IModuleView != null)
                (view as IModuleView).SetViewIsReadyToAppear(false);
            module.SetView(view);
            view.DataContext = module;
        }
        public void ShowView(IModule module)
        {
            FrameworkElement view = (FrameworkElement)module.View;
            IModuleView viewAsIModuleView = view as IModuleView;
            if (viewAsIModuleView != null)
            {
                viewAsIModuleView.BeforeViewDisappear += OnViewBeforeViewDisappear;
                viewAsIModuleView.AfterViewDisappear += OnViewAfterViewDisappear;
                viewAsIModuleView.ViewIsVisibleChanged += OnViewViewIsVisibleChanged;
            }
            view.Opacity = 1.0;
            if (viewAsIModuleView != null)
            {
                viewAsIModuleView.SetViewIsReadyToAppear(true);
            }
        }
        public IModule GetModule(object view)
        {
            return !(view is FrameworkElement viewAsFrameworkElement) ? null : viewAsFrameworkElement.DataContext as IModule;
        }
        void OnViewViewIsVisibleChanged(object sender, EventArgs e)
        {
            FrameworkElement view = (FrameworkElement)sender;
            if (view.DataContext is IModule module && view is IModuleView viewAsIModuleView)
                module.SetIsVisible(viewAsIModuleView.ViewIsVisible);
        }
        void OnViewBeforeViewDisappear(object sender, EventArgs e)
        {
            FrameworkElement view = (FrameworkElement)sender;
            if (view.DataContext is IModule module)
            {
                foreach (IModule submodule in module.GetSubmodules())
                {
                    if (submodule == null) continue;
                    submodule.RaiseBeforeDisappear();
                }
                module.RaiseBeforeDisappear();
            }
        }
        void OnViewAfterViewDisappear(object sender, EventArgs e)
        {
            FrameworkElement view = (FrameworkElement)sender;
            IModule module = view.DataContext as IModule;
            if (module != null && module.IsPersistentModule) return;
            if (view is IModuleView viewAsIModuleView)
            {
                viewAsIModuleView.ViewIsVisibleChanged -= OnViewViewIsVisibleChanged;
                viewAsIModuleView.BeforeViewDisappear -= OnViewBeforeViewDisappear;
                viewAsIModuleView.AfterViewDisappear -= OnViewAfterViewDisappear;
            }
            view.DataContext = null;
            if (module != null)
            {
                foreach (IModule submodule in module.GetSubmodules())
                {
                    if (submodule == null) continue;
                    if (submodule.View is IModuleView subviewAsIModuleView)
                        subviewAsIModuleView.RaiseAfterViewDisappear();
                }
                module.SetView(null);
                module.Dispose();
            }
            if (view.Parent is ContentControl cc)
                cc.Content = null;
            if (view.Parent is ContentPresenter cp)
                cp.Content = null;
        }
    }
}
