using System;

namespace Common.Base
{
    public interface IControlView
    {
        bool ViewIsReadyToAppear { get; }
        bool ViewIsVisible { get; }
        event EventHandler ViewIsReadyToAppearChanged;
        event EventHandler ViewIsVisibleChanged;
        event EventHandler BeforeViewDisappear;
        event EventHandler AfterViewDisappear;
        void SetViewIsVisible(bool v);
        void RaiseBeforeViewDisappear();
        void RaiseAfterViewDisappear();
    }
}
