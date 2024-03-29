﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Common.Utilities;

namespace Common.Controls
{
    [ValueConversion(typeof(double), typeof(String))]
    public class DoubleToIntegerStringConverter : IValueConverter
    {
        public Object Convert(
            Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            int    intValue    = (int)doubleValue;

            return intValue.ToString();
        }
        public Object ConvertBack(
            Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            String stringValue = (String)value;
            double doubleValue = 0;
            if (!Double.TryParse(stringValue, out doubleValue))
                doubleValue = 0;

            return doubleValue;
        }
    }

    public class ColorPicker : Control
    {
        private const string RedColorSliderName = "PART_RedColorSlider";
        private const string GreenColorSliderName = "PART_GreenColorSlider";
        private const string BlueColorSliderName = "PART_BlueColorSlider";
        private const string AlphaColorSliderName = "PART_AlphaColorSlider";

        private const string SpectrumSliderName = "PART_SpectrumSlider1";
        private const string HsvControlName = "PART_HsvControl";

        #region Fields
        private ColorSlider m_redColorSlider;
        private ColorSlider m_greenColorSlider;
        private ColorSlider m_blueColorSlider;
        private ColorSlider m_alphaColorSlider;

        private SpectrumSlider m_spectrumSlider;

        private HsvControl m_hsvControl;

        private bool m_withinChange;
        private bool m_templateApplied;
        #endregion //Fields

        #region Dependency Properties
        public String BeforeColorText
        {
            get { return (String)GetValue(BeforeColorTextProperty); }
            set { SetValue(BeforeColorTextProperty, value); }
        }
        public static readonly DependencyProperty BeforeColorTextProperty =
            DependencyProperty.Register("BeforeColorText", typeof(String), typeof(ColorPicker),
            new UIPropertyMetadata(Brushes.Black.ToString()));

        public String SelectedColorText
        {
            get { return (String)GetValue(SelectedColorTextProperty); }
            set { SetValue(SelectedColorTextProperty, value); }
        }
        public static readonly DependencyProperty SelectedColorTextProperty =
            DependencyProperty.Register("SelectedColorText", typeof(String), typeof(ColorPicker),
            new UIPropertyMetadata(Brushes.Black.ToString()));

