using System;

using Common.Utilities;

namespace Common
{
    public static class Defined
    {
        public const string DateFormat = "yyyy.MM.dd";
        public const string DateSFormat = "yyyy.MM.dd HH:mm:ss";
        public const string DateLFormat = "yyyy.MM.dd HH:mm:ss.fff";
        public const string DateMinusFormat = "yyyy-MM-dd";
        public const string DateMinusSFormat = "yyyy-MM-dd HH:mm:ss";
        public const string DateMinusLFormat = "yyyy-MM-dd HH:mm:ss.fff";



#if DEBUG
        /// <summary>
        /// 빌드 구성 DEBUG(테스트용 버전)
        /// </summary>
        public const String BUILD = "T";
        /// <summary>
        /// 상용서버 사용인 경우 True
        /// </summary>
        public static Boolean IsRelease { get; set; } = false;
#else
        /// <summary>
        /// 빌드 구성 RELEASE(배포 버전)
        /// </summary>
        public const String BUILD = "R";
        /// <summary>
        /// 상용서버 사용인 경우 True
        /// </summary>
        public static Boolean IsRelease { get; set; } = true;
#endif


        


        private const String VersionFormat = "1.0.{0}";
        /// <summary>
        /// Git Commits Number
        /// </summary>
        public static String GitCommitNo { get; set; }
        /// <summary>
        /// 프로그램 버전
        /// </summary>
        private const String _LastVersion = "10";
        /// <summary>
        /// 프로그램 버전 "1.0.{GitCommitNo}"
        /// </summary>
        public static String AppVersion
        {
            //get => String.Format(VersionFormat, GitCommitNo);
            get => String.Format(VersionFormat, _LastVersion);
        }
        /// <summary>
        /// 프로그램 버전 "1.0.{GitCommitNo}.T"
        /// </summary>
        public static String AppBuildVersion
        {
            get => String.Format("{0}{1}", AppVersion, IsRelease ? string.Empty : BUILD);
        }
        public static String OSVersion
        {
            get => String.Format("{0} {1}", OSInfo.Name, OSInfo.Version);
        }


        private static Cryptography _Cryptography;
        /// <summary>
        /// 암호화
        /// </summary>
        public static Cryptography Cryptography => _Cryptography;
        public static void SetCryptography(string key) => _Cryptography = new Cryptography(key);
    }
}
