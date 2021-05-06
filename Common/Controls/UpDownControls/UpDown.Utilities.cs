using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Controls
{
    #region UpDownControls
    enum HMSType : byte
    {
        Hour,
        HHour,
        hour,
        hhour,
        minute,
        mminute,
        second,
        ssecond,
        t,
        tt,
        unknown
    }
    public enum DecimalSeparatorType : int
    {
        System_Defined,
        Point,
        Comma
    }
    public enum NegativeSignType : int
    {
        System_Defined,
        Minus
    }
    public enum NegativeSignSide : int
    {
        System_Defined,
        Prefix,
        Suffix
    }

    internal interface IFrameTxtBoxCtrl
    {
        TextBox TextBox { get; }
    }

    static class Coercer
    {
        public static void Initialize<T>() where T : UserControl
        {
            try
            {
                FrameworkPropertyMetadata BorderThicknessMetaData = new FrameworkPropertyMetadata
                {
                    CoerceValueCallback = new CoerceValueCallback(Coercer.CoerceBorderThickness)
                };
                UserControl.BorderThicknessProperty.OverrideMetadata(typeof(T), BorderThicknessMetaData);

                // For Background, do not do in XAML part something like:
                // Background="{Binding Background, ElementName=Root}" in TextBoxCtrl settings.
                // Reason: although this will indeed set the Background values as expected, problems arise when user
                // of control does not explicitly not set a value.
                // In this case, Background of TextBoxCtrl get defaulted to values in UserControl, which is null
                // and not what we want.
                // We want to keep the default values of a standard TextBox, which may differ according to themes.
                // Have to treat similarly as with BorderThickness...

                FrameworkPropertyMetadata BackgroundMetaData = new FrameworkPropertyMetadata
                {
                    CoerceValueCallback = new CoerceValueCallback(Coercer.CoerceBackground)
                };
                UserControl.BackgroundProperty.OverrideMetadata(typeof(T), BackgroundMetaData);

                // ... Same for BorderBrush
                FrameworkPropertyMetadata BorderBrushMetaData = new FrameworkPropertyMetadata
                {
                    CoerceValueCallback = new CoerceValueCallback(Coercer.CoerceBorderBrush)
                };
                UserControl.BorderBrushProperty.OverrideMetadata(typeof(T), BorderBrushMetaData);
            }
            catch (Exception)
            {
            }
        }
        private delegate void FuncCoerce(IFrameTxtBoxCtrl FrameTxtBox, object value);

        private static void CommonCoerce(DependencyObject d, object value, FuncCoerce funco)
        {
            if (d is IFrameTxtBoxCtrl FrameTxtBox)
                funco(FrameTxtBox, value);
        }
        private static void FuncCoerceBorderThickness(IFrameTxtBoxCtrl FrameTxtBox, object value)
        {
            if (value is Thickness thickness)
                FrameTxtBox.TextBox.BorderThickness = thickness;
        }
        private static void FuncCoerceBackground(IFrameTxtBoxCtrl FrameTxtBox, object value)
        {
            if (value is Brush brush)
                FrameTxtBox.TextBox.Background = brush;
        }
        private static void FuncCoerceBorderBrush(IFrameTxtBoxCtrl FrameTxtBox, object value)
        {
            if (value is Brush brush)
                FrameTxtBox.TextBox.BorderBrush = brush;
        }
        public static object CoerceBorderThickness(DependencyObject d, object value)
        {
            CommonCoerce(d, value, FuncCoerceBorderThickness);
            return new Thickness(0.0);
        }
        public static object CoerceBackground(DependencyObject d, object value)
        {
            CommonCoerce(d, value, FuncCoerceBackground);
            return value;
        }
        public static object CoerceBorderBrush(DependencyObject d, object value)
        {
            CommonCoerce(d, value, FuncCoerceBorderBrush);
            return value;
        }
    }
    static class LanguageStrings
    {
        static readonly CultureInfo ci = CultureInfo.CurrentCulture;
        /*static LanguageStrings() // For testing purposes
        {
            //ci = CultureInfo.CreateSpecificCulture("de-DE");
            ci = CultureInfo.CreateSpecificCulture("fr-FR");
        }*/
        static string GetLangStr(string strTag)
        {
            return Properties.Resources.ResourceManager.GetString(strTag, ci);
        }
        public static string CopyTime { get { return GetLangStr("COPY_TIME"); } }
        public static string PasteTime { get { return GetLangStr("PASTE_TIME"); } }
        public static string ValidTimes { get { return GetLangStr("VALID_TIMES"); } }
        public static string From { get { return GetLangStr("FROM"); } }
        public static string To { get { return GetLangStr("TO"); } }
        public static string None { get { return GetLangStr("NONE"); } }
    }
    static class SystemNumberInfo
    {
        private static readonly NumberFormatInfo nfi;

        static SystemNumberInfo()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            nfi = ci.NumberFormat;
        }
        public static string DecimalSeparator
        {
            get { return nfi.NumberDecimalSeparator; }
        }
        public static string NegativeSign
        {
            get { return nfi.NegativeSign; }
        }
        public static bool IsNegativePrefix
        {
            // for values, see: http://msdn.microsoft.com/en-us/library/system.globalization.numberformatinfo.numbernegativepattern.aspx
            // Assume if negative number format is (xxx), number is prefixed.
            get
            {
                return nfi.NumberNegativePattern < 3;
            }
        }
    }
    static class SystemDateInfo
    {
        private static readonly DateTimeFormatInfo dtfi;

        static SystemDateInfo()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            dtfi = ci.DateTimeFormat;
        }
        // Pattern tags given in http://msdn.microsoft.com/en-us/library/system.globalization.datetimeformatinfo.aspx
        public static string LongTimePattern
        {
            get { return dtfi.LongTimePattern; }
        }
        public static string AMDesignator
        {
            get { return dtfi.AMDesignator; }
        }
        public static string PMDesignator
        {
            get { return dtfi.PMDesignator; }
        }
    }
    static class TimeCtrlExtensions
    {
        // Extension properties not allowed unfortunately, so can't write HMSType hmstype = tb.HMSType; for instance. 
        public static HMSType Get_HMSType(this FrameworkElement ctrl)
        {

            if (!Enum.TryParse(ctrl.Name, out HMSType hmsType))
                hmsType = HMSType.unknown;

            return hmsType;
        }
        public static void Set_HMSType(this FrameworkElement ctrl, HMSType hmsType)
        {
            ctrl.Name = hmsType.ToString();
        }
        public static int Get12Hour(this int Hour24)
        {
            return (Hour24 == 0) ? 12 : ((Hour24 <= 12) ? Hour24 : Hour24 % 12);
        }
        public static bool IsHalfDayHour(this TextBox tb)
        {
            return tb.Get_HMSType() == HMSType.hour || tb.Get_HMSType() == HMSType.hhour;
        }
        public static string GetAMDesignatorChar(int i)
        {
            return ((SystemDateInfo.AMDesignator.Length > i) && (SystemDateInfo.AMDesignator.Length > 0)) ? SystemDateInfo.AMDesignator[i].ToString() : "";
        }
        public static string GetPMDesignatorChar(int i)
        {
            return ((SystemDateInfo.PMDesignator.Length > i) && (SystemDateInfo.PMDesignator.Length > 0)) ? SystemDateInfo.PMDesignator[i].ToString() : "";
        }
        public static int Get_t_Idx()
        {
            int Idx = 0;

            if (SystemDateInfo.PMDesignator.Length > 0 && SystemDateInfo.AMDesignator.Length > 0
                && SystemDateInfo.PMDesignator[0] == SystemDateInfo.AMDesignator[0]) // case Japan.
                Idx++;

            return Idx;
        }
        public static bool IsAM_PM(this FrameworkElement ctrl)
        {
            return ctrl.Get_HMSType() == HMSType.tt || ctrl.Get_HMSType() == HMSType.t;
        }
        public static bool IsAlways2CharInt(this FrameworkElement ctrl)
        {
            return ((Get_HMSType(ctrl) == HMSType.HHour || Get_HMSType(ctrl) == HMSType.hhour ||
                Get_HMSType(ctrl) == HMSType.mminute || Get_HMSType(ctrl) == HMSType.ssecond));
        }
        private static string GetHMSText(FrameworkElement ctrl, DateTime dt)
        {
            string strFormat, HMSText;

            strFormat = ctrl.IsAlways2CharInt() ? "{0:D2}" : "{0:D}";

            switch (Get_HMSType(ctrl))
            {
                case HMSType.Hour:
                case HMSType.HHour:
                    HMSText = string.Format(strFormat, dt.Hour);
                    break;
                case HMSType.hour:
                case HMSType.hhour:
                    HMSText = string.Format(strFormat, dt.Hour.Get12Hour());
                    break;
                case HMSType.minute:
                case HMSType.mminute:
                    HMSText = string.Format(strFormat, dt.Minute);
                    break;
                case HMSType.second:
                case HMSType.ssecond:
                    HMSText = string.Format(strFormat, dt.Second);
                    break;
                case HMSType.t:
                    HMSText = (dt.Hour / 12 == 0) ? GetAMDesignatorChar(Get_t_Idx()) : GetPMDesignatorChar(Get_t_Idx());
                    break;
                case HMSType.tt:
                    HMSText = (dt.Hour / 12 == 0) ? SystemDateInfo.AMDesignator : SystemDateInfo.PMDesignator;
                    break;
                default:
                    HMSText = "";
                    break;
            }
            return HMSText;
        }
        public static void Set_HMSText(this TextBox tb, DateTime dt)
        {
            tb.Text = GetHMSText(tb, dt);
        }
        public static int Get_Max(this TextBox tb)
        {
            switch (tb.Get_HMSType())
            {
                case HMSType.Hour:
                case HMSType.HHour:
                    return 24;
                case HMSType.hour:
                case HMSType.hhour:
                    return 13;
                default:
                    return 60;
            }
        }
        public static int Get_Min(this TextBox tb)
        {
            switch (tb.Get_HMSType())
            {
                case HMSType.hour:
                case HMSType.hhour:
                    return 1;
                default:
                    return 0;
            }
        }
        public static DateTime ResetTime(this DateTime dt, int hms, HMSType hmsType)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day,
                (hmsType == HMSType.Hour) || (hmsType == HMSType.HHour ||
                 hmsType == HMSType.hour) || (hmsType == HMSType.hhour) ? hms : dt.Hour,
                (hmsType == HMSType.minute) || (hmsType == HMSType.mminute) ? hms : dt.Minute,
                (hmsType == HMSType.second) || (hmsType == HMSType.ssecond) ? hms : dt.Second,
                dt.Millisecond, dt.Kind);
        }
        public static DateTime Reset_AM_PM_Time(this DateTime dt, bool IsAM)
        {
            int Hour = dt.Hour;

            if (IsAM && Hour >= 12)
                Hour -= 12;
            else if (!IsAM && Hour < 12)
                Hour += 12;

            return new DateTime(dt.Year, dt.Month, dt.Day, Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
        }
        public static string Get_TextFormat(this FrameworkElement fe)
        {
            if (fe is TextBlock block)
                return block.Text;
            else
            {
                if (Get_HMSType(fe) == HMSType.tt)
                    return "{4}";
                if (Get_HMSType(fe) == HMSType.t)
                    return "{5}";

                string strFormat = "{";

                switch (Get_HMSType(fe))
                {
                    case HMSType.HHour:
                    case HMSType.Hour:
                        strFormat += "0";
                        break;
                    case HMSType.hhour:
                    case HMSType.hour:
                        strFormat += "1";
                        break;
                    case HMSType.mminute:
                    case HMSType.minute:
                        strFormat += "2";
                        break;
                    case HMSType.ssecond:
                    case HMSType.second:
                        strFormat += "3";
                        break;
                }
                strFormat += ":D";

                if ((Get_HMSType(fe) == HMSType.HHour || Get_HMSType(fe) == HMSType.hhour ||
                    Get_HMSType(fe) == HMSType.mminute || Get_HMSType(fe) == HMSType.ssecond))
                    strFormat += "2";

                strFormat += "}";

                return strFormat;
            }

        }
        public static bool IsValidTime(string strTime, string strPattern, out int Hour, out int Minute, out int Second)
        {
            // Loose validation: if for instance have an hour entry as '23' or '02', this still gets validated for an h tag.
            // Rule: if a number entry can possibly be interpreted, then it is. Allow AM/PM tag to go missing too.
            // ...but outside of this, must have a valid time and separators such as ':', '.' must be present and must match.  
            Hour = 0;
            Minute = 0;
            Second = 0;

            if (string.IsNullOrEmpty(strTime) || string.IsNullOrEmpty(strPattern))
                return false;

            bool IsValid = true;
            int Idx = 0, PatIdx = 0, Value;
            string strInt;

            while (IsValid && Idx < strTime.Length && PatIdx < strPattern.Length)
            {
                if (strPattern[PatIdx] == 'h' || strPattern[PatIdx] == 'H' || strPattern[PatIdx] == 'm' || strPattern[PatIdx] == 's')
                {
                    if (char.IsDigit(strTime, Idx))
                    {
                        strInt = strTime[Idx].ToString();

                        if (++Idx < strTime.Length && char.IsDigit(strTime, Idx))
                            strInt += strTime[Idx++].ToString();

                        Value = int.Parse(strInt);

                        if (((strPattern[PatIdx] == 'h' || strPattern[PatIdx] == 'H') && Value >= 24) || Value >= 60)
                            IsValid = false;
                        else
                        {
                            if (strPattern[PatIdx] == 'h' || strPattern[PatIdx] == 'H')
                                Hour = Value;
                            else if (strPattern[PatIdx] == 'm')
                                Minute = Value;
                            else
                                Second = Value;

                            if (PatIdx + 1 < strPattern.Length && (strPattern[PatIdx] == strPattern[PatIdx + 1]))
                                PatIdx += 2;
                            else
                                PatIdx++;
                        }
                    }
                    else
                        IsValid = false;
                }
                else if (strPattern[PatIdx] == 't')
                {
                    bool IsPM = (strTime[Idx] == SystemDateInfo.PMDesignator[0]);

                    if (strTime[Idx] == SystemDateInfo.AMDesignator[0] || strTime[Idx] == SystemDateInfo.PMDesignator[0])
                        Idx++;

                    if (PatIdx + 1 < strPattern.Length && (strPattern[PatIdx] == strPattern[PatIdx + 1]))
                    {
                        if (Idx < strTime.Length && (strTime[Idx] == SystemDateInfo.AMDesignator[1] || strTime[Idx] == SystemDateInfo.PMDesignator[1]))
                        {
                            if (IsPM && strTime[Idx] != SystemDateInfo.PMDesignator[1])
                                IsPM = false;

                            Idx++;
                        }
                        else
                            IsValid = false;

                        PatIdx += 2;
                    }
                    else
                        PatIdx++;

                    if (IsPM && Hour < 12)
                        Hour += 12;
                }
                else if (strTime[Idx] == strPattern[PatIdx])
                {
                    Idx++;
                    PatIdx++;
                }
                else
                    IsValid = false;
            }
            if (Idx < strTime.Length || PatIdx < strPattern.Length)
                IsValid = false;

            return IsValid;
        }
    }

    public class ThicknessToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (!(value is Thickness) || !(parameter is string))
                throw new ArgumentException();

            bool.TryParse((string)parameter, out bool IncrRightThickness);
            var thickness = (Thickness)value;
            return new Thickness(thickness.Left + 1.0, thickness.Top + 1.0, IncrRightThickness ? thickness.Right + 18.0 : 18.0, thickness.Bottom);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Not Implemented.");
        }
    }
    public static class TimeCtrlCustomCommands
    {
        static TimeCtrlCustomCommands()
        {
            Type my = typeof(TimeCtrlCustomCommands);
            CopyTime = new RoutedUICommand(LanguageStrings.CopyTime, "CopyTime", my);
            PasteTime = new RoutedUICommand(LanguageStrings.PasteTime, "PasteTime", my);
            ShowValidTimes = new RoutedUICommand("", "ShowValidTimes", my);
        }
        public static RoutedUICommand CopyTime { get; private set; }
        public static RoutedUICommand PasteTime { get; private set; }
        public static RoutedUICommand ShowValidTimes { get; private set; }
    }
    #endregion //UpDownControls
}
