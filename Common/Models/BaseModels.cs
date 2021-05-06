using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

using Common.Base;
using Common.Command;
using Common.Container.Events;
using Common.Notify;
using Common.Types;

using Newtonsoft.Json;

namespace Common.Models
{
    public delegate void DialogResultEventHandler(object obj);


    public class BaseMainViewModel : BaseControlViewModel
    {
        //Loading View Flag
        protected bool _IsWaiting;
        public virtual bool IsWaiting
        {
            get { return _IsWaiting; }
            set { SetValue(ref _IsWaiting, value); }
        }



        public BaseMainViewModel() { }
        public BaseMainViewModel(BindableAndDisposable parent, params object[] param)
        {
            InitialData(param);
        }



        public virtual void Clear() { }
        public virtual void InitialData(params object[] param) { }
    }

    public abstract class BaseViewModel : BindableAndDisposable
    {
        public BindableAndDisposable Parent { get; protected set; }
        protected Object _View;
        public virtual Object View
        {
            get { return _View; }
            set { SetValue(ref _View, value); }
        }

        //Loading View Flag
        protected bool _IsWaiting;
        public virtual bool IsWaiting
        {
            get { return _IsWaiting; }
            set { SetValue(ref _IsWaiting, value); }
        }

        protected string _Header;
        public virtual string Header
        {
            get { return _Header; }
            set { SetValue(ref _Header, value); }
        }

        /// <summary>
        /// 이벤트 관리
        /// </summary>
        public virtual EventAggregator EventAggregator { get { return (EventAggregator)_eventAggregator; } }



        public BaseViewModel() { }
        public BaseViewModel(BindableAndDisposable parent, params object[] param)
        {
            Parent = parent;
            InitialData(param);
        }



        public virtual void Clear() { }
        public virtual void InitialData(params object[] param) { }
    }

    [Serializable]
    public abstract class BaseDataModel : BindableAndDisposable
    {
        private DataType _DataType = DataType.Normal;
        [XmlIgnore]
        public virtual DataType DataType
        {
            get { return _DataType; }
            set { SetValue(ref _DataType, value); }
        }

        private bool _IsChecked = true;
        [XmlIgnore]
        public virtual bool IsChecked
        {
            get { return _IsChecked; }
            set { SetValue(ref _IsChecked, value); }
        }
    }

    [Serializable]
    public class BaseData : BindableAndDisposable
    {
        #region Properties

        /// <summary>
        /// Key
        /// </summary>
        public string DataKey { get; protected set; }
        [XmlIgnore]
        [JsonIgnore]
        /// <summary>
        /// Excel Table
        /// </summary>
        public DataTable Table { get; protected set; }
        /// <summary>
        /// Original Raw Data
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public IEnumerable<RowModel> RawDatas { get; protected set; }

        private List<ColumnModel> _Columns = new List<ColumnModel>();
        public List<ColumnModel> Columns
        {
            get { return _Columns; }
            set { SetValue(ref _Columns, value); }
        }

        private List<RowModel> _Rows = new List<RowModel>();
        public List<RowModel> Rows
        {
            get { return _Rows; }
            set { SetValue(ref _Rows, value); }
        }

        private DataType _DataType = DataType.Normal;
        public virtual DataType DataType
        {
            get { return _DataType; }
            set { SetValue(ref _DataType, value); }
        }
        /// <summary>
        /// 생성 날짜
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 수정 날짜
        /// </summary>
        public DateTime ModifyTime { get; set; }

        #endregion //Properties




        public BaseData(string key, DataTable table, IEnumerable<RowModel> rows)
        {
            InitData(key, table, rows);
            Rows = rows.ToList();
            CreateTime = DateTime.Now;
        }


        public virtual void InitData(string key, DataTable table, IEnumerable<RowModel> rows)
        {
            DataKey = key;
            Table = table;
            RawDatas = rows;
            ModifyTime = DateTime.Now;
        }



        /// <summary>
        /// Columns 추가
        /// </summary>
        /// <param name="table"></param>
        /// <param name="cols"></param>
        /// <param name="Columns"></param>
        public static void AddColumns(IList<ColumnModel> Columns, DataTable table, IEnumerable<ColumnModel> cols = null)
        {
            Columns.Clear();
            if (cols == null || cols.Count() <= 0)
            {
                int index = 1;
                foreach (DataColumn item in table.Columns)
                {
                    ColumnType type = GetColumnType(item);
                    Columns.Add(new ColumnModel(item.ColumnName, type, index));
                    index += type == ColumnType.Decimal ? 1 : 0;
                }
            }
            else
            {
                cols.ToList().ForEach(f => Columns.Add(f));
            }
        }

        private static ColumnType GetColumnType(DataColumn item)
        {
            ColumnType type;
            if (item.ColumnName.ToLower().Equals("no"))
                type = ColumnType.Int32;
            else if (item.ColumnName.ToLower().Equals("time"))
                type = ColumnType.Time;
            else/* if (item.ColumnName.Contains(Defined.TagName))*/
                type = ColumnType.Decimal;
            return type;
        }

        /// <summary>
        /// Rows 추가
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="Rows"></param>
        public static void AddRows(IList<RowModel> Rows, IEnumerable<RowModel> rows)
        {
            Rows.Clear();
            foreach (var row in rows.Where(w => w.IsChecked))
                Rows.Add(row);
        }
    }

    /// <summary>
    /// Popup Content
    /// </summary>
    public class BaseContentViewModel : BaseViewModel
    {
        #region Fields
        private bool _IsDialog = false;
        private double _Width = 1024D;
        private double _Height = 800D;
        private HorizontalAlignment _HorizontalAlignment        = HorizontalAlignment.Stretch;
        private HorizontalAlignment _HorizontalContentAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment _VerticalAlignment            = VerticalAlignment.Stretch;
        private VerticalAlignment _VerticalContentAlignment     = VerticalAlignment.Stretch;
        private Visibility _IsShowMin = Visibility.Hidden;
        private Visibility _IsShowMax = Visibility.Hidden;
        private ICommand _CloseCommand;
        #endregion //Fields

