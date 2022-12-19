using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

using Common.Command;
using Common.Notify;


namespace Common.Controls
{
    /// <summary>
	/// API Search Response 시 들어오는 데이터 모델
	/// </summary>
	public class BaseTreeElementModel : BindableAndDisposable, ITreeElement
    {
        #region Fields
        private string _ID = string.Empty;
        protected string _name = string.Empty;
        private string _sType = string.Empty;
        private string _value = string.Empty;
        private double _rotate = 0D;
        private double _lat = double.NaN;
        private double _lng = double.NaN;
        #endregion //Fields

        #region Properties
        public string _IW_GE_ID
        {
            get => _ID;
            set => SetValue(ref _ID, value);
        }
        public string _IW_GE_NAME
        {
            get => _name;
            set => SetValue(ref _name, value);
        }
        public string _IW_GE_SHAPETYPE
        {
            get => _sType;
            set => SetValue(ref _sType, value);
        }
        public string _IW_GE_VALUE
        {
            get => _value;
            set => SetValue(ref _value, value);
        }
        /// <summary>
        /// 회전값
        /// </summary>
        public double _IW_GE_ROTATE
        {
            get => _rotate;
            set => SetValue(ref _rotate, value);
        }
        /// <summary>
        /// 경도
        /// </summary>
        public double _IW_GE_LNG
        {
            get => _lng;
            set => SetValue(ref _lng, value);
        }
        /// <summary>
        /// 위도
        /// </summary>
        public double _IW_GE_LAT
        {
            get => _lat;
            set => SetValue(ref _lat, value);
        }
        #endregion //Properties



        public BaseTreeElementModel() { }
        public BaseTreeElementModel(ITreeElement element)
        {
            this._IW_GE_ID = element._IW_GE_ID;
            this._IW_GE_NAME = element._IW_GE_NAME;
            this._IW_GE_SHAPETYPE = element._IW_GE_SHAPETYPE;
            this._IW_GE_VALUE = element._IW_GE_VALUE;
            this._IW_GE_ROTATE = element._IW_GE_ROTATE;
            this._IW_GE_LNG = element._IW_GE_LNG;
            this._IW_GE_LAT = element._IW_GE_LAT;
        }
    }

    /// <summary>
    /// Graphic Element 가 사용할 Model
    /// </summary>
    public class TreeElementModel : BaseTreeElementModel, ITree<ITree>, ITree
    {
        private Point extentMin;
        private Point extentMax;
        private List<Point> _pointCollection;
        private bool _IsOpen = false;
        private string _GroupKey = string.Empty;

