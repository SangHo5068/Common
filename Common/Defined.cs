using System;

namespace Common
{
    public static class Defined
    {
        public const string DateSFormat = "yyyy.MM.dd HH:mm:ss";
        public const string DateLFormat = "yyyy.MM.dd HH:mm:ss.fff";
        public const string DateMinusSFormat = "yyyy-MM-dd HH:mm:ss";
        public const string DateMinusLFormat = "yyyy-MM-dd HH:mm:ss.fff";



#if DEBUG
        /// <summary>
        /// 빌드 구성 DEBUG(테스트용 버전)
        /// </summary>
        public const String BUILD = "T";
        public static Boolean IsRelease { get; set; } = false;
#else
        /// <summary>
        /// 빌드 구성 RELEASE(배포 버전)
        /// </summary>
        public const String BUILD = "R";
        public static Boolean IsRelease { get; set; } = true;
#endif



        private const String VersionFormat = "2.0.{0}";
        /// <summary>
        /// Git Commits Number
        /// </summary>
        public static String GitCommitNo { get; set; }
        /// <summary>
        /// 프로그램 버전 "1.0.{GitCommitNo}"
        /// </summary>
        public static String AppVersion
        {
            get => String.Format(VersionFormat, GitCommitNo);
        }
        /// <summary>
        /// 프로그램 버전 "1.0.{GitCommitNo}.T"
        /// </summary>
        public static String AppBuildVersion
        {
            get => String.Format("{0}{1}", AppVersion, IsRelease ? string.Empty : BUILD);
        }

        /// <summary>
        /// 
        /// </summary>
        public const String EtcName = "퀀텀스타";
        public static string AppName { get { return AppDomain.CurrentDomain.FriendlyName; } }

        #region Methods

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