        #region Properties
        /// <summary>
        /// Dialog 여부
        /// </summary>
        public virtual bool IsDialog
        {
            get { return _IsDialog; }
            set { SetValue(ref _IsDialog, value); }
        }

        public virtual double Width
        {
            get { return _Width; }
            set { SetValue(ref _Width, value); }
        }
        public virtual double Height
        {
            get { return _Height; }
            set { SetValue(ref _Height, value); }
        }

        #region Alignment
        public virtual HorizontalAlignment HorizontalAlignment
        {
            get { return _HorizontalAlignment; }
            set { SetValue(ref _HorizontalAlignment, value); }
        }
        public virtual HorizontalAlignment HorizontalContentAlignment
        {
            get { return _HorizontalContentAlignment; }
            set { SetValue(ref _HorizontalContentAlignment, value); }
        }
        public virtual VerticalAlignment VerticalAlignment
        {
            get { return _VerticalAlignment; }
            set { SetValue(ref _VerticalAlignment, value); }
        }
        public virtual VerticalAlignment VerticalContentAlignment
        {
            get { return _VerticalContentAlignment; }
            set { SetValue(ref _VerticalContentAlignment, value); }
        }
        #endregion //Alignment

        #region IsShowButton
        public virtual Visibility IsShowMin
        {
            get { return _IsShowMin; }
            set { SetValue(ref _IsShowMin, value); }
        }
        public virtual Visibility IsShowMax
        {
            get { return _IsShowMax; }
            set { SetValue(ref _IsShowMax, value); }
        }
        #endregion //IsShowButton

        #endregion //Properties

        #region Commands
        public virtual ICommand CloseCommand
        {
            get { return _CloseCommand; }
            protected set { SetValue(ref _CloseCommand, value); }
        }
        public virtual ICommand ApplyCommand { get; protected set; }
        #endregion //Commands

        public event DialogResultEventHandler DialogCloseEvent;



        public BaseContentViewModel() { }
        public BaseContentViewModel(BindableAndDisposable parent, params object[] param)
            : base(parent, param)
        {
        }
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            if (DialogCloseEvent != null)
            {
                foreach (DialogResultEventHandler handler in DialogCloseEvent.GetInvocationList())
                    DialogCloseEvent -= handler;
            }
        }




        #region Methods
        public virtual void Close()
        {
            DialogCloseEvent?.Invoke(this);
            this.Dispose();
        }
        public virtual void SetCloseCommand(ICommand command)
        {
            CloseCommand = CloseCommand ?? command;
        }
        #endregion //Methods
    }

    public class BaseChildViewModel : BaseViewModel
    {
        #region Fields
        private bool _IsDialog = false;
        private double _Width = 1024D;
        private double _Height = 800D;
        private String _Caption = String.Empty;
        private Controls.WindowState _WindowState = Controls.WindowState.Closed;

        private HorizontalAlignment _HorizontalAlignment = HorizontalAlignment.Stretch;
        private HorizontalAlignment _HorizontalContentAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment _VerticalAlignment = VerticalAlignment.Stretch;
        private VerticalAlignment _VerticalContentAlignment = VerticalAlignment.Stretch;
        #endregion //Fields

        #region Properties
        /// <summary>
        /// Dialog 여부
        /// </summary>
        public virtual bool IsDialog
        {
            get { return _IsDialog; }
            set { SetValue(ref _IsDialog, value); }
        }

        public virtual double Width
        {
            get { return _Width; }
            set { SetValue(ref _Width, value); }
        }
        public virtual double Height
        {
            get { return _Height; }
            set { SetValue(ref _Height, value); }
        }
        public virtual String Caption
        {
            get { return _Caption; }
            set { SetValue(ref _Caption, value); }
        }
        public virtual Controls.WindowState WindowState
        {
            get { return _WindowState; }
            set { SetValue(ref _WindowState, value); }
        }

        #region Alignment
        public virtual HorizontalAlignment HorizontalAlignment
        {
            get { return _HorizontalAlignment; }
            set { SetValue(ref _HorizontalAlignment, value); }
        }
        public virtual HorizontalAlignment HorizontalContentAlignment
        {
            get { return _HorizontalContentAlignment; }
            set { SetValue(ref _HorizontalContentAlignment, value); }
        }
        public virtual VerticalAlignment VerticalAlignment
        {
            get { return _VerticalAlignment; }
            set { SetValue(ref _VerticalAlignment, value); }
        }
        public virtual VerticalAlignment VerticalContentAlignment
        {
            get { return _VerticalContentAlignment; }
            set { SetValue(ref _VerticalContentAlignment, value); }
        }
        #endregion //Alignment

        #endregion //Properties

        #region Commands
        public virtual ICommand MinWindowCommand { get; protected set; }

        public virtual ICommand CloseCommand { get; protected set; }
        public virtual ICommand ApplyCommand { get; protected set; }
        #endregion //Commands



        public BaseChildViewModel() { }
        public BaseChildViewModel(BindableAndDisposable parent, double w, double h, params object[] param)
            : base(parent, param)
        {
            this.Width = w;
            this.Height = h;
        }
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
        }
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CloseCommand = new RelayCommand(OnClose);
        }





        #region Methods
        public virtual void Close()
        {
            this.Dispose();
        }
        public virtual void OnClose(object obj)
        {
            this.Close();
        }
        #endregion //Methods
    }
}
