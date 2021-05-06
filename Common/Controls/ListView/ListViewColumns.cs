using System;
using System.Windows;
using System.Windows.Controls;

namespace Common.Controls
{
    public abstract class LayoutColumn
    {
        public const string COLUMN = "column";

        // ----------------------------------------------------------------------
        protected static bool HasPropertyValue(GridViewColumn column, DependencyProperty dp)
        {
            if (column == null)
            {
                throw new ArgumentNullException(COLUMN);
            }
            object value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return true;
            }

            return false;
        } // HasPropertyValue

        // ----------------------------------------------------------------------
        protected static double? GetColumnWidth(GridViewColumn column, DependencyProperty dp)
        {
            if (column == null)
            {
                throw new ArgumentNullException(COLUMN);
            }
            object value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return (double)value;
            }

            return null;
        } // GetColumnWidth
    } // class LayoutColumn

    public sealed class FixedColumn : LayoutColumn
    {
        // ----------------------------------------------------------------------
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached(
                "Width",
                typeof(double),
                typeof(FixedColumn));

        // ----------------------------------------------------------------------
        private FixedColumn()
        {
        } // FixedColumn

        // ----------------------------------------------------------------------
        public static double GetWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(WidthProperty);
        } // GetWidth

        // ----------------------------------------------------------------------
        public static void SetWidth(DependencyObject obj, double width)
        {
            obj.SetValue(WidthProperty, width);
        } // SetWidth

        // ----------------------------------------------------------------------
        public static bool IsFixedColumn(GridViewColumn column)
        {
            if (column == null)
            {
                return false;
            }
            return HasPropertyValue(column, WidthProperty);
        } // IsFixedColumn

        // ----------------------------------------------------------------------
        public static double? GetFixedWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, WidthProperty);
        } // GetFixedWidth

        // ----------------------------------------------------------------------
        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double width)
        {
            SetWidth(gridViewColumn, width);
            return gridViewColumn;
        } // ApplyWidth

    } // class FixedColumn

    public sealed class ProportionalColumn : LayoutColumn
    {
        // ----------------------------------------------------------------------
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached(
                "Width",
                typeof(double),
                typeof(ProportionalColumn));

        // ----------------------------------------------------------------------
        private ProportionalColumn()
        {
        } // ProportionalColumn

        // ----------------------------------------------------------------------
        public static double GetWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(WidthProperty);
        } // GetWidth

        // ----------------------------------------------------------------------
        public static void SetWidth(DependencyObject obj, double width)
        {
            obj.SetValue(WidthProperty, width);
        } // SetWidth

        // ----------------------------------------------------------------------
        public static bool IsProportionalColumn(GridViewColumn column)
        {
            if (column == null)
            {
                return false;
            }
            return HasPropertyValue(column, WidthProperty);
        } // IsProportionalColumn

        // ----------------------------------------------------------------------
        public static double? GetProportionalWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, WidthProperty);
        } // GetProportionalWidth

        // ----------------------------------------------------------------------
        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double width)
        {
            SetWidth(gridViewColumn, width);
            return gridViewColumn;
        } // ApplyWidth

    } // class ProportionalColumn

    public sealed class RangeColumn : LayoutColumn
    {
        // ----------------------------------------------------------------------
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(double),
                typeof(RangeColumn));

        // ----------------------------------------------------------------------
        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.RegisterAttached(
                "MaxWidth",
                typeof(double),
                typeof(RangeColumn));

        // ----------------------------------------------------------------------
        public static readonly DependencyProperty IsFillColumnProperty =
            DependencyProperty.RegisterAttached(
                "IsFillColumn",
                typeof(bool),
                typeof(RangeColumn));

        // ----------------------------------------------------------------------
        private RangeColumn()
        {
        } // RangeColumn

        // ----------------------------------------------------------------------
        public static double GetMinWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MinWidthProperty);
        } // GetMinWidth

        // ----------------------------------------------------------------------
        public static void SetMinWidth(DependencyObject obj, double minWidth)
        {
            obj.SetValue(MinWidthProperty, minWidth);
        } // SetMinWidth

        // ----------------------------------------------------------------------
        public static double GetMaxWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MaxWidthProperty);
        } // GetMaxWidth

        // ----------------------------------------------------------------------
        public static void SetMaxWidth(DependencyObject obj, double maxWidth)
        {
            obj.SetValue(MaxWidthProperty, maxWidth);
        } // SetMaxWidth

        // ----------------------------------------------------------------------
        public static bool GetIsFillColumn(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFillColumnProperty);
        } // GetIsFillColumn

        // ----------------------------------------------------------------------
        public static void SetIsFillColumn(DependencyObject obj, bool isFillColumn)
        {
            obj.SetValue(IsFillColumnProperty, isFillColumn);
        } // SetIsFillColumn

        // ----------------------------------------------------------------------
        public static bool IsRangeColumn(GridViewColumn column)
        {
            if (column == null)
            {
                return false;
            }
            return
                HasPropertyValue(column, MinWidthProperty) ||
                HasPropertyValue(column, MaxWidthProperty) ||
                HasPropertyValue(column, IsFillColumnProperty);
        } // IsRangeColumn

        // ----------------------------------------------------------------------
        public static double? GetRangeMinWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, MinWidthProperty);
        } // GetRangeMinWidth

        // ----------------------------------------------------------------------
        public static double? GetRangeMaxWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, MaxWidthProperty);
        } // GetRangeMaxWidth

        // ----------------------------------------------------------------------
        public static bool? GetRangeIsFillColumn(GridViewColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(COLUMN);
            }
            object value = column.ReadLocalValue(IsFillColumnProperty);
            if (value != null && value.GetType() == IsFillColumnProperty.PropertyType)
            {
                return (bool)value;
            }

            return null;
        } // GetRangeIsFillColumn

        // ----------------------------------------------------------------------
        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double minWidth,
            double width, double maxWidth)
        {
            return ApplyWidth(gridViewColumn, minWidth, width, maxWidth, false);
        } // ApplyWidth

        // ----------------------------------------------------------------------
        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double minWidth,
            double width, double maxWidth, bool isFillColumn)
        {
            SetMinWidth(gridViewColumn, minWidth);
            gridViewColumn.Width = width;
            SetMaxWidth(gridViewColumn, maxWidth);
            SetIsFillColumn(gridViewColumn, isFillColumn);
            return gridViewColumn;
        } // ApplyWidth
    } // class RangeColumn
}
