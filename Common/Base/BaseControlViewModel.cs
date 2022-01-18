using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Common.Command;
using Common.Container.Events;
using Common.Models;
using Common.Notify;

namespace Common.Base
{
    /// <summary>
    /// DataContext Change Event Argument
    /// </summary>
    public class BaseEventArgs : EventArgs
    {
        public object Source { get; set; }
        public BaseEventArgs(object source)
            : base()
        {
            this.Source = source;
        }
    }

    public class BaseControlViewModel : BindableAndDisposable, IModule
    {
        #region Define

        #region Private
        object _View;
        IModule _Parent;
        IModule _Main;
        private string _Header = string.Empty;
        //bool _IsVisible;
        //bool _CanFloat = true;
        //bool _CanClose = true;
        //bool _IsSelected = false;
        bool _IsActive = false;
        bool _IsOpened;
        bool _IsClosed = false;
        bool _ShowTabHeader = true;
        bool _ShowTabHeaderPin = true;
        bool _ShowTabHeaderClose = true;
        string _DockingGroup;
        string title;
        string subTitle;
        object initParam;
        ICommand _CloseCommand;
        ICommand _FloatCommand;
        ModulesManagerInternalData internalData;
        #endregion //Private

        public bool IsPersistentModule { get; protected set; }
        //public IModulesManager ModulesManager { get; private set; }
        public IModulesManager ModulesManager { get; protected set; }

        public event EventHandler BeforeDisappear;
        public event EventHandler BeforeAppearAsync;
        public event EventHandler BeforeAppear;
        public event EventHandler<BaseEventArgs> UpdateDataContext;
        public event EventHandler RequestClose;
        public event EventHandler RequestFloat;
        public event EventHandler RequestActive;

        /// <summary>
        /// Content
        /// </summary>
        public object View
        {
            get { return _View; }
            private set { SetValue(ref _View, value); }
        }
        public IModule Parent
        {
            get { return _Parent; }
            private set { SetValue(ref _Parent, value); }
        }
        public IModule Main
        {
            get { return _Main; }
            private set { SetValue(ref _Main, value); }
        }
        public virtual string Header
        {
            get { return _Header; }
            set { SetValue(ref _Header, value); }
        }

        /// <summary>
        /// 이벤트 관리
        /// </summary>
        public virtual EventAggregator EventAggregator { get { return (EventAggregator)_eventAggregator; } }

        /// <summary>
        /// 검색 명령
        /// </summary>
        public virtual ICommand SearchCommand { get; set; }

