using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using Common.Utilities;

namespace Common.Controls
{
    public enum ItemSize
    {
        None,
        Uniform,
        UniformStretchToFit,
        /// <summary>
        /// 마지막 Child Item에는 Width="*"로 설정함
        /// </summary>
        LastWidthStar
    }


    [TemplatePart(Name = PART_CONTROL_AREA, Type = typeof(Grid))]
    //[TemplatePart(Name = PART_BACKGROUND_AREA, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_CONTENT_AREA, Type = typeof(Grid))]
    //public class SearchPanel : ContentControl
    public class SearchPanel : ItemsControl
    {
        private const string PART_CONTROL_AREA = "PART_CONTROL_AREA";
        //private const string PART_BACKGROUND_AREA = "PART_BACKGROUND_AREA";
        private const string PART_CONTENT_AREA = "PART_CONTENT_AREA";
        private const string PART_SEARCHBUTTON_AREA = "PART_SEARCHBUTTON_AREA";

        #region DependencyProperty
        public static readonly DependencyProperty ShowSearchPanelProperty;
        public static readonly DependencyProperty SearchButtonAreaTemplateProperty;
        public static readonly DependencyProperty VisibilityInitialProperty;
        #endregion //DependencyProperty

        #region Private
        //private Grid controlPanel;
        //private Rectangle backgroundRectangle;
        private Grid contentPanel;
        private Grid searchButtonArea;
        private ItemsControl root;
        public Size SearchButtonAreaOrignalSize;
        #endregion //Private

        #region Properties

        /// <summary>
        /// SearchPanel 표시 여부
        /// </summary>
        public bool ShowSearchPanel
        {
            get { return (bool)GetValue(ShowSearchPanelProperty); }
            set { SetValue(ShowSearchPanelProperty, value); }
        }

        /// <summary>
        /// Search Button Area Template
        /// </summary>
        public ControlTemplate SearchButtonAreaTemplate
        {
            get { return (ControlTemplate)GetValue(SearchButtonAreaTemplateProperty); }
            set { SetValue(SearchButtonAreaTemplateProperty, value); }
        }

        /// <summary>
        /// Initial Button Visibility
        /// </summary>
        public Visibility VisibilityInitial
        {
            get { return (Visibility)GetValue(VisibilityInitialProperty); }
            set { SetValue(VisibilityInitialProperty, value); }
        }

        #endregion //Properties

        #region Property Events

