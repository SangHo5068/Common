using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Utilities
{
    public static class KeysEnter
    {
        static KeysEnter() { }

        public static void KeyEvent(Control _Ctrl, Key _eKey)
        {
            KeyEventArgs eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, _eKey);
            eInsertBack.RoutedEvent = UIElement.KeyDownEvent;
            InputManager.Current.ProcessInput(eInsertBack);
        }

        public static void FocusMove(FocusNavigationDirection pFocusNavigation)
        {
            TraversalRequest tRequest = new TraversalRequest(pFocusNavigation);
            UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
            if (keyboardFocus != null)
                keyboardFocus.MoveFocus(tRequest);
        }
    }
}
