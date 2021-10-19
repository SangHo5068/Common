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
    /// Dictionary extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// A Dictionary&lt;TKey,TValue&gt; extension method that attempts to
        /// remove a key from the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">[out] The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <remarks>https://github.com/zzzprojects/Eval-SQL.NET/blob/master/src/Z.Expressions.SqlServer.Eval/Extensions/Dictionary%602/TryRemove.cs</remarks>
        public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            var isRemoved = dictionary.TryGetValue(key, out value);
            if (isRemoved)
            {
                dictionary.Remove(key);
            }

            return isRemoved;
        }

        /// <summary>
        ///     A Dictionary&lt;TKey,TValue&gt; extension method that adds or updates value for the
        ///     specified key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="updateValueFactory">The update value factory.</param>
        /// <returns>A TValue.</returns>
        /// <remarks>https://github.com/zzzprojects/Eval-SQL.NET/blob/master/src/Z.Expressions.SqlServer.Eval/Extensions/Dictionary%602/AddOrUpdate.cs</remarks>
        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue oldValue;
            if (dictionary.TryGetValue(key, out oldValue))
            {
                // TRY / CATCH should be done here, but this application does not require it
                value = updateValueFactory(key, oldValue);
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// A Dictionary&lt;TKey,TValue&gt; extension method that adds or
        /// updates value for the specified key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="addValueFactory">The add value factory.</param>
        /// <param name="updateValueFactory">The update value factory.</param>
        /// <returns>A TValue.</returns>
        /// <remarks>https://github.com/zzzprojects/Eval-SQL.NET/blob/master/src/Z.Expressions.SqlServer.Eval/Extensions/Dictionary%602/AddOrUpdate.cs</remarks>
        public static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            TValue oldValue;
            if (dictionary.TryGetValue(key, out oldValue))
            {
                value = updateValueFactory(key, oldValue);
                dictionary[key] = value;
            }
            else
            {
                value = addValueFactory(key);
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// A Dictionary&lt;TKey,TValue&gt; extension method that attempts to
        /// add a value in the dictionary for the specified key.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        /// <remarks>https://github.com/zzzprojects/Eval-SQL.NET/blob/master/src/Z.Expressions.SqlServer.Eval/Extensions/Dictionary%602/TryAdd.cs</remarks>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }

            return true;
        }

        /// <summary>
        /// Gets the value, if available, or <paramref name="ifNotFound"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="self">The dictionary to search.</param>
        /// <param name="key">The item key.</param>
        /// <param name="ifNotFound">The fallback value.</param>
        /// <returns>
        /// Returns the item in <paramref name="self"/> that matches <paramref name="key"/>,
        /// falling back to the value of <paramref name="ifNotFound"/> if the item is unavailable.
        /// </returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue ifNotFound = default(TValue))
        {
            TValue val;
            return self.TryGetValue(key, out val) ? val : ifNotFound;
        }

        /// <summary>
        /// Thread-safe way to gets or add the specified dictionary
        /// key and value pair.
        /// </summary>
        public static TValue GetOrAddThreadSafe<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
        {
            TValue tValue;
            TValue tValue1;

            lock (self)
            {
                if (!self.TryGetValue(key, out tValue))
                {
                    tValue = factory(key);
                    self[key] = tValue;
                }

                tValue1 = tValue;
            }

            return tValue1;
        }

        public static bool ContainsKeyWithValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, params TValue[] values)
        {
            if (self == null || values == null || values.Length == 0)
            {
                return false;
            }

            TValue temp;
            try
            {
                if (!self.TryGetValue(key, out temp))
                {
                    return false;
                }
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return values.Any(v => v.Equals(temp));
        }

        /// <summary>
        /// Tries to obtain the given key, otherwise returns the default value.
        /// </summary>
        /// <typeparam name="T">The struct type.</typeparam>
        /// <param name="values">The dictionary for the lookup.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>A nullable struct type.</returns>
        /// <remarks>https://github.com/AngleSharp/AngleSharp/blob/master/src/AngleSharp/Common/ObjectExtensions.cs#L106</remarks>
        public static T? TryGet<T>(this IDictionary<string, object> values, string key)
            where T : struct
        {
            if (values.TryGetValue(key, out var value) && value is T result)
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Tries to obtain the given key, otherwise returns null.
        /// </summary>
        /// <param name="values">The dictionary for the lookup.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>An object instance or null.</returns>
        /// <remarks>https://github.com/AngleSharp/AngleSharp/blob/master/src/AngleSharp/Common/ObjectExtensions.cs#L123</remarks>
        public static object TryGet(this IDictionary<string, object> values, string key)
        {
            values.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Gets the value of the given key, otherwise the provided default value.
        /// </summary>
        /// <typeparam name="T">The type of the keys.</typeparam>
        /// <typeparam name="TU">The type of the value.</typeparam>
        /// <param name="values">The dictionary for the lookup.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="defaultValue">The provided fallback value.</param>
        /// <returns>The value or the provided fallback.</returns>
        /// <remarks>https://github.com/AngleSharp/AngleSharp/blob/master/src/AngleSharp/Common/ObjectExtensions.cs#L139</remarks>
        public static TU GetOrDefault<T, TU>(this IDictionary<T, TU> values, T key, TU defaultValue)
        {
            return values.TryGetValue(key, out var value) ? value : defaultValue;
        }
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
