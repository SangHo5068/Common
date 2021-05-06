using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

using Common.Notify;
using Common.Types;
using Common.Utilities;

using Newtonsoft.Json;

namespace Common.Models
{
    #region DataTable
    [Serializable]
    public class ColumnModel : BaseDataModel
    {
        #region Fields
        ColumnType type;
        #endregion //Fields

        #region Properties
        public int TagIndex { get; set; }
        public string FieldName { get; set; }
        public ColumnType Type
        {
            get { return type; }
            set { SetValue(ref type, value); }
        }
        /// <summary>
        /// 전처리 진행 한 Tag에대한 표준편차 값
        /// </summary>
        public double StDev { get; set; }
        /// <summary>
        /// 전처리 진행 한 Tag에대한 평균 값
        /// </summary>
        public double Average { get; set; }


        /// <summary>
        /// 모델링용 Column
        /// </summary>
        public bool IsInput { get; set; }
        /// <summary>
        /// PLS 출력 Column
        /// </summary>
        public bool IsOutput { get; set; }
        #endregion //Properties


        public ColumnModel(string name, ColumnType type, int index)
        {
            FieldName = name;
            Type = type;
            TagIndex = index;
        }




        /// <summary>
        /// 평균과 표준편차를 저장
        /// </summary>
        public void SetStDevAverage(double stDev, double average)
        {
            StDev = stDev;
            Average = average;
        }
    }
    [Serializable]
    public class RowModel : BaseDataModel
    {
        private readonly object tag_lock = new object();
        public int No { get; set; }
        public DateTime Time { get; set; }
        /// <summary>
        /// 분석용 데이터
        /// </summary>
        [XmlIgnore]
        public bool IsAnalysis { get; set; }
        /// <summary>
        /// 이상구간 데이터 인지
        /// </summary>
        [XmlIgnore]
        public bool IsAbnormalData { get; set; }

        /// <summary>
        /// 구간별 모델링 이름
        /// </summary>
        [XmlIgnore]
        public string ModelingName { get; set; }

        private Dictionary<int, CellModel> _Tags = new Dictionary<int, CellModel>();
        public Dictionary<int, CellModel> Tags
        {
            get { return _Tags; }
            set { SetValue(ref _Tags, value); }
        }



