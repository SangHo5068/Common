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
    }
}
