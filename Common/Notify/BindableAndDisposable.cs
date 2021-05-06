using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;

using Common.Container;
using Common.Container.Events;
using Common.Utilities;

using Json = System.Text.Json.Serialization;

namespace Common.Notify
{
    /// <summary>
    /// PropertyChanged Delegate
    /// SetValue 사용 시 전달 된 Action 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public delegate void RaisePropertyChangedDelegate<T>(T oldValue, T newValue);
    public delegate void RaisePropertyChangedDelegateWithSender<T>(object sender, T oldValue, T newValue);

    /// <summary>
    /// Dispose 추상객체
    /// </summary>
    [Serializable]
    public abstract class Disposable : INotifyPropertyChanged, IDisposable
    {
        bool disposed = false;
        [XmlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [Json.JsonIgnore]
        public bool Disposed { get { return disposed; } protected set { disposed = value; } }
        /// <summary>
        /// 객체 Dispose 이후 이벤트
        /// </summary>
        public event EventHandler AfterDispose;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 내부 사용을 위한 EventAggregator 필드. 직접 사용 보다 EventAggregator property 이용할 것.
        /// </summary>
        protected IEventAggregator _eventAggregator;


        public Disposable()
        {
            InitializeCommands();
            InitEventAggregator();
        }
        ~Disposable()
        {
            this.Dispose();
        }

        public virtual void Dispose()
        {
            this.DoDispose(true);
            GC.SuppressFinalize(this);
        }


        private void InitEventAggregator()
        {
            if (this._eventAggregator != null)
                return;

            try
            {
                // TODO : 이하 구문 실패 시 예외 발생 처리 필요.
                var container = ContainerResolver.GetContainer();
                this._eventAggregator = container.Resolve<IEventAggregator>();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        /// <summary>
        /// Command 초기화
        /// </summary>
        protected virtual void InitializeCommands() { }
        /// <summary>
        /// 객체 Dispose
        /// </summary>
        protected virtual void DisposeManaged() { }
        protected virtual void DisposeUnmanaged() { }
        protected virtual void DoDispose(bool disposing)
        {
            if (Disposed) return;
            disposed = disposing;
            if (disposed)
                DisposeManaged();
            DisposeUnmanaged();
            RaiseAfterDispose();

            (_eventAggregator as ComponentContainer)?.Release();
            _eventAggregator = null;
        }
        void RaiseAfterDispose()
        {
            AfterDispose?.Invoke(this, EventArgs.Empty);
            AfterDispose = null;
        }
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// ViewModel 사용시 PropertyChanged 를 간편하게 사용 할 수 있도록 해주는 추상객체
    /// </summary>
    [Serializable]
    public abstract class BindableAndDisposable : Disposable
    {
        /// <summary>
        /// The ThreadBarrier's captured SynchronizationContext
        /// </summary>
        private readonly SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;

        bool disposeSignal;
        [XmlIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [Json.JsonIgnore]
        public bool DisposeSignal
        {
            get { return disposeSignal; }
            private set { SetValue(ref disposeSignal, value); }
        }
        /// <summary>
        /// 속성값 Binding 설정
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        protected void SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            SetValue<T>(ref field, newValue, false, null, propertyName);
        }
        /// <summary>
        /// 속성값 Binding 설정(PropertyChanged Delegate 전달)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="raiseChangedDelegate">PropertyChanged Delegate</param>
        /// <param name="propertyName"></param>
        protected void SetValue<T>(ref T field, T newValue, RaisePropertyChangedDelegate<T> raiseChangedDelegate, [CallerMemberName] string propertyName = "")
        {
            SetValue<T>(ref field, newValue, false, raiseChangedDelegate, propertyName);
        }
        /// <summary>
        /// 속성값 Binding 설정(동일값 PropertyChanged 발생유무)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="disposeOldValue">oldValue Dispose 여부</param>
        /// <param name="isEquals">oldValue = newValue 동일한 값 PropertyChanged 이벤트 발생 여부</param>
        /// <param name="raiseChangedDelegate">PropertyChanged Delegate</param>
        /// <param name="propertyName"></param>
        protected void SetValue<T>(ref T field, T newValue, bool disposeOldValue, bool isEquals, RaisePropertyChangedDelegate<T> raiseChangedDelegate = null, [CallerMemberName] string propertyName = "")
        {
            SetValue<T>(ref field, newValue, disposeOldValue, raiseChangedDelegate, isEquals, propertyName);
        }

        /// <summary>
        /// 현재값과 발생값이 동일한 경우 Changed Event 가 발생하지 않는다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="disposeOldValue">oldValue Dispose 여부</param>
        /// <param name="raiseChangedDelegate">PropertyChanged Delegate</param>
        /// <param name="propertyName"></param>
        private void SetValue<T>(ref T field, T newValue, bool disposeOldValue, RaisePropertyChangedDelegate<T> raiseChangedDelegate, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, newValue)) return;
            T oldValue = field;
            field = newValue;

            if (_syncContext == null)
                PostCallback<T>(newValue, disposeOldValue, raiseChangedDelegate, oldValue, propertyName);
            else
            {
                _syncContext.Send(delegate
                {
                    PostCallback<T>(newValue, disposeOldValue, raiseChangedDelegate, oldValue, propertyName);
                }, null);
            }
        }

        /// <summary>
        /// 현재값과 들어온 값이 같은 경우에도 이벤트를 발생하게 한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="disposeOldValue"></param>
        /// <param name="raiseChangedDelegate"></param>
        /// <param name="isEquals">OldValue.Equals(NewValue) = true 일 경우에도 raiseProperty 이벤트를 발생 시킨다.</param>
        /// <param name="propertyName"></param>
        private void SetValue<T>(ref T field, T newValue, bool disposeOldValue, RaisePropertyChangedDelegate<T> raiseChangedDelegate, bool isEquals, [CallerMemberName] string propertyName = "")
        {
            if (!isEquals && Equals(field, newValue)) return;
            T oldValue = field;
            field = newValue;

            if (_syncContext == null)
                PostCallback<T>(newValue, disposeOldValue, raiseChangedDelegate, oldValue, propertyName);
            else
            {
                _syncContext.Send(delegate
                {
                    PostCallback<T>(newValue, disposeOldValue, raiseChangedDelegate, oldValue, propertyName);
                }, null);
            }
        }

        /// <summary>
        /// SetValue Delegate CallBack 및 OldValue Dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newValue"></param>
        /// <param name="disposeOldValue"></param>
        /// <param name="raiseChangedDelegate"></param>
        /// <param name="oldValue"></param>
        /// <param name="propertyName"></param>
        private void PostCallback<T>(T newValue, bool disposeOldValue, RaisePropertyChangedDelegate<T> raiseChangedDelegate, T oldValue, [CallerMemberName] string propertyName = "")
        {
            NotifyPropertyChanged(propertyName);
            raiseChangedDelegate?.Invoke(oldValue, newValue);
            if (!disposeOldValue) return;
            if (oldValue is IDisposable disposableOldValue)
                disposableOldValue.Dispose();
        }
        protected override void DisposeManaged()
        {
            DisposeSignal = true;
            DisposeSignal = false;
            base.DisposeManaged();
        }

        #region Static
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        /// <summary>
        /// Static Notify
        /// </summary>
        /// <param name="propertyName"></param>
        protected static void NotifyStaticPropertyChanged([CallerMemberName] string propertyName = "")
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //Static
    }
}
