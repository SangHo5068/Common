using System;

namespace Common.Demo
{
    public class AppDefined
    {
        #region Lazy Instance
        private static readonly Lazy<AppDefined> instance = new Lazy<AppDefined>(() => new AppDefined());

        /// <summary>
        /// 싱글턴 객체
        /// </summary>
        public static AppDefined Instance { get { return instance.Value; } }
        #endregion //Lazy Instance



        #region Properties
        /// <summary>
        /// App 세션
        /// </summary>
        public const String AppSession = "App";
        /// <summary>
        /// 로그인 세션
        /// </summary>
        public const String LoginSession = "Login";
        /// <summary>
        /// 언어 세션
        /// </summary>
        public const String LanguageSession = "Language";


        /// <summary>
        /// 프로그램 이름
        /// </summary>
        public const String AppName = "Common.Demo";
        /// <summary>
        /// 실행 파일 경로
        /// </summary>
        public static String IniPath { get; private set; } = AppDomain.CurrentDomain.BaseDirectory + @"App.ini";
        /// <summary>
        /// 로그 파일 경로
        /// </summary>
        public static String LogFilePath { get; private set; } = AppDomain.CurrentDomain.BaseDirectory + @"Logs";


        #endregion //Properties



        #region Methods

        #region Popup
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="content"></param>
        //public static void ClosePopup(BaseContentViewModel content = null)
        //{
        //    try
        //    {
        //        PopupWindow window = null;
        //        if (!App.Current.Dispatcher.CheckAccess())
        //        {
        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                window = App.Current.Windows.OfType<PopupWindow>().FirstOrDefault(f => f.Title.Equals(content?.Header));
        //                window?.Close();
        //            });
        //        }
        //        else
        //        {
        //            window = App.Current.Windows.OfType<PopupWindow>().FirstOrDefault(f => f.Title.Equals(content?.Header));
        //            window?.Close();
        //        }
        //        content?.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog(LogTypes.Exception, "", ex);
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="content"></param>
        ///// <param name="IsDialog"></param>
        //public static bool? ShowPopup(BaseContentViewModel content, bool IsDialog = false)
        //{
        //    bool? result = false;
        //    try
        //    {
        //        if (content == null)
        //            return result;

        //        PopupWindow window = App.Current.Windows.OfType<PopupWindow>().FirstOrDefault(f => f.Title.Equals(content.Header));

        //        if (window == null)
        //        {
        //            var main = App.Current.MainWindow as MainWindow;
        //            window = new PopupWindow
        //            {
        //                Owner = main,
        //                Height = content.Height,
        //                Width = content.Width
        //            };
        //        }

        //        if (IsDialog)
        //            result = window.ShowDialog(content);
        //        else
        //            window.Show(content);

        //        window.Focus();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog(LogTypes.Exception, "", ex);
        //    }
        //    return result;
        //}
        #endregion //Popup

        /// <summary>
        /// 버전 정보를 넣으면 빌드 시간을 반환.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static System.DateTime Get_BuildDateTime(System.Version version = null)
        {
            // 주.부.빌드.수정
            // 주 버전    Major Number
            // 부 버전    Minor Number
            // 빌드 번호  Build Number
            // 수정 버전  Revision NUmber

            //매개 변수가 존재할 경우
            if (version == null)
                version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //세번째 값(Build Number)은 2000년 1월 1일부터
            //Build된 날짜까지의 총 일(Days) 수 이다.
            int day = version.Build;
            System.DateTime dtBuild = (new System.DateTime(2000, 1, 1)).AddDays(day);

            //네번째 값(Revision NUmber)은 자정으로부터 Build된
            //시간까지의 지나간 초(Second) 값 이다.
            int intSeconds = version.Revision;
            intSeconds *= 2;
            dtBuild = dtBuild.AddSeconds(intSeconds);


            //시차 보정
            System.Globalization.DaylightTime daylingTime = System.TimeZone.CurrentTimeZone
                    .GetDaylightChanges(dtBuild.Year);
            if (System.TimeZone.IsDaylightSavingTime(dtBuild, daylingTime))
                dtBuild = dtBuild.Add(daylingTime.Delta);
            return dtBuild;
        }

        #endregion //Methods
    }
}
