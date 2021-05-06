using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Common.Properties;

namespace Common.Container.Events
{
    public interface IEventSubscription
    {
        SubscriptionToken SubscriptionToken { get; set; }

        Action<object[]> GetExecutionStrategy();
    }

    public class SubscriptionToken : IEquatable<SubscriptionToken>, IDisposable
    {
        private readonly Guid _token;

        private Action<SubscriptionToken> _unsubscribeAction;

        public SubscriptionToken(Action<SubscriptionToken> unsubscribeAction)
        {
            this._unsubscribeAction = unsubscribeAction;
            this._token = Guid.NewGuid();
        }

        public bool Equals(SubscriptionToken other)
        {
            return other != null && Equals(this._token, other._token);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || this.Equals(obj as SubscriptionToken);
        }

        public override int GetHashCode()
        {
            return this._token.GetHashCode();
        }

        public virtual void Dispose()
        {
            if (this._unsubscribeAction != null)
            {
                this._unsubscribeAction(this);
                this._unsubscribeAction = null;
            }

            GC.SuppressFinalize(this);
        }
    }



    public class EventSubscription : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;

        ///<summary>
        /// Creates a new instance of <see cref="EventSubscription"/>.
        ///</summary>
        ///<param name="actionReference">A reference to a delegate of type <see cref="System.Action"/>.</param>
        ///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        ///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action"/>.</exception>
        public EventSubscription(IDelegateReference actionReference)
        {
            if (actionReference == null)
            {
                throw new ArgumentNullException(nameof(actionReference));
            }

            if (actionReference.Target is Action == false)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidDelegateRerefenceTypeException, typeof(Action).FullName), nameof(actionReference));
            }

            this._actionReference = actionReference;
        }

        /// <summary>
        /// Gets the target <see cref="System.Action"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="System.Action"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Action Action => (Action)this._actionReference.Target;

        /// <summary>
        /// Gets or sets a <see cref="SubscriptionToken"/> that identifies this <see cref="IEventSubscription"/>.
        /// </summary>
        /// <value>A token that identifies this <see cref="IEventSubscription"/>.</value>
        public SubscriptionToken SubscriptionToken { get; set; }

        /// <summary>
        /// Gets the execution strategy to publish this event.
        /// </summary>
        /// <returns>An <see cref="System.Action"/> with the execution strategy, or <see langword="null" /> if the <see cref="IEventSubscription"/> is no longer valid.</returns>
        /// <remarks>
        /// If <see cref="Action"/>is no longer valid because it was
        /// garbage collected, this method will return <see langword="null" />.
        /// Otherwise it will return a delegate that evaluates the <see cref="Filter"/> and if it
        /// returns <see langword="true" /> will then call <see cref="InvokeAction"/>. The returned
        /// delegate holds a hard reference to the <see cref="Action"/> target
        /// <see cref="Delegate">delegates</see>. As long as the returned delegate is not garbage collected,
        /// the <see cref="Action"/> references delegates won't get collected either.
        /// </remarks>
        public virtual Action<object[]> GetExecutionStrategy()
        {
            var action = this.Action;
            if (action != null)
            {
                return arguments =>
                {
                    this.InvokeAction(action);
                };
            }
            return null;
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> synchronously when not overridden.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> is thrown if <paramref name="action"/> is null.</exception>
        public virtual void InvokeAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action();
        }
    }

    /// <summary>
    /// Extends <see cref="EventSubscription"/> to invoke the <see cref="EventSubscription.Action"/> delegate in a background thread.
    /// </summary>
    public class BackgroundEventSubscription : EventSubscription
    {
        /// <summary>
        /// Creates a new instance of <see cref="BackgroundEventSubscription"/>.
        /// </summary>
        /// <param name="actionReference">A reference to a delegate of type <see cref="System.Action"/>.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action"/>.</exception>
        public BackgroundEventSubscription(IDelegateReference actionReference)
            : base(actionReference)
        {
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action"/> in an asynchronous thread by using a <see cref="Task"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public override void InvokeAction(Action action)
        {
            Task.Run(action);
        }
    }

    ///<summary>
    /// Extends <see cref="EventSubscription"/> to invoke the <see cref="EventSubscription.Action"/> delegate
    /// in a specific <see cref="SynchronizationContext"/>.
    ///</summary>
    public class DispatcherEventSubscription : EventSubscription
    {
        private readonly SynchronizationContext _syncContext;

        ///<summary>
        /// Creates a new instance of <see cref="Common.PubSubEvents.BackgroundEventSubscription"/>.
        ///</summary>
        ///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
        ///<param name="context">The synchronization context to use for UI thread dispatching.</param>
        ///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        ///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>.</exception>
        public DispatcherEventSubscription(IDelegateReference actionReference, SynchronizationContext context)
            : base(actionReference)
        {
            this._syncContext = context;
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> asynchronously in the specified <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public override void InvokeAction(Action action)
        {
            this._syncContext.Post(o => action(), null);
        }
    }



    public class EventSubscription<TPayload> : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        public Action<TPayload> Action => (Action<TPayload>)this._actionReference.Target;

        public Predicate<TPayload> Filter => (Predicate<TPayload>)this._filterReference.Target;

        public SubscriptionToken SubscriptionToken { get; set; }

        public EventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
            {
                throw new ArgumentNullException("actionReference");
            }

            if (actionReference.Target is Action<TPayload> == false)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidDelegateRerefenceTypeException, typeof(Action<TPayload>).FullName), "actionReference");
            }

            if (filterReference == null)
            {
                throw new ArgumentNullException("filterReference");
            }

            if (filterReference.Target is Predicate<TPayload> == false)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidDelegateRerefenceTypeException, typeof(Predicate<TPayload>).FullName), "filterReference");
            }

            this._actionReference = actionReference;
            this._filterReference = filterReference;
        }

        public virtual Action<object[]> GetExecutionStrategy()
        {
            var action = this.Action;
            var filter = this.Filter;
            if (action != null && filter != null)
            {
                return arguments =>
                {
                    var payload = default(TPayload);
                    if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                    {
                        payload = (TPayload)arguments[0];
                    }

                    if (filter(payload) == false)
                    {
                        return;
                    }

                    this.InvokeAction(action, payload);
                };
            }

            return null;
        }

        public virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            action(argument);
        }
    }

    ///<summary>
    /// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate
    /// in a specific <see cref="SynchronizationContext"/>.
    ///</summary>
    /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
    public class DispatcherEventSubscription<TPayload> : EventSubscription<TPayload>
    {
        private readonly SynchronizationContext _syncContext;

        ///<summary>
        /// Creates a new instance of <see cref="Common.PubSubEvents.BackgroundEventSubscription{TPayload}"/>.
        ///</summary>
        ///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
        ///<param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
        ///<param name="context">The synchronization context to use for UI thread dispatching.</param>
        ///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        ///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
        ///or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
        public DispatcherEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference, SynchronizationContext context)
            : base(actionReference, filterReference)
        {
            this._syncContext = context;
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> asynchronously in the specified <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
        public override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            this._syncContext.Post((o) => action((TPayload)o), argument);
        }
    }

    /// <summary>
    /// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate in a background thread.
    /// </summary>
    /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
    public class BackgroundEventSubscription<TPayload> : EventSubscription<TPayload>
    {
        /// <summary>
        /// Creates a new instance of <see cref="BackgroundEventSubscription{TPayload}"/>.
        /// </summary>
        /// <param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
        /// <param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
        /// or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
        public BackgroundEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
            : base(actionReference, filterReference)
        {
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> in an asynchronous thread by using a <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="argument">The action to execute.</param>
        /// <param name="action">The payload to pass <paramref name="action"/> while invoking it.</param>
        public override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            Task.Run(() => action(argument));
        }
    }
}
