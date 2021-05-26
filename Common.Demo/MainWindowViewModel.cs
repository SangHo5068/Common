using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Common.Base;
using Common.Command;
using Common.Utilities;

namespace Common.Demo
{
    public class MainWindowViewModel : BaseMainControlViewModel
    {
        private string _ProductVersion;
        private string _BuildTime;

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

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var gitVer = GitHelper.GitVersionToString();
            ProductVersion = gitVer;
            BuildTime = Defined.Get_BuildDateTime(version).ToString(Defined.DateSFormat);
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
