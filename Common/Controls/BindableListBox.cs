using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Common.Controls
{
    public class KeyValue : DependencyObject
    {
        #region <Define>

        #region <DependencyProperty>
        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty;
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty;

        public static readonly DependencyProperty IsSelectedProperty;
        #endregion //DependencyProperty

        #region <Property>
        public String Key
        {
            get { return (String)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public Object Value
        {
            get { return (Object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Boolean IsSelected
        {
            get { return (Boolean)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        #endregion //Property

        #endregion //Define


        public KeyValue(String pKey, Object pValue)
        {
            this.Key = pKey;
            this.Value = pValue;
        }
        static KeyValue()
        {
            KeyProperty = DependencyProperty.Register("Key", typeof(String), typeof(KeyValue), new PropertyMetadata("0"));
            ValueProperty = DependencyProperty.Register("Value", typeof(Object), typeof(KeyValue), new PropertyMetadata(null));
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(Boolean), typeof(KeyValue), new UIPropertyMetadata(false));
        }
    }

    /// <summary>
    /// Extends ListBox by making the SelectedItems property read/write.
    /// ListBox does have a SelectedItems property but it is read-only. 
    /// MultiSelectListBox hides the base class's SelectedItems property.
    /// The SelectedItems property of MultiSelectListBox can participate in two way data binding 
    /// and Can be set in Xaml, whereas in the base class it can only participate in one way binding.
    /// SelectionMode should not be Single when using the SelectedItems property.
    /// By default, selection mode is Multiple.
    /// </summary>
    public class BindableListBox : ListBox
    {
        #region DependencyProperty
        public static readonly DependencyProperty SelectedItemsListProperty =
           DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(BindableListBox), new PropertyMetadata(null));

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }


        //SelectedItems
        ///// <summary>
        ///// The SelectedItems dependency property. Access to the values of the items that are 
        ///// selected in the selectedItems box. If SelectionMode is Single, this property returns an array
        ///// of length one.
        ///// </summary>
        //public static new readonly DependencyProperty SelectedItemsProperty =
        //   DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<KeyValue>), typeof(BindableListBox), new PropertyMetadata(null));
        ////new UIPropertyMetadata(new ObservableCollection<KeyValue>(), 
        ////       (d, e) =>
        ////       {
        ////           // When the property changes, update the selected values in the selectedItems box.
        ////           if (e.NewValue != null)
        ////               (d as BindableListBox).SetSelectedItemsNew(e.NewValue as ObservableCollection<KeyValue>);
        ////       }));
        ///// <summary>
        ///// Get or set the selected items.
        ///// </summary>
        //public new ObservableCollection<KeyValue> SelectedItems
        //{
        //    get { return GetValue(SelectedItemsProperty) as ObservableCollection<KeyValue>; }
        //    set { SetValue(SelectedItemsProperty, value); }
        //}

        #endregion //DependencyProperty



        public BindableListBox()
        {
            // Add handler for when the listbox internal selection changes.
            base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
        }




        #region Methods

        /// <summary>
        /// Call this method to programmatically add an item to the selected items collection.
        /// Because this class hides ListBox.SelectedItems, it is necessary add the value directly
        /// to the base class's collection for all the related events to properly propagate.
        /// </summary>
        protected int SelectItem(object item)
        {
            return base.SelectedItems.Add(item);
        }

        /// <summary>
        /// Call this method to programmatically add an item to the selectedItems that is displayed in 
        /// the listbox. NOTE: For the selectedItems box display to update, ItemsSource must impliment INotifyCollectionChanged
        /// such as ObservableCollection.
        /// </summary>
        protected int AddItem(object item)
        {
            if (ItemsSource != null)
                return (ItemsSource as IList).Add(item);
            else
                return base.Items.Add(item);
        }


        /// <summary>
        /// Synchronizes the selected items of this class with the selected items of the base class.
        /// </summary>
        void UpdateNewClassSelectedItems(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
            //// If null, then we aren't bound to.
            //if (SelectedItems == null)
            //    return;

            //try
            //{
            //    //선택된 아이템이 "전체"일 경우 SelectedItems 의 컬랙션을 모두 삭제하고 "전체"만 새로 추가한다.
            //    if (e.AddedItems.Cast<KeyValue>().FirstOrDefault().Key == "0")
            //        SelectedItems.Clear();

            //    foreach (var o in e.AddedItems)
            //    {
            //        //if (SelectedItems.OfType<KeyValue>().Where(s => s.Key == (o as KeyValue).Key).FirstOrDefault().Value == null)
            //        SelectedItems.Add((KeyValue)o);
            //    }
            //    foreach (var o in e.RemovedItems)
            //    {
            //        KeyValue a = SelectedItems.OfType<KeyValue>().Where(s => s.Key == (o as KeyValue).Key).FirstOrDefault();
            //        SelectedItems.Remove(a);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.WriteLog(LogTypes.Exception, "", ex);
            //}
        }

        /// <summary>
        /// Synchronizes the selected items with the selected values. 
        /// </summary>
        protected virtual void SetSelectedItemsNew(ObservableCollection<KeyValue> newSelectedItems)
        {
            if (newSelectedItems == null)
                throw new InvalidOperationException("Collection cannot be null");

            // Remove the event handler to prevent recursion.
            base.SelectionChanged -= new SelectionChangedEventHandler(UpdateNewClassSelectedItems);

            base.SetSelectedItems(SelectedItems);

            // Reestablish the event handler.
            base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);

            // Add a collection changed handler to the new list, if it supports the interface.
            AddSelectedItemsChangedHandler(newSelectedItems as INotifyCollectionChanged);
        }

        private void AddSelectedItemsChangedHandler(INotifyCollectionChanged collection)
        {
            if (collection != null)
                collection.CollectionChanged += new NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
        }

        private void RemoveSelectedItemsChangedHandler(INotifyCollectionChanged collection)
        {
            if (collection != null)
                collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
        }

        void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SelectionChanged -= new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
            base.SetSelectedItems(SelectedItems);
            base.SelectionChanged += new SelectionChangedEventHandler(UpdateNewClassSelectedItems);
        }

        #endregion //Methods
    }
}
