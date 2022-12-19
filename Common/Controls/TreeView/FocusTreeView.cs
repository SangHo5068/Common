using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Common.Utilities;


namespace Common.Controls
{
    public class FocusTreeView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FocusTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FocusTreeViewItem;
        }
    }


    public class FocusTreeViewItem : TreeViewItem
    {
        public static readonly DependencyProperty IsSelectionFocusProperty
            = DependencyProperty.Register(nameof(IsSelectionFocus), typeof(bool), typeof(FocusTreeViewItem), new PropertyMetadata(false));

        public bool IsSelectionFocus
        {
            get { return (bool)this.GetValue(IsSelectionFocusProperty); }
            set { this.SetValue(IsSelectionFocusProperty, value); }
        }


        public FocusTreeViewItem() : base()
        {
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FocusTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FocusTreeViewItem;
        }

        /// <summary>
        /// Filter 에서 TreeViewItem 클릭 시 Checked 를 변경 하도록 한다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.IsSelectionFocus = !this.IsSelectionFocus;
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            var context = (e.OriginalSource as FrameworkElement)?.DataContext;

            if (context == null || context is TreeElementModel == false)
            {
                return;
            }

            this.SetIsOpenToFalse((TreeElementModel)context);
        }

        private void SetIsOpenToFalse<T>(T context) where T : TreeElementModel
        {
            var parentTreeView = (FocusTreeView)VisualElementHelper.FindParentControl<FocusTreeView>(this);

            var parentElements = parentTreeView.ItemsSource;

            foreach (T item in parentElements)
            {
                foreach (T child in item.Children)
                {
                    if (child.Equals(context) == false)
                    {
                        child.IsOpen = false;
                    }
                }
            }
        }
    }
}
