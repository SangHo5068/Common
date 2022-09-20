using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

using Common.Utilities;


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

    /// <summary>
    /// The character set encoding option.
    /// </summary>
    public enum CharacterSetEncodingOption
    {
        /// <summary>
        /// The ascii.
        /// </summary>
        ASCII = 0,

        /// <summary>
        /// The big endian unicode.
        /// </summary>
        BigEndianUnicode = 1,

        /// <summary>
        /// The unicode.
        /// </summary>
        Unicode = 2,

        /// <summary>
        /// The ut f 32.
        /// </summary>
        UTF32 = 3,

        /// <summary>
        /// The ut f 7.
        /// </summary>
        UTF7 = 4,

        /// <summary>
        /// The ut f 8.
        /// </summary>
        UTF8 = 5,

        /// <summary>
        /// The default.
        /// </summary>
        Default = 6,
    }



    public class ResponseParameter
    {
        public virtual bool Result { get; set; }
        public virtual object Extra { get; set; }
        public virtual long SystemTime { get; set; }
        public virtual int Error { get; set; }
        public virtual string Message { get; set; }
        public virtual string RemoteIp { get; set; }
        public virtual string AccessToken { get; set; }

        public virtual int Index { get; set; }
        public virtual int TotalCount { get; set; }
    }


    /// <summary>
    /// HTTP 비동기 요청시 이벤트 인자.
    /// </summary>
    public class RequestEventArgs : EventArgs
    {
        public object Source { get; private set; }
        public object RequestUrl { get; private set; }
        public string[] RequestSegments { get; private set; }

        /// <summary>
        /// Gets a value indicating whether SuccessRequest.
        /// </summary>
        public bool SuccessRequest { get; private set; }

        /// <summary>
        /// Gets HttpResponseString.
        /// </summary>
        public string Response { get; private set; }

        public virtual ResponseParameter Result { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEventArgs"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="success">
        /// The success.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        public RequestEventArgs(object sender, bool success, string result)
        {
            this.Source = sender;
            this.SuccessRequest = success;
            this.Response = result;
            if (!string.IsNullOrEmpty(this.Response))
                this.Result = SerializeHelper.DeserializeByJsonCamel<ResponseParameter>(result);
            if (sender is HttpWebRequest req)
            {
                this.RequestUrl      = req.RequestUri;
                this.RequestSegments = req.Address.Segments;
            }
        }
    }

    /// <summary>
    /// The request parameter.
    /// </summary>
    public class RequestParameter
    {
        #region Properties
        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the encoding option.
        /// </summary>
        public CharacterSetEncodingOption EncodingOption { get; set; }

        /// <summary>
        /// Gets or sets the post message.
        /// </summary>
        public string PostMessage { get; set; }

        /// <summary>
        /// HTTP 등을 위한 인증 정보
        /// </summary>
        public System.Net.ICredentials Credentials { get; set; }

        public string AuthToken { get; set; }
        public string APIRoute { get; set; }
        #endregion //Properties



        #region Construct

        public RequestParameter()
        {
            // 기본을 UTF-8로 설정한다.
            this.EncodingOption = CharacterSetEncodingOption.UTF8;
        }
        public RequestParameter(string url, string route = null, string token = null)
            : this()
        {
            Url = url;
            AuthToken = token ?? ServiceRequest.Instance.AuthToken;
            APIRoute  = route?.ToUpper();
        }

        #endregion //Construct
    }
}
