using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Common.Utilities;


namespace Common.Controls
{
    [TemplatePart(Name = PART_ApplyButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_Popup,       Type = typeof(Popup))]
    [TemplatePart(Name = PART_ComboBox,    Type = typeof(ComboBox))]
    public class MultiComboBox : UserControl
    {
        private const String PART_ApplyButton = "PART_ApplyButton";
        private const String PART_Popup       = "PART_Popup";
        private const String PART_ComboBox    = "PART_ComboBox";
        /// <summary>
        /// Text: 전체
        /// </summary>
        private const String All = "All";
        /// <summary>
        /// Text: 적용
        /// </summary>
        private const String Apply = "Apply";

        private Button _ApplyButton;
        private Popup _Popup;
        private ComboBox MultiCombo;
        private ObservableCollection<KeyValue> _nodeList;


        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<KeyValue>), typeof(MultiComboBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MultiComboBox.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<KeyValue>), typeof(MultiComboBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MultiComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MultiComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiComboBox), new UIPropertyMetadata(string.Empty));

        /// <summary>
        /// Dependency property backing for IsDropDownOpen.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(MultiComboBox));

        /// <summary>
        /// Dependency backing for the MaxDropDownHeight property.
        /// </summary>
        public static readonly DependencyProperty MaxDropDownHeightProperty = ComboBox.MaxDropDownHeightProperty.AddOwner(typeof(MultiComboBox));


        public ObservableCollection<KeyValue> ItemsSource
        {
            get { return (ObservableCollection<KeyValue>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ObservableCollection<KeyValue> SelectedItems
        {
            get { return (ObservableCollection<KeyValue>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the drop down is open.
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating the maximium height of the drop down.
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }
        #endregion //Dependency



        public MultiComboBox()
        {
            _nodeList = new ObservableCollection<KeyValue>();

            this.Loaded += MultiComboBox_Loaded;
        }


        #region Override
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            try
            {
                //_ApplyButton = this.GetTemplateChild(PART_ApplyButton) as Button;
                //_Popup       = this.GetTemplateChild(PART_Popup)       as Popup;
                MultiCombo  = this.GetTemplateChild(PART_ComboBox) as ComboBox;

                if (MultiCombo != null)
                {
                    _Popup       = this.MultiCombo?.Template.FindName(PART_Popup, this.MultiCombo) as Popup;
                    _ApplyButton = this.MultiCombo?.Template.FindName(PART_ApplyButton, this.MultiCombo) as Button;
                }

                //if (_ApplyButton != null)
                //{
                //    //Text: 적용
                //    _ApplyButton.Content = Apply;
                //    _ApplyButton.Click += OnApplyButton_Click;
                //}
                //if (_Popup != null)
                //{
                //    _Popup.LostFocus += (s, e) => {
                //        if (_Popup.IsOpen)
                //            _Popup.IsOpen = false;
                //    };
                //}
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }
        /// <summary>
        /// Open the drop down with the enter key.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (IsDropDownOpen)
            {
                if (this.ItemsSource == null)
                    return;
                foreach (KeyValue item in this.ItemsSource)
                {
                    var listBoxItem = (this.MultiCombo.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem);
                    if (listBoxItem != null && listBoxItem.IsMouseOver)
                    {
                        IsDropDownOpen = false;
                        break;
                    }
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            try
            {
                if (!IsDropDownOpen)
                {
                    if (SelectedItems?.Count == 0)
                    {
                        var nodeAll = _nodeList.OfType<KeyValue>().Where(w => w.Value.Equals(All)).FirstOrDefault();
                        if (nodeAll != null) nodeAll.IsSelected = true;
                    }
                    return;
                }

                // Set Handled here, popup doesn't recieve the click event so it stays open.
                e.Handled = true;

                // Click event will not reach the list box, so need to manually select or 
                // unselect the item which is under the current mouse position.
                var bAll = false;
                foreach (KeyValue item in this._nodeList)
                {
                    var listBoxItem = (this.MultiCombo.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem);
                    // 선택된 값이 "전체" 이면
                    if (listBoxItem != null && listBoxItem.IsMouseOver && (item.Value.Equals(All)))
                    {
                        bAll = item.IsSelected = true;
                        break; // exit, no more to do.
                    }
                }

                if (bAll)
                {
                    // 선택된 값이 "전체" 이면 다른 모든 아이템은 Selected=False로 한다.
                    foreach (KeyValue item in this._nodeList)
                    {
                        //ComboBoxItem listBoxItem = (this.MultiSelectCombo.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem);
                        //if (listBoxItem != null && (item.Value != strAll) && listBoxItem.IsSelected)
                        if (item.Value.Equals(All) && item.IsSelected)
                            item.IsSelected = false;
                    }
                    return; // exit, no more to do.
                }
                else
                {
                    // "전체" 항목을 False 로 만들고
                    //ComboBoxItem AllItem = (this.MultiSelectCombo.ItemContainerGenerator.ContainerFromItem(this._nodeList[0]) as ComboBoxItem);
                    //if (AllItem != null && AllItem.IsSelected) AllItem.IsSelected = false;
                    var nodeAll = _nodeList.OfType<KeyValue>().Where(w => w.Value.Equals(All)).FirstOrDefault();
                    if (nodeAll != null) nodeAll.IsSelected = false;

                    // 선택된 아이템을 찾아서 Selected를 변경한다.
                    foreach (KeyValue item in this._nodeList)
                    {
                        var listBoxItem = (this.MultiCombo.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem);
                        if (listBoxItem != null && listBoxItem.IsMouseOver)
                        {
                            item.IsSelected = !item.IsSelected;
                            return; // exit, no more to do.
                        }
                    }
                }

                // If ApplyButton != false
                if (_ApplyButton != null && !_ApplyButton.IsMouseOver
                    && !(this.MultiCombo.Template.FindName("DropDownScrollViewer", this.MultiCombo) as ScrollViewer).IsMouseOver)
                {
                    if (e.Handled && this.IsMouseOver && !(this.MultiCombo.Template.FindName("ButtonBorder", this.MultiCombo) as Border).IsMouseOver)
                        IsDropDownOpen = false;
                    return;
                }

                // If execution reaches here, the mouse was not over any ListBoxItems, so it is either
                // over the toggle button or it is over the CreateNewItem button. If the latter, we want the 
                // mouse click to go through so the button can respond. (It in turn handles the event so the 
                // popup stays open.)
                if (_Popup != null && _Popup.Child.IsMouseOver)
                    e.Handled = false;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
                if (_Popup != null && _Popup.Child.IsMouseOver)
                    e.Handled = false;
            }
            finally
            {
                SetSelectedItems();
                SetText();
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (_Popup == null)
                return;
            if (_Popup.IsOpen)
                _Popup.IsOpen = false;
        }
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
        }
        #endregion //Override

        #region Events

        private void MultiComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _Popup       = this.MultiCombo.Template.FindName(PART_Popup, this.MultiCombo) as Popup;
                _ApplyButton = this.MultiCombo.Template.FindName(PART_ApplyButton, this.MultiCombo) as Button;
                if (_ApplyButton != null)
                {
                    //Text: 적용
                    _ApplyButton.Content = Apply;
                    _ApplyButton.Click += OnApplyButton_Click;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private void OnApplyButton_Click(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MultiComboBox)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MultiComboBox)d;
            control.SelectNodes();
            control.SetText();
        }
        #endregion //Events

        #region Methods
        private void SelectNodes()
        {
            try
            {
                var list = _nodeList.OfType<KeyValue>().Where(w => w.IsSelected).ToList();
                foreach (KeyValue item in list)
                    item.IsSelected = false;
                if (SelectedItems == null) return;
                foreach (KeyValue keyValue in SelectedItems)
                {
                    var node = _nodeList.FirstOrDefault(i => i.Key.ToString() == keyValue.Key.ToString());
                    if (node != null)
                        node.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private void SetSelectedItems()
        {
            if (SelectedItems == null)
                SelectedItems = new ObservableCollection<KeyValue>();
            SelectedItems.Clear();
            foreach (KeyValue node in _nodeList.OfType<KeyValue>().Where(w => w.IsSelected).ToList())
            {
                if (node.IsSelected && node.Key != "0")
                    SelectedItems.Add(node);
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();
            foreach (KeyValue keyValue in this.ItemsSource)
            {
                var node = new KeyValue(keyValue.Key.ToString(), keyValue.Value);
                _nodeList.Add(node);
            }
            MultiCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (this.SelectedItems != null)
            {
                string s;
                if (SelectedItems != null && SelectedItems.Count > 0)
                {
                    if (SelectedItems.Count == 1)
                    {
                        var v = SelectedItems.OfType<KeyValue>().First();
                        s = v.Value?.ToString();
                    }
                    else
                        //Text: 건
                        s = SelectedItems.Count + "건";
                }
                else
                    s = All;
                this.Text = s;
            }

            if (string.IsNullOrEmpty(this.Text))
                this.Text = this.DefaultText;
        }
        #endregion //Methods
    }
}
