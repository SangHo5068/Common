using System;
using System.ComponentModel.DataAnnotations;

using Common;
using Common.Converters;

namespace ServiceBase
{
    public enum ResponseCode
    {
        [Display(Name = "Succeed")]
        Succeed = 200,
        [Display(Name = "Created")]
        Created = 201,
        [Display(Name = "Unauthorized")]
        Unauthorized = 401,
        [Display(Name = "Forbidden")]
        Forbidden = 403,
        [Display(Name = "Not Found")]
        Not_Found = 404,
        [Display(Name = "Internal Server Error")]
        ISE = 500,
    }

    public enum ServiceUrls
    {
        [Display(Name = "Util/{0}")]
        ping,

        [Display(Name = "Account/{0}")]
        user,

        [Display(Name = "Manager/{0}")]
        Manager,

        [Display(Name = "Measurement/{0}")]
        measurement,

        [Display(Name = "Room/{0}")]
        room,

        [Display(Name = "VCall/{0}")]
        vcall,
    }

    public enum ServerDomain
    {
        [Display(Name = "https://www.ht-release-api.mobicareconsole.com/")]
        HConnect,
        [Display(Name = "https://www.ht-release-api.mobicareconsole.com/")]
        Home,
        [Display(Name = "https://www.smarter-release-api.mobicareconsole.com/")]
        SmartER
    }

    public class ServiceUrl
    {
        public const string TestURL = "https://www.ht-release-api.mobicareconsole.com/";
        public const string ReleURL = "https://www.ht-release-api.mobicareconsole.com/";

        public const string TestCARDIO_WEBSITE = "https://www.testweb.seerscardio.com/";
        public const string ReleCARDIO_WEBSITE = "https://www.seerscardio.com/";

        //public static string BASEURL => Defined.IsRelease ? ReleURL : TestURL;
        public static ServerDomain BASE_DOMAIN { get; set; } = ServerDomain.SmartER;
        public static string GetBaseUrl(ServerDomain domain)
        {
            var converter = new EnumToDisplayConverter();
            var value = converter.Convert(domain, typeof(ServerDomain), null, null);
            return value.ToString();
        }
        public static string BASEURL => GetBaseUrl(BASE_DOMAIN);
        public static string BASEURL_WS => GetBaseUrl(BASE_DOMAIN).Replace("https://www.", "wss://") + "mobiCAREConsole/ws";
        public static string WEBSITE => Defined.IsRelease ? ReleCARDIO_WEBSITE : TestCARDIO_WEBSITE;

        /// <summary>
        /// Asia/Seoul
        /// </summary>
        public static string TimeZone { get; set; } = "Asia/Seoul";
        /// <summary>
        /// GMT+0900
        /// </summary>
        public static string GmtCode { get; set; } = "GMT+0900";

        //private const string UrlPrefix = "{0}mobiCARE/cardio/";
        private const string UrlPrefix = "{0}mobiCAREConsole/API/";

        #region url

        #region Util

        /// <summary>
        /// Checked Server
        /// </summary>
        public const string Ping = "CheckingPing";

        #endregion //Util

        #region User

        /// <summary>
        /// 사용자 로그인
        /// <BaseURL>/Account/Login
        /// </summary>
        public const string Login = "Login";

        #endregion //User

        #region Manager

        /// <summary>
        /// 병동의 병실 조회
        /// <BaseURL>/API/Manager/SelectSickRoom
        /// </summary>
        public const string SelectSickRoom = "SelectSickRoom";

        /// <summary>
        /// 병실의 병상 조회
        /// <BaseURL>/API/Manager/SelectSickBed
        /// </summary>
        public const string SelectSickBed = "SelectSickBed";

        #endregion //Manager

        #region Measurement

        /// <summary>
        /// 환자 측정 정보 리스트
        /// <BaseURL>/API/Measurement/SelectMeasurementInfoList
        /// </summary>
        public const string SelectMeasurementInfoList = "SelectMeasurementInfoList";

        /// <summary>
        /// 환자 측정 정보 상세
        /// <BaseURL>/API/Measurement/SelectMeasurementInfoList
        /// </summary>
        public const string SelectMeasurementInfoDetail = "SelectMeasurementInfoDetail";

        #endregion //Measurement

        #region Room

        /// <summary>
        /// 영상 통화 방 생성. 요청 생성자 ID필수
        /// <BaseURL>/API/Room/CreateRoom
        /// </summary>
        public const string CreateRoom = "CreateRoom";

        #endregion //Room

        #region VCall

        /// <summary>
        /// 화상통화 요청
        /// <BaseURL>/API/VCall/CreateVCall
        /// </summary>
        public const string CreateVCall = "CreateVCall";

        #endregion //VCall

        #endregion //url



        #region Methods

        /// <summary>
        /// Service url format을 반환하는 메서드.
        /// </summary>
        /// <example>"http://{0}:{1}/rest/data/{2}"</example>
        /// <param name="serviceUrl"></param>
        /// <param name="contract"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string GetServiceUrlFormat(ServiceUrls serviceUrl, string contract, params object[] param)
        {
            var returnContract = contract;
            var queryStrings = contract.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
            if (queryStrings.Length > 1) // has querystring.
            {
                var keys = queryStrings[1].Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                var index = 0;
                foreach (var key in keys)
                {
                    string value = param[index++].ToString();
                    returnContract = returnContract.Replace(key, $"{value}");
                }
                returnContract = returnContract.Replace("?", "");
                returnContract = returnContract.Replace("&", "/");
            }

            var converter = new Common.Converters.EnumToDisplayConverter();
            var format = converter.Convert(serviceUrl, typeof(ServiceUrls), null, null).ToString();
            var service = string.Format(format, returnContract);
            var servicePrefix = string.Format("{0}", service);

            return string.Concat(UrlPrefix, servicePrefix);
            //return string.Concat(UrlPrefix, returnContract);
        }

        #endregion //Methods
    }
}