        private static void ShowSearchPanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SearchPanel)?.RaiseShowSearchPanelPropertyChanged(e);
        }

        private void RaiseShowSearchPanelPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            RoutedEventArgs args;
            if ((bool)e.NewValue)
                args = new RoutedEventArgs(ShowEvent);
            else
                args = new RoutedEventArgs(HideEvent);
            RaiseEvent(args);
        }

        #endregion //Property Events

        #region Show Routed Event

        public static readonly RoutedEvent ShowEvent;

        public event RoutedEventHandler Show
        {
            add { AddHandler(ShowEvent, value); }
            remove { RemoveHandler(ShowEvent, value); }
        }

        #endregion //Show Routed Event

        #region Hide Routed Event

        public static readonly RoutedEvent HideEvent;

        public event RoutedEventHandler Hide
        {
            add { AddHandler(HideEvent, value); }
            remove { RemoveHandler(HideEvent, value); }
        }

        #endregion //Hide Routed Event



        static SearchPanel()
        {
            Type owner = typeof(SearchPanel);

            ShowSearchPanelProperty = DependencyProperty.Register(nameof(ShowSearchPanel), typeof(bool), owner, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ShowSearchPanelPropertyChanged));
            SearchButtonAreaTemplateProperty = DependencyProperty.Register(nameof(SearchButtonAreaTemplate), typeof(ControlTemplate), owner, new PropertyMetadata(null));
            VisibilityInitialProperty = DependencyProperty.Register(nameof(VisibilityInitial), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));

            ShowEvent = EventManager.RegisterRoutedEvent(nameof(Show), RoutingStrategy.Bubble, typeof(RoutedEventHandler), owner);
            HideEvent = EventManager.RegisterRoutedEvent(nameof(Hide), RoutingStrategy.Bubble, typeof(RoutedEventHandler), owner);
        }
        public SearchPanel()
        {
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            //this.MouseLeftButtonDown += OnMouseLeftButtonDown;
            this.SizeChanged += OnSizeChanged;
        }



        #region Override
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //this.controlPanel = GetTemplateChild(PART_CONTROL_AREA) as Grid;
            //this.backgroundRectangle = GetTemplateChild(PART_BACKGROUND_AREA) as Rectangle;
            this.contentPanel = GetTemplateChild(PART_CONTENT_AREA) as Grid;
            try
            {
                if (this.contentPanel != null)
                {
                    root = this.contentPanel.FindName("Root") as ItemsControl;
                    if (root != null)
                    {
                        //this.searchButtonArea = GetTemplateChild(PART_SEARCHBUTTON_AREA) as ContentControl;
                        this.searchButtonArea = GetTemplateChild(PART_SEARCHBUTTON_AREA) as Grid;
                        this.contentPanel.Children.Remove(this.searchButtonArea);

                        ItemCollection ic = this.Items;
                        if (this.searchButtonArea != null)
                        {
                            Panel rightpanel = (searchButtonArea.Children.Count > 0) ? searchButtonArea.Children.OfType<Panel>().FirstOrDefault() : null;
                            ButtonBase btnSearchSub = (SearchButtonAreaTemplate != null) ? SearchButtonAreaTemplate.LoadContent() as ButtonBase : null;
                            if (rightpanel != null && btnSearchSub != null)
                                rightpanel.Children.Insert(0, btnSearchSub);
                            ic.Add(this.searchButtonArea);
                        }
                        root.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = ic });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion //Override

        #region Events

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.searchButtonArea != null)
                {
                    Point Lastlocation = this.searchButtonArea.TransformToAncestor(this).Transform(new Point(0, 0));
                    double width = this.searchButtonArea.ActualWidth;
                    SearchButtonAreaOrignalSize = this.searchButtonArea.DesiredSize;

                    this.BeginInit();
                    if (this.ActualHeight >= this.searchButtonArea.ActualHeight)
                    {
                        width = this.ActualWidth - Lastlocation.X;
                        this.searchButtonArea.Width = width;
                    }
                    this.EndInit();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, ex.ToString());
            }
            finally
            {
                this.UpdateLayout();
            }
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.Unloaded -= OnUnloaded;
            //this.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            //this.SizeChanged -= OnSizeChanged;
        }

        //List<DependencyObject> hitResultsList = new List<DependencyObject>();
        //void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    Point pt = e.GetPosition((UIElement)sender);

        //    hitResultsList.Clear();
        //    VisualTreeHelper.HitTest(controlPanel, null, new HitTestResultCallback(OnHitTestResult), new PointHitTestParameters(pt));
        //    //if (hitResultsList.Count == 1)
        //    //    this.ShowSearchPanel = false;
        //}

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Point Lastlocation = searchButtonArea.TransformToAncestor(this).Transform(new Point(0, 0));
                if (searchButtonArea.Children.Count <= 0) return;

                Panel rightpanel = searchButtonArea.Children.OfType<Panel>().FirstOrDefault();
                if (rightpanel != null)
                {
                    root.BeginInit();
                    if (Lastlocation.X > 0)
                    {
                        double width = root.ActualWidth - Lastlocation.X;
                        if (width > 0)
                            searchButtonArea.Width = width;
                        else searchButtonArea.Width = rightpanel.Width;
                    }
                    else if (Lastlocation.X == 0)
                    {
                        ContentControl PrivItem = root.Items[root.Items.Count - 2] as ContentControl;
                        Point Prevlocation = PrivItem.TransformToAncestor(this).Transform(new Point(0, 0));
                        if (Prevlocation.X + PrivItem.ActualWidth + rightpanel.ActualWidth < root.ActualWidth)
                        {
                            double width = searchButtonArea.ActualWidth;
                            width = root.ActualWidth - (Prevlocation.X + PrivItem.ActualWidth);
                            if (width > 0)
                                searchButtonArea.Width = width;
                        }
                        else searchButtonArea.Width = root.ActualWidth;
                    }
                    root.EndInit();
                    root.UpdateLayout();
                    this.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion //Events


        #region Methods

        //private HitTestResultBehavior OnHitTestResult(HitTestResult result)
        //{
        //    hitResultsList.Add(result.VisualHit);

        //    return HitTestResultBehavior.Continue;
        //}

        #endregion //Methods
    }

    public class SearchWrapPanel : WrapPanel
    {
        #region Define

        #region ItemSize
        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register("ItemSize", typeof(ItemSize), typeof(SearchWrapPanel), new FrameworkPropertyMetadata(default(ItemSize), FrameworkPropertyMetadataOptions.AffectsMeasure, ItemSizeChanged));

        private static void ItemSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var SearchWrapPanel = sender as SearchWrapPanel;
            if (SearchWrapPanel != null)
            {
                if (SearchWrapPanel.Orientation == Orientation.Horizontal)
                    SearchWrapPanel.ItemWidth = double.NaN;
                else
                    SearchWrapPanel.ItemHeight = double.NaN;
            }
        }

        public ItemSize ItemSize
        {
            get { return (ItemSize)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }
        #endregion //ItemSize

        #region Parent
        public new Object Parent
        {
            get { return (Object)GetValue(ParentProperty); }
            set { SetValue(ParentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchButtonAreaTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentProperty =
            DependencyProperty.Register("Parent", typeof(Object), typeof(SearchWrapPanel), new PropertyMetadata(null));
        #endregion //SearchItems

        #endregion //Define


        #region Initial
        static SearchWrapPanel() { }
        public SearchWrapPanel()
            : base()
        {
        }
        #endregion //Initial


        #region Override
        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var mode = ItemSize;
            try
            {
                if (Children.Count > 0)
                {
                    if (ItemSize == ItemSize.LastWidthStar)
                    {
                    }
                    else if (mode != ItemSize.None)
                    {
                        bool stretchToFit = mode == ItemSize.UniformStretchToFit;
                        if (Orientation == Orientation.Horizontal)
                        {
                            double totalWidth = availableSize.Width;

                            ItemWidth = 0.0;
                            foreach (UIElement el in Children)
                            {
                                el.Measure(availableSize);
                                Size next = el.DesiredSize;
                                if (!(Double.IsInfinity(next.Width) || Double.IsNaN(next.Width)))
                                    ItemWidth = Math.Max(next.Width, ItemWidth);
                            }

                            if (stretchToFit)
                            {
                                if (!double.IsNaN(ItemWidth) && !double.IsInfinity(ItemWidth) && ItemWidth > 0)
                                {
                                    var itemsPerRow = (int)(totalWidth / ItemWidth);
                                    if (itemsPerRow > 0)
                                        ItemWidth = totalWidth / itemsPerRow;
                                }
                            }
                        }
                        else
                        {
                            double totalHeight = availableSize.Height;

                            ItemHeight = 0.0;
                            foreach (UIElement el in Children)
                            {
                                el.Measure(availableSize);
                                Size next = el.DesiredSize;
                                if (!(Double.IsInfinity(next.Height) || Double.IsNaN(next.Height)))
                                {
                                    ItemHeight = Math.Max(next.Height, ItemHeight);
                                }
                            }

                            if (stretchToFit)
                            {
                                if (!double.IsNaN(ItemHeight) && !double.IsInfinity(ItemHeight) && ItemHeight > 0)
                                {
                                    var itemsPerColumn = (int)(totalHeight / ItemHeight);
                                    if (itemsPerColumn > 0)
                                    {
                                        ItemHeight = totalHeight / itemsPerColumn;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, ex.ToString());
            }
            return base.MeasureOverride(availableSize);
        }
        #endregion //override
    }
}
