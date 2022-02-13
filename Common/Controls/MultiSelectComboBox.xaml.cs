using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Common.Controls
{
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {
        private const String All = "All";

        private ObservableCollection<KeyValue> _nodeList;
        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<KeyValue>();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MultiSelectComboBox.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox),
                new FrameworkPropertyMetadata(null,new PropertyChangedCallback(MultiSelectComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));



        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set => SetValue(ItemsSourceProperty, value);
        }

        public Dictionary<string, object> SelectedItems
        {
            get { return (Dictionary<string, object>)GetValue(SelectedItemsProperty); }
            set => SetValue(SelectedItemsProperty, value);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set => SetValue(TextProperty, value);
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set => SetValue(DefaultTextProperty, value);
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control)
                control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectComboBox control)
            {
                control.SelectNodes();
                control.SetText();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var clickedBox = (CheckBox)sender;
            if (clickedBox.Content.Equals(All) )
            {
                if (clickedBox.IsChecked.Value)
                {
                    foreach (var node in _nodeList)
                        node.IsSelected = true;
                }
                else
                {
                    foreach (var node in _nodeList)
                        node.IsSelected = false;
                }
            }
            else
            {
                int _selectedCount = 0;
                foreach (var s in _nodeList)
                {
                    if (s.IsSelected && s.Key != All)
                        _selectedCount++;
                }
                if (_selectedCount == _nodeList.Count - 1)
                    _nodeList.FirstOrDefault(i => i.Key == All).IsSelected = true;
                else
                    _nodeList.FirstOrDefault(i => i.Key == All).IsSelected = false;
            }
            SetSelectedItems();
            SetText();
        }
        #endregion


        #region Methods
        private void SelectNodes()
        {
            foreach (KeyValuePair<string, object> keyValue in SelectedItems)
            {
                var node = _nodeList.FirstOrDefault(i => i.Key == keyValue.Key);
                if (node != null)
                    node.IsSelected = true;
            }
        }

        private void SetSelectedItems()
        {
            if (SelectedItems == null)
                SelectedItems = new Dictionary<string, object>();
            SelectedItems.Clear();
            foreach (KeyValue node in _nodeList)
            {
                if (node.IsSelected && node.Key != All)
                {
                    if (this.ItemsSource.Count > 0)
                        SelectedItems.Add(node.Key, this.ItemsSource[node.Key]);
                }
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();
            if (this.ItemsSource.Count > 0)
                _nodeList.Add(new KeyValue(All, All));
            foreach (KeyValuePair<string, object> keyValue in this.ItemsSource)
            {
                var node = new KeyValue(keyValue.Key, keyValue.Value);
                _nodeList.Add(node);
            }
            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (this.SelectedItems != null)
            {
                StringBuilder displayText = new StringBuilder();
                foreach (KeyValue s in _nodeList)
                {
                    if (s.IsSelected == true && s.Key == All)
                    {
                        displayText = new StringBuilder();
                        displayText.Append(All);
                        break;
                    }
                    else if (s.IsSelected == true && s.Key != All)
                    {
                        displayText.Append(s.Key);
                        displayText.Append(',');
                    }
                }
                this.Text = displayText.ToString().TrimEnd(new char[] { ',' }); 
            }           
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = this.DefaultText;
            }
        }

        #endregion
    }
}
