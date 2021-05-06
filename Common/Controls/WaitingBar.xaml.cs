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
        public static readonly DependencyProperty EasingColorStartProperty;
        public static readonly DependencyProperty EasingColorEndProperty;
        #endregion //DependencyProperty

        #region Properties
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public string SubMessage
        {
            get { return (string)GetValue(SubMessageProperty); }
            set { SetValue(SubMessageProperty, value); }
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