        public RowModel() { }
        /// <summary>
        /// 모델링 결과 저장
        /// </summary>
        /// <param name="index">순서</param>
        /// <param name="modelingResult">모델링 결과 값</param>
        /// <param name="fieldNames">Column Name</param>
        /// <param name="date">Row Time</param>
        /// <param name="isAnalysis">분석용 데이터 인지 확인(True:분석용 데이터임)</param>
        public RowModel(int index, List<double> modelingResult, string[] fieldNames,
            DateTime? date = null, bool isAnalysis = false)
        {
            Time = date ?? DateTime.Now;
            IsAnalysis = isAnalysis;
            No = index + 1;
            lock (tag_lock)
            {
                Tags.Clear();
                int i = 0;
                modelingResult.ForEach(f =>
                {
                    if (i >= fieldNames.Count())
                        return;
                    Tags.Add(i + 1, new CellModel(f.ToString(), Time, fieldNames[i++], false));
                });
            }
        }
        public RowModel(int index, List<double> modelingResult, IEnumerable<ColumnModel> columns,
            DateTime? date = null, bool isAnalysis = false)
        {
            Time = date ?? DateTime.Now;
            IsAnalysis = isAnalysis;
            No = index + 1;
            try
            {
                lock (tag_lock)
                {
                    Tags.Clear();
                    int i = 0;
                    modelingResult.ForEach(f =>
                    {
                        var col = columns.ElementAt(i++);
                        if (col != null)
                            Tags.Add(col.TagIndex, new CellModel(f.ToString(), Time, col.FieldName, false));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }




        /// <summary>
        /// 모델링된 결과값을 저장
        /// </summary>
        /// <param name="result"></param>
        /// <param name="columns"></param>
        /// <param name="outputs">PLS Output Columns</param>
        public void SetScroeValue(List<double> result, IEnumerable<ColumnModel> columns,
            IEnumerable<ColumnModel> outputs = null)
        {
            int i = 0;
            foreach (var column in columns)
                if (Tags.ContainsKey(column.TagIndex))
                    Tags[column.TagIndex].SetScroeValue(result[i++]);
            if (outputs != null)
            {
                foreach (var column in outputs)
                    if (Tags.ContainsKey(column.TagIndex))
                        Tags[column.TagIndex].IsOutput = true;
            }
        }

        public IEnumerable<double> GetSelectTagScroeValues(IEnumerable<ColumnModel> columns)
        {
            var names = columns.Select(s => s.FieldName).ToArray();
            var selectedTag = Tags.Values.Where(w => names.Contains(w.Name));
            if (selectedTag == null || selectedTag.Count() == 0)
                return Enumerable.Empty<double>();
            return selectedTag.SelectMany(s =>
            {
                if (double.TryParse(s.ScroeResult, out double value))
                    return new double[] { value };
                return new double[] { double.NaN };
            });
        }

        /// <summary>
        /// Get Row Tag Value List
        /// </summary>
        /// <param name="empty"></param>
        /// <returns></returns>
        public double[] GetValueList(double empty = double.NaN)
        {
            return Tags.Values.SelectMany(s =>
            {
                if (double.TryParse(s.SmoothValue, out double value))
                    return new double[] { value };
                return new double[] { empty };
            }).ToArray();
        }

        /// <summary>
        /// Selected Tag Value List
        /// </summary>
        /// <param name="names">선택된 ColumnNames</param>
        /// <param name="isOriginal">결과 데이터 표준화 유무(False:표준화 됨)</param>
        /// <param name="empty"></param>
        /// <returns></returns>
        public double[] GetSelectTagValueList(string[] names, bool isOriginal = false, double empty = double.NaN)
        {
            var selectedTag = Tags.Values.Where(w => names.Contains(w.Name));
            if (selectedTag == null || selectedTag.Count() == 0)
                return new double[0];
            return selectedTag.SelectMany(s =>
            {
                if (double.TryParse(isOriginal ? s.OriginalValue : s.SmoothValue, out double value))
                    return new double[] { value };
                return new double[] { empty };
            }).ToArray();
        }

        /// <summary>
        /// Selected Tag Value List
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isOriginal"></param>
        /// <param name="empty"></param>
        /// <returns></returns>
        public double[] GetSelectTagValueList(string name, bool isOriginal = false, double empty = double.NaN)
        {
            var selectedTag = Tags.Values.Where(w => name.Equals(w.Name));
            if (selectedTag == null || selectedTag.Count() == 0)
                return new double[0];
            return selectedTag.SelectMany(s =>
            {
                if (double.TryParse(isOriginal ? s.OriginalValue : s.SmoothValue, out double value))
                    return new double[] { value };
                return new double[] { empty };
            }).ToArray();
        }
    }
    [Serializable]
    public class CellModel : BaseDataModel
    {
        #region Fields
        private string _Name;
        private string _OriginalValue;
        private string _Value;
        private string _SmoothValue;
        private string _ScroeResult;
        #endregion //Fields

        #region Properties
        public DateTime Time { get; set; }
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }
        /// <summary>
        /// 원본값
        /// </summary>
        public string OriginalValue
        {
            get { return _OriginalValue; }
            set { SetValue(ref _OriginalValue, value); }
        }
        public string Value
        {
            get { return _Value; }
            set { SetValue(ref _Value, value); }
        }
        /// <summary>
        /// 표준화 된 데이터
        /// </summary>
        public string SmoothValue
        {
            get { return _SmoothValue; }
            set { SetValue(ref _SmoothValue, value); }
        }
        public string ScroeResult
        {
            get { return _ScroeResult; }
            private set { SetValue(ref _ScroeResult, value); }
        }
        /// <summary>
        /// PLS 출력 Column
        /// </summary>
        public bool IsOutput { get; set; }
        #endregion //Properties



        public CellModel() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <param name="name"></param>
        /// <param name="isChecked"></param>
        /// <param name="type"></param>
        public CellModel(object value, DateTime time, string name = "",
            bool isChecked = true, DataType type = DataType.Normal)
        {
            //double.TryParse(value.ToString(), out _Value);
            OriginalValue = value as string;
            SmoothValue = Value = OriginalValue;
            Time = time;
            DataType = type;
            IsChecked = isChecked;
            Name = name;
        }



        #region Methods
        /// <summary>
        /// 표준화 된 데이터 저장
        /// </summary>
        /// <param name="v"></param>
        public void SetDataStandardization(double v)
        {
            SmoothValue = Value = v.ToString();
        }

        /// <summary>
        /// 입력된 빈값을 전처리 LoessInterpolator.Smooth 를 진행 한 값으로 채운다
        /// </summary>
        /// <param name="value"></param>
        public void SetSmoothValue(double value)
        {
            SmoothValue = double.IsNaN(value) && !string.IsNullOrEmpty(Value) ? Value : value.ToString();
        }

        public void SetScroeValue(double value)
        {
            ScroeResult = value.ToString();
        }
        #endregion //Methods
    }
    #endregion //DataTable


    /// <summary>
    /// 데이터 구간선택
    /// </summary>
    public class DateRange : BindableAndDisposable
    {
        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetValue(ref _IsSelected, value); }
        }
        private DateTime _StartDate;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set { SetValue(ref _StartDate, value); }
        }
        private DateTime _EndDate;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set { SetValue(ref _EndDate, value); }
        }
        /// <summary>
        /// EndTime 을 23:59:59 형식으로 리턴
        /// </summary>
        public DateTime GetEndDate
        {
            get { return EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59); }
        }
        private ICommand _CommandDelDateRange;
        [XmlIgnore]
        [JsonIgnore]
        public ICommand CommandDelDateRange
        {
            get { return _CommandDelDateRange; }
            set { SetValue(ref _CommandDelDateRange, value); }
        }
        private string _Name = string.Empty;
        public string Name
        {
            get { return _Name; }
            set { SetValue(ref _Name, value); }
        }


        public DateRange(DateTime start, DateTime end, ICommand delCommand = null, string name = null)
        {
            this.StartDate = start;
            this.EndDate = end;
            this.CommandDelDateRange = delCommand;
            this.Name = name;
        }
    }
}
