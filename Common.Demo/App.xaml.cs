using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using Common.Cultures;
using Common.Demo.Properties;

using Common.Interop;

using Common.Utilities;

using Culture = Common.Cultures.Cultures.Resources;


namespace Common.Demo
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        #region Define

        private Mutex mutex;

        public static string IniPath => AppDefined.IniPath;
        #endregion //Define



        public App()
        {
        }



        #region Override

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            var ProjectName = Assembly.GetExecutingAssembly().GetName().Name;
#if !DEBUG
            mutex = new Mutex(true, ProjectName, out createNew);
#else
            createNew = true;
#endif
            try
            {
                if (!createNew)
                {
                    if (mutex != null)
                    {
                        mutex.Dispose();
                        mutex = null;
                    }
                    Logger.WriteLog(LogTypes.Info, String.Format("Mutex StartWindow Start : {0}", ProjectName));
                    Process currentProcess = Process.GetProcessesByName(ProjectName).FirstOrDefault();
                    if (currentProcess != null)
                    {
                        if ((Int32)currentProcess.MainWindowHandle > 0)
                        {
                            //같은 인스턴스네임이 있는 프로그램이 실행하려고 하면 현재 실행되는 프로세스 종료
                            Application.Current.Shutdown();

                            var windowStyle = WinApi.GetWindowLong(currentProcess.MainWindowHandle, -16);
                            WinApi.ShowWindow(currentProcess.MainWindowHandle, (windowStyle & (long)WinApi.WindowStyles.Maximize) != 0 ? (int)WinApi.ShowWindowMode.Maximize : (int)WinApi.ShowWindowMode.Restore);
                            //이미 실행되어 있는 프로세스에 포커스 이동.
                            WinApi.SetForegroundWindow(currentProcess.MainWindowHandle.ToInt32());
                            Logger.WriteLog(LogTypes.Info, "StartWindow SetForegroundWindow");
                        }
                    }
                }
                else
                {
                    Logger.WriteLog(LogTypes.Info, String.Format("StartWindow Start : {0}", ProjectName));

                    //DispatcherUnhandledException += Application_DispatcherUnhandledException;
                    this.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);
                    this.Dispatcher.UnhandledExceptionFilter += new DispatcherUnhandledExceptionFilterEventHandler(Dispatcher_UnhandledExceptionFilter);

                    base.OnStartup(e);

                    this.ShutdownMode = ShutdownMode.OnMainWindowClose;

                    Bootstrapper.Initialize();
                    //_ = CultureResources.Instance;

                    CultureInfo culture = null;
                    try
                    {
                        var language = INIHelper.ReadIniValue(AppDefined.LanguageSession, "Language", "", IniPath);
                            //language = String.IsNullOrEmpty(language) ? Settings.Default.DefaultCulture : language;
                        culture = CultureInfo.GetCultureInfo(language);

                        foreach (var item in Application.Current.Resources.MergedDictionaries)
                            Logger.WriteLogAndTrace(LogTypes.Info, string.Format("Resources [{0}]:", item.Source));
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog(LogTypes.Exception, "", ex);
                    }
                    finally
                    {
                        culture = culture ?? Settings.Default.DefaultCulture;
                        CultureResources.ResourceProvider.DataChanged += new EventHandler(ResourceProvider_DataChanged);
                        //initialise with default culture
                        Logger.WriteLogAndTrace(LogTypes.Info, string.Format("Set culture to default [{0}]:", culture));
                        //CultureResources.ChangeCulture(Settings.Default.DefaultCulture);
                        CultureResources.ChangeCulture(culture);
                    }

                    var window = new StartApp();
                    App.Current.MainWindow = window;
                    window.Show();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message + "\n----\n" + ex.StackTrace);
                Logger.WriteLog(LogTypes.Exception, "", ex);
#else
                MessageBox.Show(ex.Message);
                Logger.WriteLog(LogTypes.Exception, "", ex);   
#endif
            }
        }

        private void ResourceProvider_DataChanged(object sender, EventArgs e)
        {
            Logger.WriteLogAndTrace(LogTypes.Info, string.Format("ObjectDataProvider.DataChanged event. fetching culturename property for new culture [{0}]", Culture.Culture));
            Logger.WriteLogAndTrace(LogTypes.Info, string.Format("DataChanged event.  Language [{0}]", Culture.Language));

            var culture = Culture.Culture;
            INIHelper.WriteIniValue(AppDefined.LanguageSession, "Language", culture.Name, IniPath);
            Settings.Default.DefaultCulture = culture;

            //App.Current.Dispatcher.Thread.CurrentUICulture = culture;
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.WriteLog(LogTypes.Info, "Application OnExit");
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
                mutex = null;
            }
            base.OnExit(e);
        }

        #endregion //Override

        #region Events

        private void Dispatcher_UnhandledExceptionFilter(object sender, DispatcherUnhandledExceptionFilterEventArgs e)
        {
            e.RequestCatch = true;
        }

        /// <summary>
        /// 프로그램의 처리되지 않은 Exception을 처리한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            try
            {
                var thread = new Thread(delegate ()
                {
                    string message = ex.Exception.Message;
                    if (ex.Exception.InnerException != null)
                        message += Environment.NewLine + ex.Exception.InnerException.Message;
                    //MessageBox.Show(message);

                    if (!string.IsNullOrEmpty(message))
                        Logger.WriteLog(LogTypes.Exception, message);
                });

                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Priority = ThreadPriority.Highest;
                thread.Start();
            }
            catch (Exception InEx)
            {
                Logger.WriteLog(LogTypes.Exception, "", InEx);
            }
            ex.Handled = true;
        }

        #endregion //Events
    }
}
