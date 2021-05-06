using System.Collections.ObjectModel;

namespace Common.Controls
{
    public enum TreeViewType
    {
        None,
        GisElementList,
        GisFilter
    }

    public enum UpdateSource
    {
        None,
        Parent,
        Child,
    }

    public enum CheckedType
    {
        Checked,
        UnChecked,
        Indeterminate
    }

    public interface IGISElement
    {
        string _IW_GE_ID { get; set; }
        string _IW_GE_NAME { get; set; }
        string _IW_GE_SHAPETYPE { get; set; }
        string _IW_GE_VALUE { get; set; }
        double _IW_GE_ROTATE { get; set; }
        double _IW_GE_LNG { get; set; }
        double _IW_GE_LAT { get; set; }
    }

    public interface ITree
    {
        TreeViewType TreeViewType { get; set; }

        UpdateSource UpdateSource { get; set; }

        string Name { get; set; }

        CheckedType IsChecked { get; set; }

        bool IsExpanded { get; set; }

        bool IsVisible { get; set; }

        void SetChildrenCheckStatus(CheckedType isChecked);

        void UpdateCheckStatusWithChildren();

        void UpdateParentCheckStatus();
    }

    public interface ITree<T> where T : ITree
    {
        T Parent { get; set; }

        ObservableCollection<T> Children { get; set; }
    }
}
