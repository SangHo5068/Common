using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Common.Extensions
{
    public class ObservableDictionary<T, TV> : Dictionary<T, TV>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableDictionary() { }
        public new void Add(T key, TV value)
        {
            base.Add(key, value);
            NotifyCollectionChangedEventArgs e =
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            try
            {
                CollectionChanged?.Invoke(this, e);
            }
            catch (NotSupportedException)
            {
                NotifyCollectionChangedEventArgs alternativeEventArgs =
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(alternativeEventArgs);
            }
        }
    }

    /// <summary>
    /// Expanded ObservableCollection to include some List<T> Methods
    /// </summary>
    [Serializable]
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Constructors
        /// </summary>
        public ObservableCollectionEx() : base() { }
        public ObservableCollectionEx(List<T> l) : base(l) { }
        public ObservableCollectionEx(IEnumerable<T> l) : base(l) { }



        #region Sorting
        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            InternalSort(Items.OrderBy(keySelector));
        }

        /// <summary>
        /// Sorts the items of the collection in descending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        public void SortDescending<TKey>(Func<T, TKey> keySelector)
        {
            InternalSort(Items.OrderByDescending(keySelector));
        }

        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            InternalSort(Items.OrderBy(keySelector, comparer));
        }

        /// <summary>
        /// Moves the items of the collection so that their orders are the same as those of the items provided.
        /// </summary>
        /// <param name="sortedItems">An <see cref="IEnumerable{T}"/> to provide item orders.</param>
        private void InternalSort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }
        #endregion // Sorting
    }

    /// <summary>
    /// Collection 확장 메서드
    /// </summary>
    public static class CollectionExtensions
    {
        public static TCol AddRange<TCol, TItem>(this TCol destination, IEnumerable<TItem> source)
            where TCol : ICollection<TItem>
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (source == null) throw new ArgumentNullException(nameof(source));

            // don't cast to IList to prevent recursion
            if (destination is List<TItem> list)
            {
                list.AddRange(source);
                return destination;
            }

            foreach (var item in source)
                destination.Add(item);

            return destination;
        }

        public static TCol RemoveRange<TCol, TItem>(this TCol destination, IEnumerable<TItem> source)
            where TCol : ICollection<TItem>
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (source == null) throw new ArgumentNullException(nameof(source));

            // don't cast to IList to prevent recursion
            if (destination is List<TItem> list)
            {
                source.ToList().ForEach(f =>
                {
                    if (list.Contains(f))
                        list.Remove(f);
                });
                return destination;
            }

            source.ToList().ForEach(f =>
            {
                if (destination.Contains(f))
                    destination.Remove(f);
            });

            return destination;
        }
    }

    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext _synchronizationContext = AsyncOperationManager.SynchronizationContext;

        public AsyncObservableCollection()
        {
        }

        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        private void ExecuteOnSyncContext(Action action)
        {
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                action();
            }
            else
            {
                _synchronizationContext.Send(_ => action(), null);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            ExecuteOnSyncContext(() => base.InsertItem(index, item));
        }

        protected override void RemoveItem(int index)
        {
            ExecuteOnSyncContext(() => base.RemoveItem(index));
        }

        protected override void SetItem(int index, T item)
        {
            ExecuteOnSyncContext(() => base.SetItem(index, item));
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            ExecuteOnSyncContext(() => base.MoveItem(oldIndex, newIndex));
        }

        protected override void ClearItems()
        {
            ExecuteOnSyncContext(() => base.ClearItems());
        }

        //protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    if (SynchronizationContext.Current == _synchronizationContext)
        //    {
        //        // Execute the CollectionChanged event on the current thread
        //        RaiseCollectionChanged(e);
        //    }
        //    else
        //    {
        //        // Raises the CollectionChanged event on the creator thread
        //        _synchronizationContext.Send(RaiseCollectionChanged, e);
        //    }
        //}

        //private void RaiseCollectionChanged(object param)
        //{
        //    // We are in the creator thread, call the base implementation directly
        //    base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        //}

        //protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    if (SynchronizationContext.Current == _synchronizationContext)
        //    {
        //        // Execute the PropertyChanged event on the current thread
        //        RaisePropertyChanged(e);
        //    }
        //    else
        //    {
        //        // Raises the PropertyChanged event on the creator thread
        //        _synchronizationContext.Send(RaisePropertyChanged, e);
        //    }
        //}

        //private void RaisePropertyChanged(object param)
        //{
        //    // We are in the creator thread, call the base implementation directly
        //    base.OnPropertyChanged((PropertyChangedEventArgs)param);
        //}
    }
}
