using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Controls
{
    /// <summary>
    /// Static class used to attach to wpf control
    /// </summary>
    public static class GridViewColumnResize
    {
        #region DependencyProperties

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached("Width", typeof(string), typeof(GridViewColumnResize),
                                                new PropertyMetadata(OnSetWidthCallback));

        public static readonly DependencyProperty GridViewColumnResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("GridViewColumnResizeBehavior",
                                                typeof(GridViewColumnResizeBehavior), typeof(GridViewColumnResize),
                                                null);

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(GridViewColumnResize),
                                                new PropertyMetadata(OnSetEnabledCallback));

        public static readonly DependencyProperty ListViewResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("ListViewResizeBehaviorProperty",
                                                typeof(ListViewResizeBehavior), typeof(GridViewColumnResize), null);

        #endregion

        public static string GetWidth(DependencyObject obj)
        {
            return (string)obj.GetValue(WidthProperty);
        }

        public static void SetWidth(DependencyObject obj, string value)
        {
            obj.SetValue(WidthProperty, value);
        }

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        #region CallBack

        private static void OnSetWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is GridViewColumn element)
            {
                GridViewColumnResizeBehavior behavior = GetOrCreateBehavior(element);
                behavior.Width = e.NewValue as string;
            }
            else
            {
                Console.Error.WriteLine("Error: Expected type GridViewColumn but found " +
                                        dependencyObject.GetType().Name);
            }
        }

        private static void OnSetEnabledCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = dependencyObject as ListView;
            if (element != null)
            {
                ListViewResizeBehavior behavior = GetOrCreateBehavior(element);
                behavior.Enabled = (bool)e.NewValue;
            }
            else
            {
                Console.Error.WriteLine("Error: Expected type ListView but found " + dependencyObject.GetType().Name);
            }
        }


        private static ListViewResizeBehavior GetOrCreateBehavior(ListView element)
        {
            if (!(element.GetValue(GridViewColumnResizeBehaviorProperty) is ListViewResizeBehavior behavior))
            {
                behavior = new ListViewResizeBehavior(element);
                element.SetValue(ListViewResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        private static GridViewColumnResizeBehavior GetOrCreateBehavior(GridViewColumn element)
        {
            if (!(element.GetValue(GridViewColumnResizeBehaviorProperty) is GridViewColumnResizeBehavior behavior))
            {
                behavior = new GridViewColumnResizeBehavior(element);
                element.SetValue(GridViewColumnResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        #endregion

        #region Nested type: GridViewColumnResizeBehavior

        /// <summary>
        /// GridViewColumn class that gets attached to the GridViewColumn control
        /// </summary>
        public class GridViewColumnResizeBehavior
        {
            private readonly GridViewColumn _element;

            public GridViewColumnResizeBehavior(GridViewColumn element)
            {
                _element = element;
            }

            public string Width { get; set; }

            public bool IsStatic
            {
                get { return StaticWidth >= 0; }
            }

            public double StaticWidth
            {
                get
                {
                    return double.TryParse(Width, out double result) ? result : -1;
                }
            }

            public double Percentage
            {
                get
                {
                    if (!IsStatic)
                    {
                        return Mulitplier * 100;
                    }
                    return 0;
                }
            }

            public double Mulitplier
            {
                get
                {
                    if (Width == "*" || Width == "1*") return 1;
                    if (Width.EndsWith("*"))
                    {
                        if (double.TryParse(Width.Substring(0, Width.Length - 1), out double perc))
                        {
                            return perc;
                        }
                    }
                    return 1;
                }
            }

            public void SetWidth(double allowedSpace, double totalPercentage)
            {
                if (IsStatic)
                {
                    _element.Width = StaticWidth;
                }
                else
                {
                    double width = allowedSpace * (Percentage / totalPercentage);
                    _element.Width = width;
                }
            }
        }

        #endregion

        #region Nested type: ListViewResizeBehavior

        /// <summary>
        /// ListViewResizeBehavior class that gets attached to the ListView control
        /// </summary>
        public class ListViewResizeBehavior
        {
            private const int Margin = 25;
            private const long RefreshTime = Timeout.Infinite;
            private const long Delay = 500;

            private readonly ListView _element;
            private readonly Timer _timer;

            public ListViewResizeBehavior(ListView element)
            {
                _element = element ?? throw new ArgumentNullException("element");
                element.Loaded += OnLoaded;

                // Action for resizing and re-enable the size lookup
                // This stops the columns from constantly resizing to improve performance
                Action resizeAndEnableSize = () =>
                {
                    Resize();
                    _element.SizeChanged += OnSizeChanged;
                };
                _timer = new Timer(x => Application.Current.Dispatcher.BeginInvoke(resizeAndEnableSize), null, Delay,
                                   RefreshTime);
            }

            public bool Enabled { get; set; }


            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                _element.SizeChanged += OnSizeChanged;
            }

            private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (e.WidthChanged)
                {
                    _element.SizeChanged -= OnSizeChanged;
                    _timer.Change(Delay, RefreshTime);
                }
            }

            private void Resize()
            {
                if (Enabled)
                {
                    double totalWidth = _element.ActualWidth;
                    if (_element.View is GridView gv)
                    {
                        double allowedSpace = totalWidth - GetAllocatedSpace(gv);
                        allowedSpace -= Margin;
                        double totalPercentage = GridViewColumnResizeBehaviors(gv).Sum(x => x.Percentage);
                        foreach (GridViewColumnResizeBehavior behavior in GridViewColumnResizeBehaviors(gv))
                        {
                            behavior.SetWidth(allowedSpace, totalPercentage);
                        }
                    }
                }
            }

            private static IEnumerable<GridViewColumnResizeBehavior> GridViewColumnResizeBehaviors(GridView gv)
            {
                foreach (GridViewColumn t in gv.Columns)
                {
                    if (t.GetValue(GridViewColumnResizeBehaviorProperty) is GridViewColumnResizeBehavior gridViewColumnResizeBehavior)
                    {
                        yield return gridViewColumnResizeBehavior;
                    }
                }
            }

            private static double GetAllocatedSpace(GridView gv)
            {
                double totalWidth = 0;
                foreach (GridViewColumn t in gv.Columns)
                {
                    if (t.GetValue(GridViewColumnResizeBehaviorProperty) is GridViewColumnResizeBehavior gridViewColumnResizeBehavior)
                    {
                        if (gridViewColumnResizeBehavior.IsStatic)
                        {
                            totalWidth += gridViewColumnResizeBehavior.StaticWidth;
                        }
                    }
                    else
                    {
                        totalWidth += t.ActualWidth;
                    }
                }
                return totalWidth;
            }
        }

        #endregion
    }

    public class GridViewEx : GridView
    {
        #region DPA HorizontalContainerAlignment

        public static readonly DependencyProperty HorizontalContainerAlignmentProperty =
            DependencyProperty.RegisterAttached("HorizontalContainerAlignment", typeof(HorizontalAlignment), typeof(GridViewEx), new UIPropertyMetadata(default(HorizontalAlignment), HorizontalContainerAlignmentChanged));

        public static HorizontalAlignment GetHorizontalContainerAlignment(DependencyObject obj)
        {
            return (HorizontalAlignment)obj.GetValue(HorizontalContainerAlignmentProperty);
        }

        public static void SetHorizontalContainerAlignment(DependencyObject obj, HorizontalAlignment value)
        {
            obj.SetValue(HorizontalContainerAlignmentProperty, value);
        }

        private static void HorizontalContainerAlignmentChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var current = sender as DependencyObject;
            int levels = 10;

            while (current != null && levels-- > 0)
            {
                var next = VisualTreeHelper.GetParent(current);
                if (next is GridViewRowPresenter)
                {
                    if (current is ContentPresenter cp)
                        cp.HorizontalAlignment = (HorizontalAlignment)args.NewValue;
                    break;
                }
                current = next;
            }
        }

        #endregion //DPA HorizontalContainerAlignment

        #region DPA VerticalContainerAlignment

        public static readonly DependencyProperty VerticalContainerAlignmentProperty =
            DependencyProperty.RegisterAttached("VerticalContainerAlignment", typeof(VerticalAlignment), typeof(GridViewEx), new UIPropertyMetadata(default(VerticalAlignment), VerticalContainerAlignmentChanged));

        public static VerticalAlignment GetVerticalContainerAlignment(DependencyObject obj)
        {
            return (VerticalAlignment)obj.GetValue(VerticalContainerAlignmentProperty);
        }

        public static void SetVerticalContainerAlignment(DependencyObject obj, VerticalAlignment value)
        {
            obj.SetValue(VerticalContainerAlignmentProperty, value);
        }

        private static void VerticalContainerAlignmentChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var current = sender as DependencyObject;
            int levels = 10;

            while (current != null && levels-- > 0)
            {
                var next = VisualTreeHelper.GetParent(current);
                if (next is GridViewRowPresenter)
                {
                    if (current is ContentPresenter cp)
                        cp.VerticalAlignment = (VerticalAlignment)args.NewValue;
                    break;
                }
                current = next;
            }
        }

        #endregion //DPA VerticalContainerAlignment

        static GridViewEx()
        {
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(TMSGridViewEx), new FrameworkPropertyMetadata { DefaultValue = 20 });
        }

        //protected override void PrepareItem(ListViewItem item)
        //{
        //    foreach (GridViewColumn column in Columns)
        //    {
        //        //setting NaN for the column width automatically determines the required width enough to hold the content completely.
        //        //if column width was set to NaN already, set it ActualWidth temporarily and set to NaN. This raises the property change event and re computes the width.
        //        if (double.IsNaN(column.Width)) column.Width = column.ActualWidth;
        //        column.Width = double.NaN;
        //    }
        //    base.PrepareItem(item);
        //}
    }
}
