using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Common.Models
{
    public interface IModule : IDisposable, ISupportInitialize
    {
        object View { get; }
        void SetView(object v);
        bool IsPersistentModule { get; }
        List<IModule> GetSubmodules();
        //bool IsVisible { get; }
        void SetIsVisible(bool v);
        event EventHandler BeforeDisappear;
        void RaiseBeforeDisappear();
        event EventHandler BeforeAppearAsync;
        void RaiseBeforeAppearAsync();
        event EventHandler BeforeAppear;
        void RaiseBeforeAppear();
        object InitParam { get; set; }
        ModulesManagerInternalData ModulesManagerInternalData { get; set; }
        IModulesManager ModulesManager { get; }
        void SetModulesManager(IModulesManager v);
        IModule Parent { get; }
        IModule Main { get; }
        void SetParent(IModule v);
        void RaiseUpdateDataContext(object parameter);
    }
}
