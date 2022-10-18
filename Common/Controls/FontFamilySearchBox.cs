using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;


namespace Common.Controls
{
    public class FontFamilySearchBox : Grid, INotifyPropertyChanged
    {
        private readonly List<string> _listFontFamily;
        private double _listBox_Height;

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FontFamilySearchBox), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedFontProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(FontFamilySearchBox), new PropertyMetadata());


        #region Properties

        public double ListBox_Height
        {
            get => _listBox_Height;
            set
            {
                _listBox_Height = value;
                FontFamilyListBox.Height = ListBox_Height;
            }
        }

        public TextBox SearchTextBox { get; set; }
        public ListBox FontFamilyListBox { get; set; }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public object SelectedFont
        {
            get => GetValue(SelectedFontProperty);
            set
            {
                SetValue(SelectedFontProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedFont"));
            }
        }

        private void SetTextToItem() => SearchTextBox.Text = (string)FontFamilyListBox.SelectedItem ?? SearchTextBox.Text;
        #endregion //Properties



        public FontFamilySearchBox()
        {
            _listFontFamily = new List<string>(Fonts.SystemFontFamilies.Count);
            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                if (font.FamilyNames.ContainsKey(cond))
                    _listFontFamily.Add(font.FamilyNames[cond]);
                else
                    _listFontFamily.Add(font.ToString());
            }
            _listFontFamily.Sort();


            SearchTextBox = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Top,
                DataContext = this
            };
            SearchTextBox.SetBinding(TextBox.TextProperty, new Binding("Text") { Mode = BindingMode.TwoWay });
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;


            FontFamilyListBox = new ListBox()
            {
                ItemsSource = _listFontFamily,
                Visibility = Visibility.Collapsed,
                SelectionMode = SelectionMode.Single,
                VerticalAlignment = VerticalAlignment.Top,
                DataContext = this
            };
            FontFamilyListBox.SetBinding(ListBox.SelectedItemProperty, new Binding("SelectedFont") { Mode = BindingMode.TwoWay });


            SearchTextBox.PreviewKeyDown += SearchTextBox_KeyDown;
            SearchTextBox.GotFocus += FontFamilySearchBox_GotFocus;
            SearchTextBox.LostFocus += FontFamilySearchBox_LostFocus;

            FontFamilyListBox.KeyDown += FontFamilyListBox_KeyDown;
            FontFamilyListBox.PreviewMouseLeftButtonDown += FontFamilyListBox_PreviewMouseLeftButtonDown;
            FontFamilyListBox.GotFocus += FontFamilySearchBox_GotFocus;
            FontFamilyListBox.LostFocus += FontFamilySearchBox_LostFocus;

            SizeChanged += FontFamilySearchBox_SizeChanged;
            GotFocus += FontFamilySearchBox_GotFocus;
            LostFocus += FontFamilySearchBox_LostFocus;

            Children.Add(SearchTextBox);
            Children.Add(FontFamilyListBox);
        }

        private void FontFamilySearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FontFamilyListBox.Visibility = Visibility.Visible;
        }

        private void FontFamilySearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsFocused == false
                && SearchTextBox.IsFocused == false
                && FontFamilyListBox.IsFocused == false)
            {
                FontFamilyListBox.Visibility = Visibility.Collapsed;
                SearchTextBox.Text = (string)FontFamilyListBox.SelectedItem ?? string.Empty;
            }
        }

        private void FontFamilySearchBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SearchTextBox.Height = ActualHeight;
            FontFamilyListBox.Margin = new Thickness(0, ActualHeight, 0, -ActualHeight - ListBox_Height);
        }

        private void FontFamilyListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetTextToItem();
            }

            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
            SearchTextBox.Focus();
        }

        private void FontFamilyListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(FontFamilyListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
                SearchTextBox.Text = (string)item.Content;
                SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (!FontFamilyListBox.Items.MoveCurrentToNext())
                    {
                        FontFamilyListBox.Items.MoveCurrentToLast();
                    }
                    break;

                case Key.Down:
                    if (!FontFamilyListBox.Items.MoveCurrentToPrevious())
                    {
                        FontFamilyListBox.Items.MoveCurrentToFirst();
                    }
                    break;

                case Key.Enter:
                    SetTextToItem();
                    SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
                    return;

                default:
                    return;
            }

            ListBoxItem lbi = (ListBoxItem)FontFamilyListBox.ItemContainerGenerator.ContainerFromItem(FontFamilyListBox.SelectedItem);
            lbi?.Focus();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                FontFamilyListBox.Visibility = Visibility.Collapsed;
                return;
            }

            FontFamilyListBox.Visibility = Visibility.Visible;
            SetValue(TextProperty, SearchTextBox.Text);

            string lower = SearchTextBox.Text.ToLower();
            foreach (var item in _listFontFamily)
            {
                if (item.ToLower().StartsWith(lower))
                {
                    FontFamilyListBox.SelectedItem = item;

                    ScrollViewer scrollViewer = GetScrollViewer(FontFamilyListBox) as ScrollViewer;
                    scrollViewer?.ScrollToHome();
                    FontFamilyListBox.ScrollIntoView(item);
                    return;
                }
            }
        }

        private static DependencyObject GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }
    }
}
