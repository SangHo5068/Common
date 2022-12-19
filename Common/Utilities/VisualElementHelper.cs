using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace Common.Utilities
{
    public class VisualElementHelper
    {
        public static DependencyObject FindChildControl<T>(object element)
        {
            var control = element as DependencyObject;
            if (control == null)
            {
                return null;
            }

            var childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (var i = 0; i < childNumber; i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                if (child != null && child is T)
                {
                    return child;
                }
                else
                {
                    var childItem = FindChildControl<T>(child);
                    if (childItem != null && childItem is T)
                    {
                        return childItem;
                    }
                }
            }
            return null;
        }

        public static DependencyObject FindVisualTreeRoot(DependencyObject d)
        {
            var current = d;
            var result = d;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                    break;

                current = LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

        public static DependencyObject FindParentControl<T>(object element)
        {
            var control = element as DependencyObject;
            if (control == null)
            {
                return null;
            }

            control = FindVisualTreeRoot(control);

            var parent = VisualTreeHelper.GetParent(control);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }
}
