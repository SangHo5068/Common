using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Common.Base;
using Common.Command;
using Common.Utilities;

namespace Common.Controls
{
    public class Paging : Dictionary<String, String> { }


    /// <summary>
    /// PagingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PagingControl : BaseUserControl
    {
        #region Const
        /// <summary>
        /// 리스트 조회의 검색 갯수입니다. (기본 100건)
        /// </summary>
        public const int PAGE_COUNT = 100;

        public const string Language_Total = "Total";
        public const string Language_Count = "Count";
        public const string Language_Firs = "Firs";
        public const string Language_Prev = "Prev";
        public const string Language_Next = "Next";
        public const string Language_Last = "Last";
        public const string Language_MovePage = "MovePage";
        #endregion //Const

        #region DependencyProperty
        public static readonly DependencyProperty FirstProperty;
        public static readonly DependencyProperty CurrentPageProperty;
        public static readonly DependencyProperty CurrentMinProperty;
        public static readonly DependencyProperty CurrentMaxProperty;
        public static readonly DependencyProperty LastProperty;
        public static readonly DependencyProperty TotalCountProperty;
        public static readonly DependencyProperty PageSizeProperty;
        public static readonly DependencyProperty PageSizesVisibilityProperty;
        public static readonly DependencyProperty PageSizesCollectionProperty;

        public static readonly DependencyProperty LanguageTotalProperty;
        public static readonly DependencyProperty LanguageCountProperty;
        public static readonly DependencyProperty LanguageFirstProperty;
        public static readonly DependencyProperty LanguagePreviousProperty;
        public static readonly DependencyProperty LanguageNextProperty;
        public static readonly DependencyProperty LanguageLastProperty;
        public static readonly DependencyProperty LanguageMovePageProperty;
        #endregion //DependencyProperty

        #region Event
        public delegate void PageChangedEventHandler(Object sender, int CurrPage);
        public event PageChangedEventHandler PageChanged;

        public delegate void PageSizeChangedEventHandler(int PageSize);
        public event PageSizeChangedEventHandler PageSizeChanged;
        #endregion //Event

        #region Properties
        [Description("첫번째 페이지")]
        public int First
        {
            get { return (int)GetValue(FirstProperty); }
            set { SetValue(FirstProperty, value); }
        }

        [Description("현재 페이지")]
        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        [Description("현재 페이지 Min")]
        public int CurrentMin
        {
            get { return (int)GetValue(CurrentMinProperty); }
            set { SetValue(CurrentMinProperty, value); }
        }

        [Description("현재 페이지 Max")]
        public int CurrentMax
        {
            get { return (int)GetValue(CurrentMaxProperty); }
            set { SetValue(CurrentMaxProperty, value); }
        }

        [Description("마지막 페이지")]
        public int Last
        {
            get { return (int)GetValue(LastProperty); }
            set { SetValue(LastProperty, value); }
        }

        [Description("토탈 아이템 갯수")]
        public int TotalCount
        {
            get { return (int)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }

        [Description("그리드에 포함될 아이템 갯수")]
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        [Description("페이지 사이즈 컬렉션 표시 여부")]
        public Visibility PageSizesVisibility
        {
            get { return (Visibility)GetValue(PageSizesVisibilityProperty); }
            set { SetValue(PageSizesVisibilityProperty, value); }
        }

        [Description("페이지 사이즈 컬렉션")]
        public Paging PageSizesCollection
        {
            get { return (Paging)GetValue(PageSizesCollectionProperty); }
            set { SetValue(PageSizesCollectionProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether HasPreviousPage.
        /// </summary>
        /// <value>
        /// The has previous page.
        /// </value>
        public bool HasPreviousPage
        {
            get { return BtnFirst.IsEnabled; }
            internal set { BtnFirst.IsEnabled = BtnPrevious.IsEnabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether HasNextPage.
        /// </summary>
        /// <value>
        /// The has next page.
        /// </value>
        public bool HasNextPage
        {
            get { return BtnLast.IsEnabled; }
            internal set { BtnLast.IsEnabled = BtnNext.IsEnabled = value; }
        }
        #endregion //Properties

        #region Language
        /// <summary>
        /// Text: 전체
        /// </summary>
        public String LanguageTotal
        {
            get { return (String)GetValue(LanguageTotalProperty); }
            set { SetValue(LanguageTotalProperty, value); }
        }
        /// <summary>
        /// Text: 건
        /// </summary>
        public String LanguageCount
        {
            get { return (String)GetValue(LanguageCountProperty); }
            set { SetValue(LanguageCountProperty, value); }
        }
        /// <summary>
        /// Text: 처음
        /// </summary>
        public String LanguageFirst
        {
            get { return (String)GetValue(LanguageFirstProperty); }
            set { SetValue(LanguageFirstProperty, value); }
        }
        /// <summary>
        /// Text: 이전
        /// </summary>
        public String LanguagePrevious
        {
            get { return (String)GetValue(LanguagePreviousProperty); }
            set { SetValue(LanguagePreviousProperty, value); }
        }
        /// <summary>
        /// Text: 다음
        /// </summary>
        public String LanguageNext
        {
            get { return (String)GetValue(LanguageNextProperty); }
            set { SetValue(LanguageNextProperty, value); }
        }
        /// <summary>
        /// Text: 마지막
        /// </summary>
        public String LanguageLast
        {
            get { return (String)GetValue(LanguageLastProperty); }
            set { SetValue(LanguageLastProperty, value); }
        }
        /// <summary>
        /// Text: 페이지 이동
        /// </summary>
        public String LanguageMovePage
        {
            get { return (String)GetValue(LanguageMovePageProperty); }
            set { SetValue(LanguageMovePageProperty, value); }
        }
        #endregion //SetLanguage

        #region Property Event

        #region CurrentPage

        private static object OnCurrentPage_Core(DependencyObject d, object baseValue)
        {
            return (int)baseValue;
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagingControl)?.OnCurrentPageChanged(e.OldValue, e.NewValue);
        }

        private void OnCurrentPageChanged(object p1, object p2)
        {
            //this.TxtCurrPage.Text = p2.ToString();
            //SetBtnEnableChanged();
        }
        #endregion //CurrentPage

        #region OnTotalCount
        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.Property.Name.Equals("TotalCount"))
        //        SetBtnEnableChanged();
        //    base.OnPropertyChanged(e);
        //}

        private static object OnTotalCount_Core(DependencyObject d, object baseValue)
        {
            (d as PagingControl)?.SetButtonEnableChanged();
            return (int)baseValue;
        }

        private static void OnTotalCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagingControl)?.OnTotalCountChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual void OnTotalCountChanged(int oldValue, int newValue)
        {
            //CurrentPage = (TotalCount > 0) ? 1 : 0;
            Last = ((newValue % PageSize) == 0) ? (newValue / PageSize) : (newValue / PageSize) + 1;

            if (CurrentPage <= 0)
                CurrentPage = 1;

            if (Last <= 0)
                Last = 1;

            CurrentMin = 1;
            CurrentMax = Last;

            //this.TxtCurrPage.Text = (CurrentPage > 0) ? CurrentPage.ToString() : "1";
            //this.TxtCurrPage.Min = 1;
            //this.TxtCurrPage.Max = Last;
            //this.TxtLast.Text = (Last > 0) ? Last.ToString() : "1";

            SetButtonEnableChanged();
        }
        #endregion //OnTotalCount

        #region OnPageSizesVisibility
        private static void OnPageSizesVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagingControl)?.OnPageSizesVisibilityChanged((Visibility)e.OldValue, (Visibility)e.NewValue);
        }

        protected virtual void OnPageSizesVisibilityChanged(Visibility oldValue, Visibility newValue)
        {
            this.cbPageSize.Visibility = newValue;
        }
        #endregion //OnPageSizesVisibility

        #region OnPageSizeCollection
        private static void OnPageSizesCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PagingControl)?.OnPageSizesCollectionChanged((Paging)e.OldValue, (Paging)e.NewValue);
        }

