using System;
using System.Windows;

namespace Common.Notify
{
    public interface ILoadedAction
    {
        void Loaded<T>(object o);
    }

    public class DelegateLoadedAction : ILoadedAction
    {
        public Action<object> LoadedActionDelegate { get; set; }

        public DelegateLoadedAction() { }
        public DelegateLoadedAction(Action<object> action)
        {
            LoadedActionDelegate = action;
        }

        public void Loaded<T>(object o)
        {
            LoadedActionDelegate?.Invoke(o);
        }
    }

    public class LoadedBindings
    {
        public static readonly DependencyProperty LoadedEnabledProperty =
            DependencyProperty.RegisterAttached(
                "LoadedEnabled",
                typeof(bool),
                typeof(LoadedBindings),
                new PropertyMetadata(false, new PropertyChangedCallback(OnLoadedEnabledPropertyChanged)));

        public static bool GetLoadedEnabled(DependencyObject sender)
        {
            return (bool)sender.GetValue(LoadedEnabledProperty);
        }

        public static void SetLoadedEnabled(DependencyObject sender, bool value)
        {
            sender.SetValue(LoadedEnabledProperty, value);
        }

        private static void OnLoadedEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                bool newEnabled = (bool)e.NewValue;
                bool oldEnabled = (bool)e.OldValue;

                if (oldEnabled && !newEnabled)
                    fe.Loaded -= OnLoaded;
                else if (!oldEnabled && newEnabled)
                    fe.Loaded += OnLoaded;
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var loadedAction = GetLoadedAction((FrameworkElement)sender);
            if (sender is FrameworkElement element && element.Visibility == Visibility.Visible)
                loadedAction?.Loaded<FrameworkElement>(element);
        }


        public static readonly DependencyProperty LoadedActionProperty =
            DependencyProperty.RegisterAttached(
                "LoadedAction",
                typeof(ILoadedAction),
                typeof(LoadedBindings),
                new PropertyMetadata(null));

        public static ILoadedAction GetLoadedAction(DependencyObject sender)
        {
            return (ILoadedAction)sender.GetValue(LoadedActionProperty);
        }

        public static void SetLoadedAction(DependencyObject sender, ILoadedAction value)
        {
            sender.SetValue(LoadedActionProperty, value);
        }
    }
}
