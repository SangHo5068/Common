using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Common.Command;
using Common.Models;
using Common.Utilities;

namespace Common.Demo
{
    public class StartAppViewModel : BaseViewModel
    {
        private string _id;
        private string _pw;
        private string _ProductVersion;
        private string _StartingMessage;


        #region Properties
        public string ID
        {
            get => _id;
            set => SetValue(ref _id, value);
        }
        public string PW
        {
            get => _pw;
            set => SetValue(ref _pw, value);
        }

        public string ProductVersion
        {
            get => _ProductVersion;
            set => SetValue(ref _ProductVersion, value);
        }

        public string StartingMessage
        {
            get => _StartingMessage;
            set => SetValue(ref _StartingMessage, value);
        }
        #endregion //Properties

        #region ICommand
        public ICommand CommandMouseMove { get; private set; }
        public ICommand CommandLogin { get; private set; }
        public ICommand CommandDebugLogin { get; private set; }
        #endregion //ICommand



        public StartAppViewModel()
        {
            // 전역 인스턴스 생성
            _ = AppDefined.Instance;

            Header = Cultures.Resources.Starting;
            StartingMessage = Header + "...";
            IsWaiting = true;

            InitialData();
        }


        #region override

        protected override void DisposeManaged()
        {
            IsWaiting = false;

            base.DisposeManaged();

            Logger.WriteLog(LogTypes.Info, "StartAppViewModel Dispose");
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CommandMouseMove = new RelayCommand(OnWindowMouseMove);
        }

        public override async void InitialData(params object[] param)
        {
            base.InitialData(param);

            IsWaiting = true;
            Header = "Check Server";

            try
            {
                ID = INIHelper.ReadIniValue(AppDefined.LoginSession, nameof(ID), "", App.IniPath);
                PW = INIHelper.ReadIniValue(AppDefined.LoginSession, nameof(PW), "", App.IniPath);


                Defined.GitCommitNo = ThisAssembly.Git.Commits;

                //Assembly _asm = Assembly.LoadFile(fileName);
                var _asm = Assembly.GetExecutingAssembly();
                var assemblies = _asm.GetReferencedAssemblies();
                foreach (AssemblyName name in assemblies.OrderBy(n => n.Name))
                    Logger.WriteLog(LogTypes.Info, string.Format("StartApp\tAssemblyName : {0}, AssemblyVersion : {1}", name.Name, name.Version.ToString()));

                INIHelper.WriteIniValue(AppDefined.AppSession, "Commits", ThisAssembly.Git.Commits, App.IniPath);

                ProductVersion = Defined.AppBuildVersion;

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Logger.WriteLog(LogTypes.Info, string.Format("StartApp\tProductVersion : {0}", ProductVersion));
                Logger.WriteLog(LogTypes.Info, string.Format("StartApp\tFileVersion : {0}", version.ToString()));
                Logger.WriteLog(LogTypes.Info, string.Format("StartApp\tInitialData\tRelease : {0}", Defined.IsRelease));


                var startedServer = await StartingServerService();
                if (startedServer)
                {
                    //App.Current.Dispatcher.Invoke(new Action(() => {
                    StartMainWindow();
                    //}));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            finally
            {
                IsWaiting = false;
            }
        }

        #endregion //override



        #region Methods

        private void OnWindowMouseMove(object obj)
        {
            //if (!(obj is RoutedEventArgs args)) return;
            if (obj is Window window)
                window.DragMove();
        }

        /// <summary>
        /// 메인화면 표시
        /// </summary>
        private void StartMainWindow()
        {
            try
            {
                Logger.WriteLog(LogTypes.Info, "Start MainWindow");

                var window = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (window == null)
                {
                    window = new MainWindow();
                    window.Show();
                }
                else
                {
                    window.Focus();
                }
                App.Current.MainWindow = window;

                //this.Dispose();
                if (App.Current.Windows.OfType<StartApp>().FirstOrDefault() is StartApp startApp)
                    startApp.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }

        private async Task<bool> StartingServerService()
        {
            bool result = false;
            await Task.Run(() => result = StartDelay()).ConfigureAwait(false);
            
            await Task.Run(() => {
                return result;
            });
            return result;
        }

        private bool StartDelay()
        {
            try
            {
                int index = 0;
                do
                {
                    //App.Current.Dispatcher.Invoke(new Action(() => {
                    StartingMessage = "Starting Service " + index++.ToString();
                    //}));

                    Thread.Sleep(1000);
                } while (index <= 5);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return false;
        }

        #endregion //Methods
    }
}
