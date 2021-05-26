using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Controls
{
    #region Border
    public class BorderMainHeader : Control
    {
        #region DependencyProperties
        public static readonly DependencyProperty CloseCommandProperty;
        public static readonly DependencyProperty HeaderProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        public static readonly DependencyProperty ImageProperty;
        public static readonly DependencyProperty ImageWidthProperty;
        public static readonly DependencyProperty ImageHeightProperty;

        public static readonly DependencyProperty IsShowMinProperty;
        public static readonly DependencyProperty IsShowMaxProperty;
        #endregion //DependencyProperties

        #region Properties
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public Visibility IsShowMin
        {
            get { return (Visibility)GetValue(IsShowMinProperty); }
            set { SetValue(IsShowMinProperty, value); }
        }
        public Visibility IsShowMax
        {
            get { return (Visibility)GetValue(IsShowMaxProperty); }
            set { SetValue(IsShowMaxProperty, value); }
        }
        #endregion //Properties



        static BorderMainHeader()
        {
            Type owner = typeof(BorderMainHeader);
            CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), owner, new PropertyMetadata(null));
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), owner, new PropertyMetadata(null));
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), owner, new PropertyMetadata(null));
            ImageWidthProperty = DependencyProperty.Register(nameof(ImageWidth), typeof(double), owner, new PropertyMetadata(36D));
            ImageHeightProperty = DependencyProperty.Register(nameof(ImageHeight), typeof(double), owner, new PropertyMetadata(36D));
            IsShowMinProperty = DependencyProperty.Register(nameof(IsShowMin), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
            IsShowMaxProperty = DependencyProperty.Register(nameof(IsShowMax), typeof(Visibility), owner, new PropertyMetadata(Visibility.Visible));
        }
        public BorderMainHeader()
        {

        }


        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            e.Handled = true;
        }
    }

    public class BorderMainFooter : ContentControl
    {
        #region DependencyProperties
        public static readonly DependencyProperty CornerRadiusProperty;
        public static readonly DependencyProperty ImageProperty;
        #endregion //DependencyProperties

        #region Properties
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        #endregion //Properties



        static BorderMainFooter()
        {
            Type owner = typeof(BorderMainFooter);
            CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), owner, new PropertyMetadata(new CornerRadius()));
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), owner, new PropertyMetadata(null));
        }
    }

    public class StrokeBorder : Border
    {
        #region DependencyProperties
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty FillProperty;
        public static readonly DependencyProperty StrokeDashArrayProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        #endregion //DependencyProperties

        #region Properties
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }
        public Thickness StrokeThickness
        {
            get { return (Thickness)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion //Properties


        static StrokeBorder()
        {
            Type owner = typeof(StrokeBorder);
            StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), owner, new PropertyMetadata(null));
            FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), owner, new PropertyMetadata(null));
            StrokeDashArrayProperty = DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), owner, new PropertyMetadata(new DoubleCollection()));
            StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(Thickness), owner, new PropertyMetadata(new Thickness(0)));
        }
    }
    #endregion //Border

    #region Button
    public class CommonButton : Button { }
    public class PopupCommonButton : Button { }

    public class WindowMinButton : Button { }
    public class WindowMaxButton : Button { }
    public class WindowCloseButton : Button { }

    public class LogoutButton : Button { }
    public class RadiusButton : Button
    {
        #region DependencyProperty
        public static DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(RadiusButton), new PropertyMetadata(null));
        #endregion //DependencyProperty

        #region Property

        #region CornerRadius
        /// <summary>
        /// CornerRadius
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        #endregion //CornerRadius

        #endregion //Property
    }
    public class RemoveTextButton : Button
    {
        /// <summary>
        /// Initializes the <see cref="CrossButton"/> class.
        /// </summary>
        static RemoveTextButton()
        {
            //  Set the style key, so that our control template is used.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RemoveTextButton), new FrameworkPropertyMetadata(typeof(RemoveTextButton)));
        }
    }
    #endregion //Button

    #region MessageBox
    public class MessageGrid : Grid { }
    public class MessagePanel : StackPanel { }
    public class MessageBorder : Border { }
    #endregion //MessageBox

    #region Panels
    public class PopupBottomPanel : StackPanel { }

    public class HorizonPanel : StackPanel { }
    public class VerticalPanel : StackPanel { }
    public class RightPanel : StackPanel { }
    #endregion //Panels

    #region PasswordBox
    public class WaterMarkPassword : ContentControl
    {
        private const string PART_Remove = "PART_Remove";
        private Button _Remove;

        #region DependencyProperty
        public static DependencyProperty WaterMarkProperty = DependencyProperty.Register(nameof(WaterMark), typeof(String), typeof(WaterMarkPassword), new PropertyMetadata(null));
        public static DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(String), typeof(WaterMarkPassword), new PropertyMetadata(null));
        public static DependencyProperty PasswordCharProperty = DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(WaterMarkPassword), new PropertyMetadata(null));
        #endregion //DependencyProperty

        #region Property

        #region WaterMark
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String WaterMark
        {
            get { return (String)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        #endregion //WaterMark

        #region Password
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String Password
        {
            get { return (String)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        #endregion //WaterMark

        #region PasswordChar
        /// <summary>
        /// PasswordChar
        /// </summary>
        public char PasswordChar
        {
            get { return (char)GetValue(PasswordCharProperty); }
            set { SetValue(PasswordCharProperty, value); }
        }
        #endregion //PasswordChar

        #endregion //Property

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild(PART_Remove) is Button btn)
            {
                _Remove = btn;
                _Remove.Click += (s, e) => { this.Password = string.Empty; };
            }
        }
    }
    #endregion //PasswordBox

    #region TextBlock
    /// <summary>
    /// TextBlock의 TextTrimed를 확인하기 위한 서비스
    /// </summary>
    public class TextBlockService
    {
        static TextBlockService()
        {
            // Register for the SizeChanged event on all TextBlocks, even if the event was handled.
            EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTextBlockSizeChanged), true);
        }

        public static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsTextTrimmed",
            typeof(bool),
            typeof(TextBlockService),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Boolean GetIsTextTrimmed(TextBlock target)
        {
            return (Boolean)target.GetValue(IsTextTrimmedProperty);
        }

        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlockEx textBlock = sender as TextBlockEx;
            if (null == textBlock)
                return;
            if (textBlock.IsTrimmingToolTip)
                textBlock.SetValue(IsTextTrimmedKey, calculateIsTextTrimmed(textBlock));
        }

        private static bool calculateIsTextTrimmed(TextBlock textBlock)
        {
            double width = textBlock.ActualWidth;
            if (textBlock.TextTrimming == TextTrimming.None)
                return false;
            if (textBlock.TextWrapping != TextWrapping.NoWrap)
                return false;
            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
            double totalWidth = textBlock.DesiredSize.Width;
            return width < totalWidth;
        }
    }

    /// <summary>
    /// TextTrimming 속성이 CharacterEllipsis 일 때 IsTrimmingToolTip="True" 이면 ToolTip를 표시한다.
    /// </summary>
    public class TextBlockEx : TextBlock
    {
        /// <summary>
        /// The ThreadBarrier's captured SynchronizationContext
        /// </summary>
        private SynchronizationContext _synchronizationContext;

        #region IsTrimmingToolTip
        public static readonly DependencyProperty IsTrimmingToolTipProperty =
            DependencyProperty.Register("IsTrimmingToolTip", typeof(Boolean), typeof(TextBlockEx), new PropertyMetadata(false));

        /// <summary>
        /// TextTrimming 속성이 CharacterEllipsis 일 때 툴팁 표시 여부
        /// </summary>
        public Boolean IsTrimmingToolTip
        {
            get { return (Boolean)this.GetValue(IsTrimmingToolTipProperty); }
            set
            {
                if (this._synchronizationContext != null)
                    this._synchronizationContext.Send(delegate { this.SetValue(IsTrimmingToolTipProperty, value); }, null);
                else
                    this.SetValue(IsTrimmingToolTipProperty, value);
            }
        }
        #endregion //IsTrimmingToolTip


        public TextBlockEx()
            : base()
        {
            this._synchronizationContext = AsyncOperationManager.SynchronizationContext;
        }
        static TextBlockEx()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlockEx), new FrameworkPropertyMetadata(typeof(TextBlockEx)));
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(TextBlockEx), new FrameworkPropertyMetadata { DefaultValue = 20 });
        }


        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            if (TextTrimming != TextTrimming.None && ToolTip != null)
                e.Handled = !IsTextTrimmed();
        }

        private bool IsTextTrimmed()
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, Foreground);
            return formattedText.Width > ActualWidth;
        }
    }
    public class PopupTextBlockHeader : TextBlockEx { }

    public class TextBlockMainHeader : TextBlock { }
    #endregion //TextBlock

    #region TextBox
    public class PopupTextBox : TextBox { }
    /// <summary>
    /// Watermark TextBox
    /// </summary>
    [TemplatePart(Name = PART_WaterMark, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    public class WaterMarkTextBox : TextBox
    {
        private const string PART_WaterMark = "PART_WaterMark";
        private const string PART_TextBox = "PART_TextBox";
        private const string PART_Remove = "PART_Remove";
        private TextBlock _WaterMarkTextBlock;
        private TextBox _TextBox;
        private Button _Remove;

        #region DependencyProperty
        public static DependencyProperty WaterMarkProperty = DependencyProperty.Register(nameof(WaterMark), typeof(String), typeof(WaterMarkTextBox), new PropertyMetadata(null));
        public static DependencyProperty IsEnabledRemoveProperty = DependencyProperty.Register(nameof(IsEnabledRemove), typeof(Boolean), typeof(WaterMarkTextBox), new PropertyMetadata(true));
        #endregion //DependencyProperty

        #region Property

        #region WaterMark
        /// <summary>
        /// TextBox 에 보여지는 WaterMark
        /// </summary>
        public String WaterMark
        {
            get { return (String)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }
        #endregion //WaterMark

        #region IsEnabledRemove
        /// <summary>
        /// 텍스트 삭제 버튼 표시 여부
        /// </summary>
        public Boolean IsEnabledRemove
        {
            get { return (Boolean)GetValue(IsEnabledRemoveProperty); }
            set { SetValue(IsEnabledRemoveProperty, value); }
        }
        #endregion //WaterMark

        #endregion //Property


        public WaterMarkTextBox()
            : base()
        {
        }


        #region Override
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _WaterMarkTextBlock = GetTemplateChild(PART_WaterMark) as TextBlock;
            _TextBox = GetTemplateChild(PART_TextBox) as TextBox;
            if (GetTemplateChild(PART_Remove) is Button btn)
            {
                _Remove = btn;
                _Remove.Click += (s, e) => { this.Text = string.Empty; };
            }
        }
        #endregion //Override
    }
    #endregion //TextBox

    #region Toggle
    public class EyeToggleButton : ToggleButton { }
    #endregion //Toggle
}