        #region Dock
        //public bool IsVisible
        //{
        //    get { return _IsVisible; }
        //    set { SetValue("IsVisible", ref _IsVisible, value); }
        //    //private set { SetValue("IsVisible", ref isVisible, value); }
        //}
        //public bool CanFloat
        //{
        //    get { return _CanFloat; }
        //    set { SetValue("CanFloat", ref _CanFloat, value); }
        //}
        //public bool CanClose
        //{
        //    get { return _CanClose; }
        //    set { SetValue("CanClose", ref _CanClose, value); }
        //}
        //public bool IsSelected
        //{
        //    get { return _IsSelected; }
        //    set { SetValue("IsSelected", ref _IsSelected, value); }
        //}
        public bool IsActive
        {
            get { return _IsActive; }
            set { SetValue(ref _IsActive, value, OnActiveChanged); }
        }
        public bool IsOpened
        {
            get { return _IsOpened; }
            set { SetValue(ref _IsOpened, value); }
        }
        public bool IsClosed
        {
            get { return _IsClosed; }
            set { SetValue(ref _IsClosed, value, OnIsClosedChanged); }
        }
        /// <summary>
        /// Dock Header
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetValue(ref title, value); }
        }
        /// <summary>
        /// Dock Header SubTitle
        /// </summary>
        public string SubTitle
        {
            get { return subTitle; }
            set { SetValue(ref subTitle, value); }
        }
        /// <summary>
        /// 단일 탭으로 올라가 있는지 여부(True:단일탭 있음)
        /// </summary>
        [DefaultValue(false)]
        public Boolean IsTabTrue { get; set; }

        #region DockLayoutManager 관련 속성
        /// <summary>
        /// Text: 닫기
        /// </summary>
        public String LanguageClose
        {
            get { return "닫기"; }
        }
        /// <summary>
        /// Text: 팝업으로 전환
        /// </summary>
        public String LanguageShowWindow
        {
            get { return "팝업으로 전환"; }
        }

        public bool ShowTabHeader
        {
            get { return _ShowTabHeader; }
            set { SetValue(ref _ShowTabHeader, value); }
        }
        public bool ShowTabHeaderClose
        {
            get { return _ShowTabHeaderClose; }
            set { SetValue(ref _ShowTabHeaderClose, value); }
        }
        public bool ShowTabHeaderPin
        {
            get { return _ShowTabHeaderPin; }
            set { SetValue(ref _ShowTabHeaderPin, value); }
        }
        /// <summary>
        /// DocumentGroup, LayoutGroup 에 Dock 할 Panel Group 이름
        /// </summary>
        public string DockingGroup
        {
            get { return _DockingGroup; }
            set { SetValue(ref _DockingGroup, value); }
        }

        /// <summary>
        /// Dock Item Close Command
        /// </summary>
        public virtual ICommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                    //_closeCommand = DelegateCommandFactory.Create<object>(OnRequestClose);
                    _CloseCommand = new RelayCommand(OnRequestClose);
                return _CloseCommand;
            }
            set { _CloseCommand = value; }
        }
        public virtual ICommand FloatCommand
        {
            get
            {
                if (_FloatCommand == null)
                    _FloatCommand = new RelayCommand(OnRequestFloat);
                return _FloatCommand;
            }
            set { _FloatCommand = value; }
        }

        protected virtual void OnRequestClose(object param)
        {
            this.RequestClose?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnRequestFloat(object param)
        {
            this.RequestFloat?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnIsClosedChanged(bool oldValue, bool newValue)
        {
            IsOpened = !newValue;
        }
        protected virtual void OnActiveChanged(bool oldValue, bool newValue)
        {
            if (newValue)
                this.RequestActive?.Invoke(this, EventArgs.Empty);
        }
        #endregion //DockLayoutManager 관련 속성

        #endregion //Dock

        #endregion //Define


        private DelegateLoadedAction _loadAction = null;
        public virtual DelegateLoadedAction LoadAction
        {
            get
            {
                return _loadAction ?? (_loadAction = new DelegateLoadedAction((s) => {
                    if (!(s is FrameworkElement view))
                        return;
                    _View = _View ?? view;
                    if (view.Visibility == Visibility.Collapsed)
                        return;

                    /// do your window initialization here
                    InitData(internalData);
                }));
            }
        }
        public virtual List<IModule> GetSubmodules()
        {
            return new List<IModule>();
        }
        public virtual void BeginInit() { }
        public virtual void EndInit() { }
        protected virtual void LoadData(object parameter) { }
        public virtual void ReLoadData(object parameter) { }
        protected virtual void InitData(object parameter) { }
        protected virtual void SaveData() { }
        protected override void DisposeManaged()
        {
            if (View != null) (View as BaseUserControl)?.Dispose();
            View = null;
            Main = null;
            Parent = null;
            Header = null;

            if (CloseCommand != null) CloseCommand = null;
            if (FloatCommand != null) FloatCommand = null;
            if (SearchCommand != null) SearchCommand = null;

            if (BeforeDisappear != null) BeforeDisappear = null;
            if (BeforeAppearAsync != null) BeforeAppearAsync = null;
            if (BeforeAppear != null) BeforeAppear = null;
            if (UpdateDataContext != null) UpdateDataContext = null;
            if (RequestClose != null) RequestClose = null;
            if (RequestFloat != null) RequestFloat = null;
            if (RequestActive != null) RequestActive = null;

            base.DisposeManaged();
        }
        //protected virtual void SetLanguage() { }
        //protected virtual void SetSearchOptions(object parameter) { }

        void IModule.SetView(object v)
        {
            View = v;
        }
        void IModule.SetIsVisible(bool v)
        {
            //IsVisible = v;
        }
        void IModule.RaiseBeforeDisappear()
        {
            SaveData();
            BeforeDisappear?.Invoke(this, EventArgs.Empty);
        }
        object IModule.InitParam
        {
            get { return initParam; }
            set { initParam = value; }
        }
        void IModule.RaiseBeforeAppearAsync()
        {
            LoadData(initParam);
            BeforeAppearAsync?.Invoke(this, EventArgs.Empty);
        }
        void IModule.RaiseBeforeAppear()
        {
            InitData(initParam);
            BeforeAppear?.Invoke(this, EventArgs.Empty);
        }
        void IModule.SetModulesManager(IModulesManager v)
        {
            ModulesManager = v;
        }
        ModulesManagerInternalData IModule.ModulesManagerInternalData
        {
            get { return internalData; }
            set { internalData = value; }
        }
        void IModule.SetParent(IModule v)
        {
            Parent = v;
            Main = (v == null) ? this : v.Main;
        }
        void IModule.RaiseUpdateDataContext(object parameter)
        {
            UpdateDataContext?.Invoke(this, new BaseEventArgs(parameter));
        }
    }
}