        private void OnPageSizesCollectionChanged(Paging OldValue, Paging NewValue)
        {
            if (NewValue != null)
            {
                //Binding ItemsSourceBinding = new Binding();
                //ItemsSourceBinding.Mode = BindingMode.TwoWay;
                //ItemsSourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ////ItemsSourceBinding.Source = chart;
                //ItemsSourceBinding.Path = new PropertyPath("PageSizesCollection");
                //this.cbPageSize.SetBinding(ComboBox.ItemsSourceProperty, ItemsSourceBinding);
                this.cbPageSize.ItemsSource = NewValue;
                this.cbPageSize.SelectedIndex = 0;
            }
        }
        #endregion //OnPageSizeCollection

        #endregion //Property Event

        #region ICommand
        public ICommand KeyEnterCommand { get; private set; }
        public ICommand KeyTabCommand { get; private set; }
        #endregion //ICommand



        static PagingControl()
        {
            Type owner = typeof(PagingControl);
            FirstProperty = DependencyProperty.Register(nameof(First), typeof(int), owner, new PropertyMetadata(1));
            CurrentPageProperty = DependencyProperty.Register(nameof(CurrentPage), typeof(int), owner, new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentPageChanged, OnCurrentPage_Core, false, UpdateSourceTrigger.PropertyChanged));
            CurrentMinProperty = DependencyProperty.Register(nameof(CurrentMin), typeof(int), owner, new FrameworkPropertyMetadata());
            CurrentMaxProperty = DependencyProperty.Register(nameof(CurrentMax), typeof(int), owner, new FrameworkPropertyMetadata());
            LastProperty = DependencyProperty.Register(nameof(Last), typeof(int), owner, new PropertyMetadata(1));
            TotalCountProperty = DependencyProperty.Register(nameof(TotalCount), typeof(int), owner, new PropertyMetadata(0, new PropertyChangedCallback(OnTotalCountChanged), new CoerceValueCallback(OnTotalCount_Core)));
            PageSizeProperty = DependencyProperty.Register(nameof(PageSize), typeof(int), owner, new PropertyMetadata(PAGE_COUNT));
            PageSizesVisibilityProperty = DependencyProperty.Register(nameof(PageSizesVisibility), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible, OnPageSizesVisibilityChanged));
            PageSizesCollectionProperty = DependencyProperty.Register(nameof(PageSizesCollection), typeof(Paging), owner, new PropertyMetadata(null, OnPageSizesCollectionChanged));

            LanguageTotalProperty = DependencyProperty.Register(nameof(LanguageTotal), typeof(String), owner, new PropertyMetadata(Language_Total));
            LanguageCountProperty = DependencyProperty.Register(nameof(LanguageCount), typeof(String), owner, new PropertyMetadata(Language_Count));
            LanguageFirstProperty = DependencyProperty.Register(nameof(LanguageFirst), typeof(String), owner, new PropertyMetadata(Language_Firs));
            LanguagePreviousProperty = DependencyProperty.Register(nameof(LanguagePrevious), typeof(String), owner, new PropertyMetadata(Language_Prev));
            LanguageNextProperty = DependencyProperty.Register(nameof(LanguageNext), typeof(String), owner, new PropertyMetadata(Language_Next));
            LanguageLastProperty = DependencyProperty.Register(nameof(LanguageLast), typeof(String), owner, new PropertyMetadata(Language_Last));
            LanguageMovePageProperty = DependencyProperty.Register(nameof(LanguageMovePage), typeof(String), owner, new PropertyMetadata(Language_MovePage));
        }
        public PagingControl()
        {
            InitializeComponent();

            AddEvent();

            this.HasNextPage = false;
            this.HasPreviousPage = false;

            KeyEnterCommand = new RelayCommand(OnKenEnter);
            KeyTabCommand = new RelayCommand(OnKenTab);
        }
        ~PagingControl()
        {
            RmoveEvent();
        }



        #region <Events>
        private void ComboBoxPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox cbo))
                return;

            try
            {
                var cboItem = (cbo.SelectedItem as KeyValuePair<String, String>?);
                if (cboItem == null) return;
                PageSize = Convert.ToInt32(cboItem.GetValueOrDefault().Key);

                if (TotalCount > 0)
                    Last = (TotalCount % PageSize) == 0 ? (TotalCount / PageSize) : (TotalCount / PageSize) + 1;
                else
                    Last = 1;

                //TxtLast.Text = Last.ToString();
                CurrentPage = First;
                CurrentMin = 1;
                CurrentMax = Last;

                //this.TxtCurrPage.Min = 1;
                //this.TxtCurrPage.Max = Last;

                //TxtCurrPage.Text = CurrentPage.ToString();

                PageSizeChanged?.Invoke(PageSize);
            }
            catch (Exception) { }
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button))
                return;

            switch (button.Name.ToString())
            {
                case "BtnFirst":
                    CurrentPage = 1;
                    break;
                case "BtnPrevious":
                    CurrentPage = (First < CurrentPage) ? CurrentPage - 1 : 1;
                    break;
                case "BtnNext":
                    CurrentPage = (Last > CurrentPage) ? CurrentPage + 1 : Last;
                    break;
                case "BtnLast":
                    CurrentPage = Last;
                    break;
                default:
                    break;
            }
            PageChanged?.Invoke(this, CurrentPage);

            //this.TxtCurrPage.Text = CurrentPage.ToString();
            SetButtonEnableChanged();
        }

        private void TxtCurrPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (sender as NumericTextBox);
            if (textBox == null) return;

            if (int.TryParse(textBox.Text, out int InText))
            {
                if (textBox.Text.Equals(CurrentPage.ToString())) return;

                if ((First <= InText) && (InText <= Last))
                    CurrentPage = InText;
                else
                {
                    //if (InText == 0) return;
                    if (First > InText)
                        textBox.Text = First.ToString();
                    else if (Last < InText)
                        textBox.Text = Last.ToString();
                }
            }
        }

        //private void TxtCurrPage_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    var textBox = sender as TMSNumericTextBox;
        //    if (textBox == null) return;

        //    if (e.Key == Key.Delete || e.Key == Key.Back) return;

        //    if (e.Key == Key.Tab)
        //        KeysEnter.FocusMove(FocusNavigationDirection.Next);
        //}

        //private void TxtCurrPage_KeyDown(object sender, KeyEventArgs e)
        //{
        //    ///Mainview에서 Enter 키에 대한 inputbiding을 적용했기 때문에 아래 구문은 주석처리
        //    ///
        //    //if (e.Key == Key.Enter)
        //        //SetPageMoving();
        //}

        //private void TxtCurrPage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    var textBox = (sender as TMSNumericTextBox);
        //    if (textBox == null)
        //        return;

        //    int checkInt = 0;
        //    if (!int.TryParse(e.Text, out checkInt))
        //    {
        //        e.Handled = true;
        //        if (e.Text == "\r")
        //        {
        //            e.Handled = true;
        //            KeysEnter.KeyEvent(this, Key.Tab);
        //        }
        //    }
        //}

        /// <summary>
        /// CurrentPage TextBox 에서 Enter Key를 입력했을 때
        /// </summary>
        /// <param name="obj"></param>
        private void OnKenEnter(object obj)
        {
            SetPageMoving();
        }

        /// <summary>
        /// CurrentPage TextBox 에서 Enter Tab를 입력했을 때
        /// </summary>
        /// <param name="obj"></param>
        private void OnKenTab(object obj)
        {
            KeysEnter.FocusMove(FocusNavigationDirection.Next);
        }

        private void BtnPageMove_Click(object sender, RoutedEventArgs e)
        {
            SetPageMoving();
        }

        private void SetPageMoving()
        {
            if (int.TryParse(CurrentPage.ToString(), out int OutText))
            {
                if ((First <= OutText) && (OutText <= Last))
                {
                    CurrentPage = OutText;
                    PageChanged?.Invoke(this, CurrentPage);

                    SetButtonEnableChanged();
                }
            }
            // 들어온 값에 문제가 있으면 
            else
            {
                //Text: {0} 입력값이 잘못 되었습니다.  //Text: 확인
                //MessageBox.XmlMessageShow("M900047", ConvertLanguage.GetCodeText("T23003"), MessageBoxButton.OK, MessageBoxImage.Warning, TxtCurrPage.Text);
                CurrentPage = First;

                //TxtCurrPage.Text = First.ToString();
            }
        }

        #endregion //Events

        #region <Methods>
        private void AddEvent()
        {
            BtnFirst.Click += BtnMove_Click;
            BtnPrevious.Click += BtnMove_Click;
            BtnNext.Click += BtnMove_Click;
            BtnLast.Click += BtnMove_Click;
            cbPageSize.SelectionChanged += ComboBoxPageSize_SelectionChanged;
            TxtCurrPage.TextChanged += TxtCurrPage_TextChanged;
        }

        private void RmoveEvent()
        {
            BtnFirst.Click -= BtnMove_Click;
            BtnPrevious.Click -= BtnMove_Click;
            BtnNext.Click -= BtnMove_Click;
            BtnLast.Click -= BtnMove_Click;
            cbPageSize.SelectionChanged -= ComboBoxPageSize_SelectionChanged;
            TxtCurrPage.TextChanged -= TxtCurrPage_TextChanged;
        }

        private void SetButtonEnableChanged()
        {
            this.HasNextPage = Last > CurrentPage;
            this.HasPreviousPage = CurrentPage > 1;

            if (!this.HasNextPage && !this.HasPreviousPage)
                this.Label1.IsEnabled = false;
            else if (this.HasNextPage || this.HasPreviousPage)
                this.Label1.IsEnabled = true;
        }
        #endregion //Methods
    }
}
