using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Controls
{
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
    #endregion //TextBlock

    #region Button
    public class CommonButton : Button { }
    public class PopupCommonButton : Button { }

    public class WindowMinButton : Button { }
    public class WindowMaxButton : Button { }
    public class WindowCloseButton : Button { }
    #endregion //Button
}
