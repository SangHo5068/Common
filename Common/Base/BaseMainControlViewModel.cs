using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using Common.Command;
using Common.Models;
using Common.Utilities;

namespace Common.Base
{
    public abstract class BaseMainControlViewModel : BaseControlViewModel
    {
        IModule currentModule;
        Type currentModuleType;
        ObservableCollection<BaseControlViewModel> modelCollection = new ObservableCollection<BaseControlViewModel>();

        public EventHandler OnEventFloat;
        public EventHandler OnEventClose;



        public BaseMainControlViewModel()
        {
            IsPersistentModule = true;
            modelCollection.CollectionChanged += ModelCollection_CollectionChanged;
        }


        void ModelCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (BaseControlViewModel model in e.NewItems)
                {
                    model.RequestClose += this.OnModelRequestClose;
                    model.RequestFloat += this.OnModelRequestFloat;
                }
            }
            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (BaseControlViewModel model in e.OldItems)
                {
                    model.RequestClose -= this.OnModelRequestClose;
                    model.RequestFloat -= this.OnModelRequestFloat;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BaseControlViewModel item in e.OldItems)
                {
                    RaiseBeforeViewDisappear(item.View);
                    RaiseAfterViewDisappear(item.View);
                    item.Dispose();
                }
                NotifyPropertyChanged("ModelCollection");
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (BaseControlViewModel item in e.NewItems)
                {
                    NotifyPropertyChanged("ModelCollection");
                }
            }
        }

        void OnModelRequestClose(object sender, EventArgs e)
        {
            try
            {
                if (sender is BaseControlViewModel model)
                {
                    model.Dispose();
                    if (this.ModelCollection != null && this.ModelCollection.Contains(model))
                        this.ModelCollection.Remove(model);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            finally
            {
                if (this.OnEventClose != null)
                    OnEventClose(sender, e);
            }
        }

        void OnModelRequestFloat(object sender, EventArgs e)
        {
            this.OnEventFloat?.Invoke(sender, e);
        }

        public override void EndInit()
        {
            base.EndInit();
            CurrentModuleType = GetType();
            CurrentModule = this;
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            foreach (var model in ModelCollection)
                model.Dispose();
            ModelCollection.CollectionChanged -= ModelCollection_CollectionChanged;
            ModelCollection.Clear();
            ModelCollection = null;

            if (OnEventFloat != null) OnEventFloat = null;
            if (OnEventClose != null) OnEventClose = null;
        }

        public void ShowModule<TModule>(object parameter) where TModule : class, IModule, new()
        {
            CurrentModuleType = typeof(TModule);
            CurrentModule = ModulesManager.CreateModule<TModule>(null, this, parameter);
            if (!ModelCollection.Contains(CurrentModule))
                ModelCollection.Add((BaseControlViewModel)CurrentModule);
        }
        public void ShowModule(Type moduleType, object parameter)
        {
            if (moduleType == null) return;
            CurrentModuleType = moduleType;
            CurrentModule = ModulesManager.CreateModule(moduleType, null, this, parameter);
        }

        public Type CurrentModuleType
        {
            get { return currentModuleType; }
            set { SetValue(ref currentModuleType, value); }
        }
        public IModule CurrentModule
        {
            get { return currentModule; }
            set { SetValue(ref currentModule, value); }
        }
        public ObservableCollection<BaseControlViewModel> ModelCollection
        {
            get { return modelCollection; }
            set { SetValue(ref modelCollection, value); }
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

        protected virtual ICommand CreateShowModuleCommand<T>() where T : class, IModule, new()
        {
            //return new ExtendedActionCommand(p => ShowModule<T>(p), this, "CurrentModuleType", x => CurrentModuleType != typeof(T), null);
            //TODO:ModelCollection 속성 변경여부 적용안되고 있음 추후수정
            return new ExtendedActionCommand(p => ShowModule<T>(p), this, "ModelCollection", x => ModelCollection.OfType<T>().Count() == 0, null);
        }
        protected virtual ICommand CreateShowModuleCommand<T>(object param) where T : class, IModule, new()
        {
            return new ExtendedActionCommand(p => ShowModule<T>(p), this, "ModelCollection", x => ModelCollection.OfType<T>().Count() == 0, param);
        }
        //protected virtual ICommand CreateShowModuleCommand()
        //{
        //    return new ExtendedActionCommand(t => ShowModule(t as Type, null), this, "CurrentModuleType", t => CurrentModuleType != t as Type, null);
        //}
        //protected virtual ICommand CreateShowNavigationParentCommand()
        //{
        //    return new ExtendedActionCommand(p => ShowNavigationParent(p as IModule, null), this, "CurrentModuleType", x => CurrentModuleType != GetType(), null);
        //}
    }
}
