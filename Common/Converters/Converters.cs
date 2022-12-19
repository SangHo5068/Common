using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

using Common.Controls;
using Common.Extensions;
using Common.Utilities;

namespace Common.Converters
{
    public class BooleanToReverseConverter : IValueConverter
    {
        static BooleanToReverseConverter()
        {
            Instance = new BooleanToReverseConverter();
        }

        public static BooleanToReverseConverter Instance { get; private set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        static BooleanToVisibilityConverter()
        {
            Instance = new BooleanToVisibilityConverter();
        }

        public static BooleanToVisibilityConverter Instance
        {
            get;
            private set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BooleanToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return 0;

            double.TryParse(parameter.ToString(), out double w);

            return ((bool)value) ? w : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CountOverToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            if (int.TryParse(value.ToString(), out int val) && val > 0 &&
                int.TryParse(parameter.ToString(), out int param))
                return val > param;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class CountZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            if (int.TryParse(value.ToString(), out int val) && val > 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }

    public class CollectionZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list && list.Count > 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }

    public class ContainsToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return parameter.ToString().Contains(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class ContainsToBooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return !parameter.ToString().Contains(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class ContainsValueToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return value.ToString().Contains(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class ContainsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;
            return parameter.ToString().Contains(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }

    public class CheckedTypeToNullableBooleanConvereter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var checkedType = (CheckedType)value;

                switch (checkedType)
                {
                    case CheckedType.Checked:
                        return true;
                    case CheckedType.UnChecked:
                        return false;
                }

                return null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var nullableBool = (bool?)value;

                switch (nullableBool)
                {
                    case true:
                        return CheckedType.Checked;
                    case false:
                        return CheckedType.UnChecked;
                }

                return CheckedType.Indeterminate;
            }
            catch (Exception)
            {
                return CheckedType.UnChecked;
            }
        }
    }

    public class DoubleToStringConverterWithNaNNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                if (double.TryParse((string)parameter, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out double multiplier))
                {
                    doubleValue *= multiplier;
                }

                return double.IsNaN(doubleValue) ? null : doubleValue.ToString(culture);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string stringValue))
            {
                if (value != null)
                {
                    throw new ArgumentException("must provide string type value for 'value'", "value");
                }

                return double.NaN;
            }

