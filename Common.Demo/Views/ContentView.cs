using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Common.Cultures;
using Common.Extensions;
using Common.Models;

using Common.Notify;


namespace Common.Demo.Views
{
    public enum CultureTypes
    {
        [Display(Name = "en-US", Description = "영문")]
        enUS,
        [Display(Name = "ko-KR", Description = "한글")]
        koKR,
    }


    public class BaseContentView : BaseViewModel
    {
        private object _SelectedItem;
        #region Properties
        public object SelectedItem
        {
            get => _SelectedItem;
            set => SetValue(ref _SelectedItem, value, OnChangedSelection);
        }
        #endregion //Properties



        public BaseContentView() { }
        public BaseContentView(BindableAndDisposable parent, params object[] param)
            : base(parent, param)
        {
        }



        public override void InitialData(params object[] param)
        {
            base.InitialData(param);
        }




        private void OnChangedSelection(object oldValue, object newValue)
        {
            if (Enum.TryParse(newValue.ToString(), out CultureTypes value))
            {
                var culture = new CultureInfo(value.ToDisplay());
                CultureResources.ChangeCulture(culture);
            }
        }
    }
}
