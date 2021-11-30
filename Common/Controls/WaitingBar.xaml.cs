using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Common.Controls
{
    /// <summary>
    /// WaitingBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WaitingBar : UserControl
    {
        #region DependencyProperty
        public static readonly DependencyProperty MessageProperty;

        public static readonly DependencyProperty SubMessageProperty;
        public static readonly DependencyProperty SubFontSizeProperty;
        public static readonly DependencyProperty SubFontWeightProperty;
        public static readonly DependencyProperty SubForegroundProperty;

        public static readonly DependencyProperty LodingSizeProperty;
        public static readonly DependencyProperty EasingColorStartProperty;
        public static readonly DependencyProperty EasingColorEndProperty;
        #endregion //DependencyProperty

        #region Properties
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        #region Sub
        public string SubMessage
        {
            get { return (string)GetValue(SubMessageProperty); }
            set { SetValue(SubMessageProperty, value); }
        }
        public double SubFontSize
        {
            get { return (double)GetValue(SubFontSizeProperty); }
            set { SetValue(SubFontSizeProperty, value); }
        }
        public FontWeight SubFontWeight
        {
            get { return (FontWeight)GetValue(SubFontWeightProperty); }
            set { SetValue(SubFontWeightProperty, value); }
        }
        public Brush SubForeground
        {
            get { return (Brush)GetValue(SubForegroundProperty); }
            set { SetValue(SubForegroundProperty, value); }
        }
        #endregion //Sub

        public double LodingSize
        {
            get { return (double)GetValue(LodingSizeProperty); }
            set { SetValue(LodingSizeProperty, value); }
        }
        public SolidColorBrush EasingColorStart
        {
            get { return (SolidColorBrush)GetValue(EasingColorStartProperty); }
            set { SetValue(EasingColorStartProperty, value); }
        }
        public SolidColorBrush EasingColorEnd
        {
            get { return (SolidColorBrush)GetValue(EasingColorEndProperty); }
            set { SetValue(EasingColorEndProperty, value); }
        }
        #endregion //Properties



        static WaitingBar()
        {
            Type owner = typeof(WaitingBar);
            MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(WaitingBar), new PropertyMetadata(null));

            SubMessageProperty = DependencyProperty.Register(nameof(SubMessage), typeof(string), typeof(WaitingBar), new PropertyMetadata(null));
            SubFontSizeProperty = DependencyProperty.Register(nameof(SubFontSize), typeof(double), owner, new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits));
            SubFontWeightProperty = DependencyProperty.Register(nameof(SubFontWeight), typeof(FontWeight), owner, new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits));
            SubForegroundProperty = DependencyProperty.Register(nameof(SubForeground), typeof(Brush), owner, new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits));

            LodingSizeProperty = DependencyProperty.Register(nameof(LodingSize), typeof(double), owner, new FrameworkPropertyMetadata(60D));
            EasingColorStartProperty = DependencyProperty.Register(nameof(EasingColorStart), typeof(SolidColorBrush), typeof(WaitingBar), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
            EasingColorEndProperty = DependencyProperty.Register(nameof(EasingColorEnd), typeof(SolidColorBrush), typeof(WaitingBar), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(66, 00, 00, 00))));
        }
        public WaitingBar()
        {
            InitializeComponent();

            IsVisibleChanged += WaitingBar_IsVisibleChanged;
        }
        ~WaitingBar()
        {
            IsVisibleChanged -= WaitingBar_IsVisibleChanged;
        }



        private void WaitingBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var storyBoard = (Storyboard)FindResource("sbWaiting");
            if (storyBoard != null)
            {
                if ((bool)e.NewValue)
                    storyBoard.Begin();
                else
                    storyBoard.Stop();
            }
        }
    }
}