            if (!double.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out double parseResult))
            {
                return null;
            }

            if (double.TryParse((string)parameter, NumberStyles.Float | NumberStyles.AllowThousands, culture.NumberFormat, out double multiplier))
            {
                parseResult /= multiplier;
            }

            return parseResult;
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CompareEnumToArgument(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }

        public static bool CompareEnumToArgument(object value, object parameter)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString());
        }
    }

    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string ParameterString))
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);
            return paramvalue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string ParameterString))
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, ParameterString);
        }
    }

    public class EnumToBooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CompareEnumToArgument(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }

        public static bool CompareEnumToArgument(object value, object parameter)
        {
            if (value == null || parameter == null)
                return false;

            return !value.ToString().Equals(parameter.ToString());
        }
    }

    public class EnumToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DependencyProperty.UnsetValue;
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(false).Cast<Attribute>();
            if (!(attributes.FirstOrDefault(attribute => attribute is DisplayAttribute) is DisplayAttribute displayAttribute))
            	return Enum.GetName(value.GetType(), value);
            return displayAttribute.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DependencyProperty.UnsetValue;
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(false).Cast<Attribute>();
            if (!(attributes.FirstOrDefault() is EnumFactory.DisplayAttribute display))
                return Enum.GetName(value.GetType(), value);
            return display.Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;
            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class EnumToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;
            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class EqualsToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return value.ToString().Equals(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class EqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Visible;
            return value.ToString().Equals(parameter.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }

    public class EqualsToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Visible;
            return value.ToString().Equals(parameter.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }

    public class EnumToAttributeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MemberInfo member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member == null)
                return string.Empty;

            XmlEnumAttribute attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
            if (attribute == null)
                return member.Name; // Fallback to the member name when there's no attribute

            return attribute.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }

    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double height))
            {
                if (double.TryParse(parameter.ToString(), out double paramHeight))
                    return height - paramHeight;
                else
                    return height;
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class ImageUrlConverter : IValueConverter
    {
        static ImageUrlConverter()
        {
            Instance = new ImageUrlConverter();
        }

        public static ImageUrlConverter Instance
        {
            get;
            private set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Uri imagePath = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
            //ImageSource source = new BitmapImage(imagePath);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string strBaseName = assembly.GetName().Name + "." + "Properties.Resources";
            ImageSource source = FindResources.GetResource<BitmapImage>(value.ToString(), strBaseName);
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class ImageResourceConverter : IValueConverter
    {
        static ImageResourceConverter()
        {
            Instance = new ImageResourceConverter();
        }

        public static ImageResourceConverter Instance
        {
            get;
            private set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource source = null;
            try
            {
                Uri imagePath = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
                source = new BitmapImage(imagePath);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            //ImageSource source = FindResources.GetAssemblyResource<BitmapImage>(value.ToString(), parameter.ToString());
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TreeViewItem item))
                return new Thickness(0);

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MultiParamConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value가 Null 일 경우 Param에 맞춰서 Boolean 값을 Return 한다.
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return false;
            else
                return value == null ? System.Convert.ToBoolean(parameter) : !System.Convert.ToBoolean(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    /// <summary>
    /// Value가 Null 이면 표시하지 않는다.
    /// </summary>
    public class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }

    public class ObjectNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : (object)Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringConcatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            return string.Format(parameter.ToString(), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class StringEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// String.IsEmpty을 Visibility로 반환한다.
        /// String.IsEmpty가 true이면 Visibility.Visible 를 반환하고 false이면 Visibility.Collapsed를 반환한다.
        /// parameter에 false를 전달하면 반환값이 반전된다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = value == null || string.IsNullOrEmpty(value.ToString());
            var noParamResult = boolean ? Visibility.Visible : Visibility.Collapsed;

            if (parameter == null)
                return noParamResult;

            if (bool.TryParse(parameter.ToString(), out bool inverted) == false)
                return noParamResult;

            return boolean ^ inverted ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }

    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return ImageConverter.FileSteamToImage(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return parameter;
            if (double.TryParse(value.ToString(), out double conValue))
                return conValue;

            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SubtractionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double MinusWidth = 25.0;
            if (parameter != null && string.IsNullOrEmpty(parameter.ToString()))
                double.TryParse(parameter.ToString(), out MinusWidth);
            return (double)value - MinusWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class TreeViewTypeToCheckBoxVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var treeviewType = (TreeElementType)value;

                if (treeviewType == TreeElementType.Filter)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TreeElementType.None;
        }
    }

    public class TypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.GetType() == (Type)parameter)
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Watermark Converter
    /// </summary>
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is string text && values[1] is bool isFocus)
            {
                bool hasText = !string.IsNullOrEmpty(text);
                bool hasFocus = isFocus;

                if (hasFocus || hasText)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = null;
            if (value is Visual v)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    XamlWriter.Save(v, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    result = XamlReader.Load(ms);
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return InternalConvert(value, targetType, parameter);
        }

        public object InternalConvert(object[] value, Type targetType, object parameter)
        {
            Double intReturn = 100;
            try
            {
                if (value[0] == null || Double.Parse(value[0].ToString()) <= 0)
                    return Double.NaN;

                intReturn = Double.Parse(value[0].ToString());
                if (!(value[1] is FrameworkElement control))
                    return Double.NaN;

                if (!(control.Parent is Panel parent) || parent.Children.Count <= 1)
                    return (Double)intReturn;

                double childwidth = 0;
                TextBlock Self = value[value.Length - 1] as TextBlock;
                for (int i = 0; i < parent.Children.Count; i++)
                {
                    if (Self == parent.Children[i]) continue;
                    if (!(parent.Children[i] is FrameworkElement item)) continue;

                    childwidth += item.ActualWidth + item.Margin.Left + item.Margin.Right;
                }
                intReturn = parent.ActualWidth - childwidth - parent.Margin.Left - parent.Margin.Right - 16;
            }
            catch (Exception) { }
            return (Double)intReturn;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string[] splitValue = null;
            if (value != null)
            {
                splitValue = ((string)value).Split((Char)parameter);
            }
            return splitValue;
        }
    }

    public class WaitingMessageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var msg = values[0] as string;
            var sub_msg = values[1] as string;

            if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(sub_msg))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ZeroToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Int32.TryParse(value.ToString(), out int intVal);
                if (intVal > 0)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return Visibility.Visible;
            else
                return Binding.DoNothing;
        }
    }
}
