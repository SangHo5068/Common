using System;
using System.Windows;
using System.Windows.Input;

using Common.Base;
using Common.Command;
using Common.Demo.Views;
using Common.Utilities;

namespace Common.Demo
{
    public class MainWindowViewModel : BaseMainControlViewModel
    {
        private string _ProductVersion;
        private string _BuildTime;
        private BaseContentView _CurrentView;

        #region Properties
        public string ProductVersion
        {
            get => _ProductVersion;
            set => SetValue(ref _ProductVersion, value);
        }
        public string BuildTime
        {
            get => _BuildTime;
            set => SetValue(ref _BuildTime, value);
        }

        public BaseContentView CurrentView
        {
            get => _CurrentView;
            set => SetValue(ref _CurrentView, value);
        }
        #endregion //Properties

        #region ICommand
        public ICommand CommandMouseMove { get; private set; }
        #endregion //ICommand



        public MainWindowViewModel()
        {
            Header = "WPF Demo App";

            InitData(null);
        }



        #region override
        protected override void DisposeManaged()
        {
            base.DisposeManaged();
        }
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CommandMouseMove = new RelayCommand(OnWindowMouseMove);
        }
        protected override void InitData(object parameter)
        {
            base.InitData(parameter);

            var gitVer = GitHelper.VersionToString();
            ProductVersion = gitVer;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            BuildTime = Defined.Get_BuildDateTime(version).ToString(Defined.DateSFormat);

            CurrentView = new BaseContentView();
        }
        #endregion //override


        #region Methods
        private void OnWindowMouseMove(object obj)
        {
            try
            {
                if (obj is Window window)
                    window.DragMove();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }
        #endregion //Methods
    }
}
