using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Common.Converters
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
         where T : class, new()
    {
        private static readonly Lazy<T> _converter = new Lazy<T>(() => new T());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter.Value;
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    public class BooleanToVisibilityInvertConverter : ConverterMarkupExtension<BooleanToVisibilityInvertConverter>
    {
        /// <summary>
        /// Boolean을 Visibility로 반환한다.
        /// value가 true이면 Visibility.Visible 를 반환하고 false이면 Visibility.Collapsed를 반환한다.
        /// parameter에 false를 전달하면 반환값이 반전된다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = (bool)value;
            var noParamResult = boolean ? Visibility.Visible : Visibility.Collapsed;

            if (parameter == null)
                return noParamResult;

            if (bool.TryParse(parameter.ToString(), out bool inverted) == false)
                return noParamResult;

            return boolean ^ inverted ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Visibility 를 Boolean으로 반환한다.
        /// value가 Visibility.Visible이면 true를 반환하고 Visibility.Collapsed 이면 false를 반환한다.
        /// parameter에 false를 전달하면 반환값이 반전된다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (int)((Visibility)value) < 1;
            var inverted = (bool?)parameter;
            if (inverted == null)
                return visible;

            return !(visible ^ (bool)inverted);
        }
    }

    public class ColorToSolidBrushConverter : ConverterMarkupExtension<ColorToSolidBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (System.Drawing.Color)value;
            return new SolidColorBrush(new Color() { R = color.R, G = color.G, B = color.B, A = color.A });
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SolidColorBrush solidBrush))
                return default(System.Drawing.Color);

            var color = solidBrush.Color;
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ConvertStringToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }

    public class NullToUnsetValueConverter : ConverterMarkupExtension<NullToUnsetValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;
            return ImageConverter.ToImageSource(value.ToString());
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;
            return ImageConverter.ToImageSource(value.ToString());
        }
    }
}
