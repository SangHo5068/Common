using System;
using System.ComponentModel.DataAnnotations;

using Common;

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
    }

    public class ServiceUrl
    {
        public const string TestURL = "https://www.testapi.seerscardio.com/";
        public const string ReleURL = "https://www.apirelease.seerscardio.com/";
        public const string TestCARDIO_WEBSITE = "https://www.testweb.seerscardio.com/";
        public const string ReleCARDIO_WEBSITE = "https://www.seerscardio.com/";

        public static string BASEURL => Defined.IsRelease ? ReleURL : TestURL;
        public static string WEBSITE => Defined.IsRelease ? ReleCARDIO_WEBSITE : TestCARDIO_WEBSITE;

        public static string TimeZone { get; set; } = "Asia/Seoul";
        public static string GmtCode { get; set; } = "GMT+0900";

        private const string UrlPrefix = "{0}mobiCARE/cardio/";

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
