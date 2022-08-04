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

        public static Dictionary<TKey, TValue>CloneDictionaryCloningValues<TKey, TValue>
            (Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            var ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            return ret;
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

namespace System.Collections.ObjectModel
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Data;

    /// <summary>
    /// Implementation of a dynamic data collection based on generic Collection&lt;T&gt;,
    /// implementing INotifyCollectionChanged to notify listeners
    /// when items get added, removed or the whole list is refreshed.
    /// </summary>
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields    
        [NonSerialized]
        private DeferredEventsCollection _deferredEvents;
        #endregion Private Fields


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors
        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public RangeObservableCollection() { }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public RangeObservableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public RangeObservableCollection(List<T> list) : base(list) { }

        #endregion Constructors

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties
        EqualityComparer<T> _Comparer;
        public EqualityComparer<T> Comparer
        {
            get => _Comparer ?? EqualityComparer<T>.Default;
            private set => _Comparer = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this collection acts as a <see cref="HashSet{T}"/>,
        /// disallowing duplicate items, based on <see cref="Comparer"/>.
        /// This might indeed consume background performance, but in the other hand,
        /// it will pay off in UI performance as less required UI updates are required.
        /// </summary>
        public bool AllowDuplicates { get; set; } = true;

        #endregion Public Properties

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        /// <summary>
        /// Inserts the elements of a collection into the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="collection">The collection whose elements should be inserted into the List<T>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>                
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not in the collection range.</exception>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (!AllowDuplicates)
                collection =
                  collection
                  .Distinct(Comparer)
                  .Where(item => !Items.Contains(item, Comparer))
                  .ToList();

            if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                    return;
            }
            else if (!collection.Any())
                return;

            CheckReentrancy();

            //expand the following couple of lines when adding more constructors.
            var target = (List<T>)Items;
            target.InsertRange(index, collection);

            OnEssentialPropertiesChanged();

            if (!(collection is IList list))
                list = new List<T>(collection);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
        }


        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="collection">The items to remove.</param>        
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (Count == 0)
                return;
            else if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                    return;
                else if (countable.Count == 1)
                    using (IEnumerator<T> enumerator = countable.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        Remove(enumerator.Current);
                        return;
                    }
            }
            else if (!collection.Any())
                return;

            CheckReentrancy();

            var clusters = new Dictionary<int, List<T>>();
            var lastIndex = -1;
            List<T> lastCluster = null;
            foreach (T item in collection)
            {
                var index = IndexOf(item);
                if (index < 0)
                    continue;

                Items.RemoveAt(index);

                if (lastIndex == index && lastCluster != null)
                    lastCluster.Add(item);
                else
                    clusters[lastIndex = index] = lastCluster = new List<T> { item };
            }

            OnEssentialPropertiesChanged();

            if (Count == 0)
                OnCollectionReset();
            else
                foreach (KeyValuePair<int, List<T>> cluster in clusters)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster.Value, cluster.Key));

        }

        /// <summary>
        /// Iterates over the collection and removes all items that satisfy the specified match.
        /// </summary>
        /// <remarks>The complexity is O(n).</remarks>
        /// <param name="match"></param>
        /// <returns>Returns the number of elements that where </returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<T> match)
        {
            return RemoveAll(0, Count, match);
        }

        /// <summary>
        /// Iterates over the specified range within the collection and removes all items that satisfy the specified match.
        /// </summary>
        /// <remarks>The complexity is O(n).</remarks>
        /// <param name="index">The index of where to start performing the search.</param>
        /// <param name="count">The number of items to iterate on.</param>
        /// <param name="match"></param>
        /// <returns>Returns the number of elements that where </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(int index, int count, Predicate<T> match)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (Count == 0)
                return 0;

            List<T> cluster = null;
            var clusterIndex = -1;
            var removedCount = 0;

            using (BlockReentrancy())
            using (DeferEvents())
            {
                for (var i = 0; i < count; i++, index++)
                {
                    T item = Items[index];
                    if (match(item))
                    {
                        Items.RemoveAt(index);
                        removedCount++;

                        if (clusterIndex == index)
                        {
                            Debug.Assert(cluster != null);
                            cluster?.Add(item);
                        }
                        else
                        {
                            cluster = new List<T> { item };
                            clusterIndex = index;
                        }

                        index--;
                    }
                    else if (clusterIndex > -1)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster, clusterIndex));
                        clusterIndex = -1;
                        cluster = null;
                    }
                }

                if (clusterIndex > -1)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster, clusterIndex));
            }

            if (removedCount > 0)
                OnEssentialPropertiesChanged();

            return removedCount;
        }

        /// <summary>
        /// Removes a range of elements from the <see cref="ObservableCollection{T}"/>>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified range is exceeding the collection.</exception>
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count == 0)
                return;

            if (count == 1)
            {
                RemoveItem(index);
                return;
            }

            //Items will always be List<T>, see constructors
            var items = (List<T>)Items;
            List<T> removedItems = items.GetRange(index, count);

            CheckReentrancy();

            items.RemoveRange(index, count);

            OnEssentialPropertiesChanged();

            if (Count == 0)
                OnCollectionReset();
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, index));
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection,
        /// using <see cref="Comparer"/>.
        /// </summary>             
        /// <param name="collection">The items to fill the collection with, after clearing it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void ReplaceRange(IEnumerable<T> collection)
        {
            ReplaceRange(0, Count, collection);
        }

        /// <summary>
        /// Removes the specified range and inserts the specified collection in its position, leaving equal items in equal positions intact.
        /// </summary>
        /// <param name="index">The index of where to start the replacement.</param>
        /// <param name="count">The number of items to be replaced.</param>
        /// <param name="collection">The collection to insert in that location.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (!AllowDuplicates)
                collection =
                  collection
                  .Distinct(Comparer)
                  .ToList();

            if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                {
                    RemoveRange(index, count);
                    return;
                }
            }
            else if (!collection.Any())
            {
                RemoveRange(index, count);
                return;
            }

            if (index + count == 0)
            {
                InsertRange(0, collection);
                return;
            }

            if (!(collection is IList<T> list))
                list = new List<T>(collection);

            using (BlockReentrancy())
            using (DeferEvents())
            {
                var rangeCount = index + count;
                var addedCount = list.Count;

                var changesMade = false;
                List<T>
                  newCluster = null,
                  oldCluster = null;


                int i = index;
                for (; i < rangeCount && i - index < addedCount; i++)
                {
                    //parallel position
                    T old = this[i], @new = list[i - index];
                    if (Comparer.Equals(old, @new))
                    {
                        OnRangeReplaced(i, newCluster, oldCluster);
                        continue;
                    }
                    else
                    {
                        Items[i] = @new;

                        if (newCluster == null)
                        {
                            Debug.Assert(oldCluster == null);
                            newCluster = new List<T> { @new };
                            oldCluster = new List<T> { old };
                        }
                        else
                        {
                            newCluster.Add(@new);
                            oldCluster?.Add(old);
                        }

                        changesMade = true;
                    }
                }

                OnRangeReplaced(i, newCluster, oldCluster);

                //exceeding position
                if (count != addedCount)
                {
                    var items = (List<T>)Items;
                    if (count > addedCount)
                    {
                        var removedCount = rangeCount - addedCount;
                        T[] removed = new T[removedCount];
                        items.CopyTo(i, removed, 0, removed.Length);
                        items.RemoveRange(i, removedCount);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, i));
                    }
                    else
                    {
                        var k = i - index;
                        T[] added = new T[addedCount - k];
                        for (int j = k; j < addedCount; j++)
                        {
                            T @new = list[j];
                            added[j - k] = @new;
                        }
                        items.InsertRange(i, added);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, i));
                    }

                    OnEssentialPropertiesChanged();
                }
                else if (changesMade)
                {
                    OnIndexerPropertyChanged();
                }
            }
        }

        #endregion Public Methods


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            if (Count == 0)
                return;

            CheckReentrancy();
            base.ClearItems();
            OnEssentialPropertiesChanged();
            OnCollectionReset();
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            if (!AllowDuplicates && Items.Contains(item))
                return;

            base.InsertItem(index, item);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            if (AllowDuplicates)
            {
                if (Comparer.Equals(this[index], item))
                    return;
            }
            else
              if (Items.Contains(item, Comparer))
                return;

            CheckReentrancy();
            T oldItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
        }

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_deferredEvents != null)
            {
                _deferredEvents.Add(e);
                return;
            }
            base.OnCollectionChanged(e);
        }

        protected virtual IDisposable DeferEvents() => new DeferredEventsCollection(this);

        #endregion Protected Methods


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// Helper to raise Count property and the Indexer property.
        /// </summary>
        void OnEssentialPropertiesChanged()
        {
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
            OnIndexerPropertyChanged();
        }

        /// <summary>
        /// /// Helper to raise a PropertyChanged event for the Indexer property
        /// /// </summary>
        void OnIndexerPropertyChanged() =>
         OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index) =>
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        void OnCollectionReset() =>
         OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        /// <summary>
        /// Helper to raise event for clustered action and clear cluster.
        /// </summary>
        /// <param name="followingItemIndex">The index of the item following the replacement block.</param>
        /// <param name="newCluster"></param>
        /// <param name="oldCluster"></param>
        //TODO should have really been a local method inside ReplaceRange(int index, int count, IEnumerable<T> collection, IEqualityComparer<T> comparer),
        //move when supported language version updated.
        void OnRangeReplaced(int followingItemIndex, ICollection<T> newCluster, ICollection<T> oldCluster)
        {
            if (oldCluster == null || oldCluster.Count == 0)
            {
                Debug.Assert(newCluster == null || newCluster.Count == 0);
                return;
            }

            OnCollectionChanged(
              new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                new List<T>(newCluster),
                new List<T>(oldCluster),
                followingItemIndex - oldCluster.Count));

            oldCluster.Clear();
            newCluster.Clear();
        }

        #endregion Private Methods

        //------------------------------------------------------
        //
        //  Private Types
        //
        //------------------------------------------------------

        #region Private Types
        sealed class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
        {
            readonly RangeObservableCollection<T> _collection;
            public DeferredEventsCollection(RangeObservableCollection<T> collection)
            {
                Debug.Assert(collection != null);
                Debug.Assert(collection._deferredEvents == null);
                _collection = collection;
                _collection._deferredEvents = this;
            }

            public void Dispose()
            {
                _collection._deferredEvents = null;
                foreach (var args in this)
                    _collection.OnCollectionChanged(args);
            }
        }

        #endregion Private Types
    }

    /// <remarks>
    /// To be kept outside <see cref="ObservableCollection{T}"/>, since otherwise, a new instance will be created for each generic type used.
    /// </remarks>
    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }

    public class SortableObservableCollection<T> : RangeObservableCollection<T>
    {
        public SortableObservableCollection()
        {
        }

        public SortableObservableCollection(IComparer<T> comparer, bool descending = false) : base()
        {
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            IsDescending = descending;
        }

        public SortableObservableCollection(IEnumerable<T> collection, IComparer<T> comparer, bool descending = false) : base(collection)
        {
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            IsDescending = descending;

            Sort();
        }

        IComparer<T> _Comparer;
        public IComparer<T> Comparer
        {
            get => _Comparer ?? Comparer<T>.Default;
            set
            {
                _Comparer = value;
                Sort();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Comparer)));
            }
        }

        bool _IsDescending;
        /// <summary>
        /// Gets or sets a value indicating whether the sorting should be descending.
        /// Default value is false.
        /// </summary>
        public bool IsDescending
        {
            get => _IsDescending;
            set
            {
                if (_IsDescending != value)
                {
                    _IsDescending = value;
                    Sort();
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsDescending)));
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (_Reordering) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    return;
            }

            Sort();
        }

        bool _Reordering;
        public void Sort() // TODO, concern change index so no need to walk the whole list
        {
            var query = this
              .Select((item, index) => (Item: item, Index: index));
            query = IsDescending
              ? query.OrderByDescending(tuple => tuple.Item, Comparer)
              : query.OrderBy(tuple => tuple.Item, Comparer);

            var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index))
             .Where(o => o.OldIndex != o.NewIndex);

            using (var enumerator = map.GetEnumerator())
                if (enumerator.MoveNext())
                {
                    _Reordering = true;
                    Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
                    _Reordering = false;
                }
        }
    }

    public class WpfObservableRangeCollection<T> : ObservableCollection<T>
    {
        DeferredEventsCollection _deferredEvents;

        public WpfObservableRangeCollection()
        {
        }

        public WpfObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public WpfObservableRangeCollection(List<T> list) : base(list)
        {
        }


        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var _deferredEvents = (ICollection<NotifyCollectionChangedEventArgs>)typeof(ObservableCollection<T>).GetField("_deferredEvents", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            if (_deferredEvents != null)
            {
                _deferredEvents.Add(e);
                return;
            }

            foreach (var handler in GetHandlers())
                if (IsRange(e) && handler.Target is CollectionView cv)
                    cv.Refresh();
                else
                    handler(this, e);
        }

        protected IDisposable DeferEvents() => new DeferredEventsCollection(this);

        bool IsRange(NotifyCollectionChangedEventArgs e) => e.NewItems?.Count > 1 || e.OldItems?.Count > 1;
        IEnumerable<NotifyCollectionChangedEventHandler> GetHandlers()
        {
            var info = typeof(ObservableCollection<T>).GetField(nameof(CollectionChanged), BindingFlags.Instance | BindingFlags.NonPublic);
            var @event = (MulticastDelegate)info.GetValue(this);
            return @event?.GetInvocationList()
              .Cast<NotifyCollectionChangedEventHandler>()
              .Distinct()
              ?? Enumerable.Empty<NotifyCollectionChangedEventHandler>();
        }

        class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
        {
            private readonly WpfObservableRangeCollection<T> _collection;
            public DeferredEventsCollection(WpfObservableRangeCollection<T> collection)
            {
                Debug.Assert(collection != null);
                Debug.Assert(collection._deferredEvents == null);
                _collection = collection;
                _collection._deferredEvents = this;
            }

            public void Dispose()
            {
                _collection._deferredEvents = null;

                var handlers = _collection
                  .GetHandlers()
                  .ToLookup(h => h.Target is CollectionView);

                foreach (var handler in handlers[false])
                    foreach (var e in this)
                        handler(_collection, e);

                foreach (var cv in handlers[true]
                  .Select(h => h.Target)
                  .Cast<CollectionView>()
                  .Distinct())
                    cv.Refresh();
            }
        }
    }
}