        public Color BeforeColor
        {
            get { return (Color)GetValue(BeforeColorProperty); }
            set { SetValue(BeforeColorProperty, value); }
        }
        public static readonly DependencyProperty BeforeColorProperty =
            DependencyProperty.Register("BeforeColor", typeof(Color), typeof(ColorPicker),
            new UIPropertyMetadata(Colors.Black));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), 
            new UIPropertyMetadata(Colors.Black, new PropertyChangedCallback(OnSelectedColorPropertyChanged)));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ColorPicker),
            new UIPropertyMetadata(Orientation.Horizontal));

        public bool FixedSliderColor
        {
            get { return (bool)GetValue(FixedSliderColorProperty); }
            set { SetValue(FixedSliderColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FixedSliderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FixedSliderColorProperty =
            DependencyProperty.Register("FixedSliderColor", typeof(bool), typeof(SpectrumSlider),
            new UIPropertyMetadata(false, new PropertyChangedCallback(OnFixedSliderColorPropertyChanged)));
        #endregion //Dependency Properties

        #region Routed Events

        public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedColorChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<Color>),
            typeof(ColorPicker)
        );

        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add { AddHandler(SelectedColorChangedEvent, value); }
            remove { RemoveHandler(SelectedColorChangedEvent, value); }
        }

        #endregion Routed Events



        static ColorPicker()
        {
            Type type = typeof(ColorPicker);
            DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));

            // Register Event Handler for slider
            EventManager.RegisterClassHandler(type, Slider.ValueChangedEvent, new RoutedPropertyChangedEventHandler<double>(ColorPicker.OnSliderValueChanged));

            // Register Event Handler for Hsv Control
            EventManager.RegisterClassHandler(type, HsvControl.SelectedColorChangedEvent, new RoutedPropertyChangedEventHandler<Color>(ColorPicker.OnHsvControlSelectedColorChanged));
        }



        #region Event Handling

        private void OnSliderValueChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            // Avoid endless loop
            if (m_withinChange)
                return;

            m_withinChange = true;
            if (e.OriginalSource == m_redColorSlider || 
                e.OriginalSource == m_greenColorSlider ||
                e.OriginalSource == m_blueColorSlider ||
                e.OriginalSource == m_alphaColorSlider)
            {
                Color newColor = GetRgbColor();
                UpdateHsvControlColor(newColor);
                UpdateSelectedColor(newColor);
            }
            else if (e.OriginalSource == m_spectrumSlider)
            {
                double hue = m_spectrumSlider.Hue;
                UpdateHsvControlHue(hue);
                Color newColor = GetHsvColor();
                UpdateRgbColors(newColor);
                UpdateSelectedColor(newColor);
            }

            m_withinChange = false;
        }

        private static void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorPicker colorPicker = sender as ColorPicker;
            colorPicker.OnSliderValueChanged(e);
        }

        private void OnHsvControlSelectedColorChanged(RoutedPropertyChangedEventArgs<Color> e)
        {
            // Avoid endless loop
            if (m_withinChange)
                return;

            m_withinChange = true;

            Color newColor = GetHsvColor();
            UpdateRgbColors(newColor);
            UpdateSelectedColor(newColor);

            m_withinChange = false;
        }

        private static void OnHsvControlSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            ColorPicker colorPicker = sender as ColorPicker;
            colorPicker.OnHsvControlSelectedColorChanged(e);
        }

        private void OnSelectedColorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!m_templateApplied)
                return;

            // Avoid endless loop
            if (m_withinChange)
                return;

            m_withinChange = true;

            try
            {
                Color oldColor = (Color)e.OldValue;
                Color newColor = (Color)e.NewValue;
                UpdateControlColors(oldColor, newColor);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }

            m_withinChange = false;
        }

        private static void OnSelectedColorPropertyChanged(
            DependencyObject relatedObject, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = relatedObject as ColorPicker;
            colorPicker.OnSelectedColorPropertyChanged(e);
        }

        private static void OnFixedSliderColorPropertyChanged(
            DependencyObject relatedObject, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = relatedObject as ColorPicker;
            colorPicker.UpdateColorSlidersBackground();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_redColorSlider = GetTemplateChild(RedColorSliderName) as ColorSlider;
            m_greenColorSlider = GetTemplateChild(GreenColorSliderName) as ColorSlider;
            m_blueColorSlider = GetTemplateChild(BlueColorSliderName) as ColorSlider;
            m_alphaColorSlider = GetTemplateChild(AlphaColorSliderName) as ColorSlider;
            m_spectrumSlider = GetTemplateChild(SpectrumSliderName) as SpectrumSlider;
            m_hsvControl = GetTemplateChild(HsvControlName) as HsvControl;

            m_templateApplied = true;
            UpdateControlColors(BeforeColor, SelectedColor);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == UIElement.IsVisibleProperty && (bool)e.NewValue == true)
                Focus();
            base.OnPropertyChanged(e);
        }

        #endregion

        #region Private Methods

        private void SetColorSliderBackground(ColorSlider colorSlider, Color leftColor, Color rightColor)
        {
            colorSlider.LeftColor  = leftColor;
            colorSlider.RightColor = rightColor;
        }

        private void UpdateColorSlidersBackground()
        {
            if (FixedSliderColor)
            {
                SetColorSliderBackground(m_redColorSlider, Colors.Red, Colors.Red);
                SetColorSliderBackground(m_greenColorSlider, Colors.Green, Colors.Green);
                SetColorSliderBackground(m_blueColorSlider, Colors.Blue, Colors.Blue);
                SetColorSliderBackground(m_alphaColorSlider, Colors.Transparent, Colors.White);
            }
            else
            {
                byte red   = SelectedColor.R;
                byte green = SelectedColor.G;
                byte blue  = SelectedColor.B;
                SetColorSliderBackground(m_redColorSlider,
                    Color.FromRgb(0, green, blue), Color.FromRgb(255, green, blue));
                SetColorSliderBackground(m_greenColorSlider,
                    Color.FromRgb(red, 0, blue), Color.FromRgb(red, 255, blue));
                SetColorSliderBackground(m_blueColorSlider,
                    Color.FromRgb(red, green, 0), Color.FromRgb(red, green, 255));
                SetColorSliderBackground(m_alphaColorSlider,
                    Color.FromArgb(0, red, green, blue), Color.FromArgb(255, red, green, blue));
            }
        }

        private Color GetRgbColor()
        {
            return Color.FromArgb(
                (byte)m_alphaColorSlider.Value,
                (byte)m_redColorSlider.Value,
                (byte)m_greenColorSlider.Value,
                (byte)m_blueColorSlider.Value);
        }

        private void UpdateRgbColors(Color newColor)
        {
            m_alphaColorSlider.Value = newColor.A;
            m_redColorSlider.Value   = newColor.R;
            m_greenColorSlider.Value = newColor.G;
            m_blueColorSlider.Value  = newColor.B;
        }

        private Color GetHsvColor()
        {
            Color hsvColor = m_hsvControl.SelectedColor;
            hsvColor.A = (byte)m_alphaColorSlider.Value;
            return hsvColor;
        }

        private void UpdateSpectrumColor(Color newColor)
        {
        }

        private void UpdateHsvControlHue(double hue)
        {
            m_hsvControl.Hue = hue;
        }


        private void UpdateHsvControlColor(Color newColor)
        {
            double hue, saturation, value;

            ColorUtils.ConvertRgbToHsv(newColor, out hue, out saturation, out value);

            // if saturation == 0 or value == 1 hue don't count so we save the old hue
            if (saturation != 0 && value != 0)
                m_hsvControl.Hue        = hue;

            m_hsvControl.Saturation = saturation;
            m_hsvControl.Value      = value;

            m_spectrumSlider.Hue = m_hsvControl.Hue;
        }

        private void UpdateSelectedColor(Color newColor)
        {
            BeforeColor = SelectedColor;
            BeforeColorText = BeforeColor.ToString();
            SelectedColor = newColor;
            SelectedColorText = SelectedColor.ToString();

            if (!FixedSliderColor)
                UpdateColorSlidersBackground();

            ColorUtils.FireSelectedColorChangedEvent(this, SelectedColorChangedEvent, BeforeColor, newColor);
        }

        private void UpdateControlColors(Color oldColor, Color newColor)
        {
            BeforeColor = oldColor;
            BeforeColorText = BeforeColor.ToString();
            UpdateRgbColors(newColor);
            UpdateSpectrumColor(newColor);
            UpdateHsvControlColor(newColor);
            UpdateColorSlidersBackground();
        }

        #endregion
    }
}