        private ITree _parent;
        private int _childrenCount;
        private ObservableCollection<ITree> _children = new ObservableCollection<ITree>();
        private TreeElementType _treeViewType = TreeElementType.Element;
        private ImageDataInfo _shapetype = null;


        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Point ExtentMin
        {
            get => extentMin;
            set => SetValue(ref this.extentMin, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public Point ExtentMax
        {
            get => extentMax;
            set => SetValue(ref this.extentMax, value);
        }

        public List<Point> PointCollection
        {
            get => _pointCollection;
            set => SetValue(ref this._pointCollection, value, OnPointCollectionChanged);
        }

        private void OnPointCollectionChanged(List<Point> oldValue, List<Point> newValue)
        {
            if (this.PointCollection.Count > 0)
            {
                var minx = this.PointCollection.Min(e => e.X);
                var miny = this.PointCollection.Min(e => e.Y);
                var maxx = this.PointCollection.Max(e => e.X);
                var maxy = this.PointCollection.Max(e => e.Y);

                this.ExtentMin = new Point(minx, miny);
                this.ExtentMax = new Point(maxx, maxy);
            }
        }

        public bool IsOpen
        {
            get => _IsOpen;
            set => SetValue(ref _IsOpen, value);
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        /// <summary>
        /// Tree Group Key
        /// </summary>
        public string GroupKey
        {
            get => _GroupKey;
            set => SetValue(ref _GroupKey, value);
        }

        #region Tree

        [XmlIgnore]
        public ITree Parent
        {
            get => _parent;
            set => SetValue(ref _parent, value);
        }

        [XmlIgnore]
        public ObservableCollection<ITree> Children
        {
            get => _children;
            set => SetValue(ref _children, value);
        }

        [XmlIgnore]
        public bool HasChildren
        {
            get { return Children.Count > 0; }
        }

        [XmlIgnore]
        public int ChildrenCount
        {
            get => _childrenCount;
            set => SetValue(ref _childrenCount, value);
        }

        [XmlIgnore]
        public TreeElementType TreeViewType
        {
            get => _treeViewType;
            set => SetValue(ref _treeViewType, value);
        }

        private UpdateSource _updateSource;
        [XmlIgnore]
        public UpdateSource UpdateSource
        {
            get => _updateSource;
            set => SetValue(ref _updateSource, value);
        }

        private CheckedType _isChecked = CheckedType.UnChecked;
        [XmlIgnore]
        public CheckedType IsChecked
        {
            get => _isChecked;
            set => SetValue(ref _isChecked, value);
        }

        private bool _isExpanded = false;
        [XmlIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetValue(ref _isExpanded, value);
        }

        private bool _IsVisible = false;
        [XmlIgnore]
        public bool IsVisible
        {
            get => _IsVisible;
            set => SetValue(ref _IsVisible, value);
        }

        #region filter
        public ImageDataInfo ShapeType
        {
            get => _shapetype;
            set => SetValue(ref _shapetype, value);
        }
        #endregion //filter


        public void SetChildrenCheckStatus(CheckedType isChecked)
        {
            this.Children?.ToList().ForEach(c =>
            {
                c.UpdateSource = UpdateSource.Parent;
                c.IsChecked = isChecked;
            });
        }

        public void UpdateCheckStatusWithChildren()
        {
            SetChildrenCheckStatus(this.IsChecked);

            this.UpdateSource = UpdateSource.None;
        }

        public void UpdateParentCheckStatus()
        {
            var childrenCount = this.Children.Count;
            var checkedCount = this.Children.Count(c => c.IsChecked != CheckedType.UnChecked);

            //Parent가 있을 경우 Parent의 Parent가 변경 될 수 있도록 Source 변경
            if (this.Parent != null)
            {
                this.Parent.UpdateSource = UpdateSource.Child;
            }

            if (checkedCount == 0)
            {
                this.IsChecked = CheckedType.UnChecked;
            }
            else if (checkedCount == childrenCount)
            {
                this.IsChecked = CheckedType.Checked;
            }
            else
            {
                this.IsChecked = CheckedType.Indeterminate;
            }

            this.UpdateSource = UpdateSource.None;
        }

        #endregion //Tree

        #endregion //Properties

        #region ICommand
        public ICommand CloseCommand { get; private set; }
        public ICommand SelectedCommand { get; private set; }
        public ICommand ClickCommand { get; private set; }
        #endregion //ICommand



        public TreeElementModel() { }
        public TreeElementModel(ITreeElement element)
            : base(element)
        {
            this.CloseCommand = new RelayCommand(OnClose);
            this.SelectedCommand = new RelayCommand(OnSelected);
        }
        public TreeElementModel(ITreeElement element, string groupKey, ImageDataInfo shapetype)
            : this(element, groupKey)
        {
            this.ShapeType = shapetype;
        }
        public TreeElementModel(ITreeElement element, string groupKey)
            : this(element)
        {
            this.GroupKey = groupKey;
        }
        ~TreeElementModel()
        {
            this.ShapeType = null;
            this.CloseCommand = null;
            this.SelectedCommand = null;
            this.ClickCommand = null;
        }



        #region Methods
        private void OnClose(object obj)
        {
            this.IsOpen = false;
        }

        private void OnSelected(object obj)
        {
            this.IsOpen = true;
        }

        public void UpdateData(BaseTreeElementModel model, ImageDataInfo shapetype = null)
        {
            if (model != null)
            {
                this._IW_GE_NAME = model._IW_GE_NAME;
                this.Name = this._IW_GE_NAME;
                this._IW_GE_VALUE = model._IW_GE_VALUE;
                this._IW_GE_ROTATE = model._IW_GE_ROTATE;
                this._IW_GE_LNG = model._IW_GE_LNG;
                this._IW_GE_LAT = model._IW_GE_LAT;
                this.NotifyPropertyChanged(nameof(Name));
                if (shapetype != null)
                {
                    this.ShapeType = shapetype;
                }
            }
        }

        /// <summary>
        /// GIS Element List 에서 Item Button Click Command
        /// </summary>
        /// <param name="command"></param>
        public void SetCommand(RelayCommand command)
        {
            this.ClickCommand = command;
        }
        #endregion //Methods
    }

    [XmlRoot(nameof(ImageDataInfo))]
    public class ImageDataInfo : BindableAndDisposable, ITree<ITree>, ITree
    {
        #region Fields
        private string _name;
        private string _objectId;
        private string _imageString;

        private bool _isDefault;
        private bool _IsOpen = false;
        private bool _IsVisible = false;
        private bool _IsSelectionFocus = false;

        private BitmapImage _image;


        private ITree _parent;
        private ObservableCollection<ITree> _children = new ObservableCollection<ITree>();
        private TreeElementType _treeViewType = TreeElementType.Filter;
        private UpdateSource _updateSource;
        private CheckedType _isChecked = CheckedType.Checked;
        private bool _isExpanded = true;
        #endregion //Fields

        #region Propertiess

        [XmlElement("ObjectId")]
        public string ObjectId
        {
            get => _objectId;
            set => SetValue(ref _objectId, value);
        }

        [XmlElement("Name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [XmlElement("IsDefault")]
        public bool IsDefault
        {
            get => _isDefault;
            set => SetValue(ref _isDefault, value);
        }

        [XmlElement("ImageString")]
        public string ImageString
        {
            get => _imageString;
            set => SetValue(ref _imageString, value);
        }

        [XmlIgnore]
        public BitmapImage Image
        {
            get => _image;
            set => SetValue(ref _image, value);
        }

        [XmlIgnore]
        public bool IsOpen
        {
            get => _IsOpen;
            set => SetValue(ref _IsOpen, value);
        }

        [XmlIgnore]
        public bool IsVisible
        {
            get => _IsVisible;
            set => SetValue(ref _IsVisible, value);
        }

        [XmlIgnore]
        public bool IsSelectionFocus
        {
            get => _IsSelectionFocus;
            set => SetValue(ref _IsSelectionFocus, value, OnMouseLeftDown);
        }

        private void OnMouseLeftDown(bool oldValue, bool newValue)
        {
            IsChecked = IsChecked == CheckedType.UnChecked ? CheckedType.Checked : CheckedType.UnChecked;
        }

        #region Tree

        [XmlIgnore]
        public ITree Parent
        {
            get => _parent;
            set => SetValue(ref _parent, value);
        }
        [XmlIgnore]
        public ObservableCollection<ITree> Children
        {
            get => _children;
            set => SetValue(ref _children, value);
        }
        [XmlIgnore]
        public bool HasChildren
        {
            get { return Children.Count > 0; }
        }
        [XmlIgnore]
        public TreeElementType TreeViewType
        {
            get => _treeViewType;
            set => SetValue(ref _treeViewType, value);
        }
        [XmlIgnore]
        public UpdateSource UpdateSource
        {
            get => _updateSource;
            set => SetValue(ref _updateSource, value);
        }
        [XmlIgnore]
        public CheckedType IsChecked
        {
            get => _isChecked;
            set => SetValue(ref _isChecked, value, OnIsCheckedChanged);
        }
        [XmlIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetValue(ref _isExpanded, value);
        }

        private void OnIsCheckedChanged(CheckedType oldValue, CheckedType newValue)
        {
            if (this.TreeViewType == TreeElementType.Filter)
            {
                switch (this.UpdateSource)
                {
                    case UpdateSource.Parent:
                        UpdateCheckStatusWithChildren();
                        break;
                    case UpdateSource.Child:
                        this.Parent?.UpdateParentCheckStatus();
                        break;
                    default:
                        this.Parent.UpdateSource = UpdateSource.Child;
                        this.Parent.UpdateParentCheckStatus();
                        SetChildrenCheckStatus(this.IsChecked);

                        break;
                }
            }
        }

        public void SetChildrenCheckStatus(CheckedType isChecked)
        {
            this.Children?.ToList().ForEach(c =>
            {
                c.UpdateSource = UpdateSource.Parent;
                c.IsChecked = isChecked;
            });
        }

        public void UpdateCheckStatusWithChildren()
        {
            SetChildrenCheckStatus(this.IsChecked);

            this.UpdateSource = UpdateSource.None;
        }

        public void UpdateParentCheckStatus()
        {
            var childrenCount = this.Children.Count;
            var checkedCount = this.Children.Count(c => c.IsChecked != CheckedType.UnChecked);

            //Parent가 있을 경우 Parent의 Parent가 변경 될 수 있도록 Source 변경
            if (this.Parent != null)
            {
                this.Parent.UpdateSource = UpdateSource.Child;
            }

            if (checkedCount == 0)
            {
                this.IsChecked = CheckedType.UnChecked;
            }
            else if (checkedCount == childrenCount)
            {
                this.IsChecked = CheckedType.Checked;
            }
            else
            {
                this.IsChecked = CheckedType.Indeterminate;
            }

            this.UpdateSource = UpdateSource.None;
        }

        #endregion //Tree

        private void OnChangedFilter(bool? oldValue, bool? newValue)
        {

        }

        #endregion
    }
}
