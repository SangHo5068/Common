using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Common.Utilities;


namespace ServiceBase
{
    /// <summary>
    /// Http 통신 클래스
    /// </summary>
    public class ServiceRequest
    {
        public const string POST = "POST";
        public const string GET  = "GET";

        public const string ContentType = "application/{0};charset={1}";
        public const string Multipart   = "multipart/form-data";

        public const string SXClientIP  = "SX-Client-IP";
        public const string SXAPIRoute  = "SX-API-ROUTE";
        public const string SXAuthToken = "SX-Auth-Token";

        private const bool IS_HTTPS = true;


        public string ClientIP { get; private set; }
        public string AuthToken { get; set; }

        #region Lazy Instance
        private static readonly Lazy<ServiceRequest> instance = new Lazy<ServiceRequest>(() => new ServiceRequest());

        /// <summary>
        /// 싱글턴 객체
        /// </summary>
        public static ServiceRequest Instance { get { return instance.Value; } }
        #endregion //Lazy Instance


        public ServiceRequest()
        {
            string localIP = "Not available, please check your network seetings!";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            ClientIP = localIP;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }


        #region Public Methods

        public static string Request(RequestParameter parameter, string method = GET, bool isHttps = IS_HTTPS)
        {
            try
            {
                var req = RequestMakeAndToken(parameter, method, isHttps);
                if (req == null)
                    return null;

                var result = GetResponseStream(parameter, req);
                Logger.WriteLog(LogTypes.WebInterface, $"[Receive ]\r\n{result}");
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[Web request Error]", ex);
                return string.Format("[REQUEST ERROR] {0}", ex.Message);
            }
        }

        /// <summary>
        /// HttpWebRequest
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="method">POST/GET</param>
        /// <param name="callback">CallBack Action</param>
        /// <param name="isHttps"></param>
        /// <param name="application">JSON</param>
        public static void BeginRequest(RequestParameter parameter, Action<RequestEventArgs> callback = null,
            string method = POST, bool isHttps = IS_HTTPS, string application = "json")
        {
            try
            {
                var req = RequestMakeAndToken(parameter, method, isHttps);
                if (req == null)
                    return;

                req.Timeout = callback == null ? 5000 : 100000;

                if (method == POST)
                {
                    var content = GetEncodingOption(parameter.EncodingOption).GetBytes(parameter.PostMessage);
                    req.ContentLength = content.Length;
                    req.BeginGetRequestStream(RequestCallback, new object[] { req, callback, content });
                }
                // GET
                else
                {
                    req.BeginGetResponse(ReceiveCallback, new object[] { req, callback });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[BeginRequest Error] {0}", ex);
            }
        }

        /// <summary>
        /// Method = form-data
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="callback"></param>
        /// <param name="isHttps"></param>
        /// <param name="path"></param>
        public static void BeginRequestFormData(RequestParameter parameter, Action<RequestEventArgs> callback = null,
            bool isHttps = IS_HTTPS)
        {
            try
            {
                var req = RequestMakeAndToken(parameter, POST, isHttps);
                if (req == null)
                    return;

                Logger.WriteLog(LogTypes.WebInterface, $"[Request][{POST}] {parameter.Url}\r\n{parameter.PostMessage}");

                // RequestStream 에 데이터 추가
                UploadFilesStream(req, new string[] { parameter.PostMessage });

                req.Timeout = callback == null ? 5000 : 100000;
                // 비동기로 Response 호출.
                req.BeginGetResponse(ReceiveCallback, new object[] { req, callback });
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[BeginRequest_PostData Error] {0}", ex);
            }
        }

        /// <summary>
        /// The request to stream response.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="callback">
        /// The is Callback Action.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        public static void BeginRequest_ToStream(RequestParameter parameter, Action<Stream, long> callback = null)
        {
            Stream stream = null;
            long ContentLength = 0;

            try
            {
                var req = RequestMakeAndToken(parameter);
                if (req == null)
                    return;

                var webResponse = req.GetResponse();
                stream = webResponse?.GetResponseStream();
                ContentLength = webResponse.ContentLength;
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[BeginRequest_ToStream Error] {0}", ex);
            }
            finally
            {
                callback?.Invoke(stream, ContentLength);
            }
        }

        #endregion //Public Methods

        #region private Methods

        /// <summary>
        /// Request 를 생성하고 Token 을 추가한다
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="method"></param>
        /// <param name="isHttps"></param>
        /// <returns></returns>
        private static HttpWebRequest RequestMakeAndToken(RequestParameter parameter, string method = GET,
            bool isHttps = IS_HTTPS)
        {
            var req = MakeRequestOnly(parameter, method, isHttps);
            if (req == null)
                return null;

            string msg = $"[Request][{method}] {parameter.Url}";
            if (method == POST)
                msg += $"\r\n{ parameter.PostMessage}";
            Logger.WriteLog(LogTypes.WebInterface, msg);
            return req;
        }

        /// <summary>
        /// The web request only.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The System.Net.HttpWebRequest.
        /// </returns>
        private static HttpWebRequest MakeRequestOnly(RequestParameter parameter, string method = GET,
            bool isHttps = IS_HTTPS, string application = "json")
        {
            try
            {
                var url = isHttps ? NetworkHelper.ChangeHttpsUrl(parameter.Url) : parameter.Url;

                if (WebRequest.Create(url) is HttpWebRequest req)
                {
                    req.Timeout = 100000; // default time out.
                    req.Method = method;
                    //req.Proxy = null;
                    //req.SendChunked = false;
                    //req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    //req.Headers[HttpRequestHeader.ContentType] = "application/" + application + ";charset=" + GetHttpCharset(parameter.EncodingOption);
                    req.ContentType = String.Format(ContentType, application, GetHttpCharset(parameter.EncodingOption));
                    req.Headers[SXClientIP] = Instance.ClientIP;

                    RequestAddHeader(req, SXAuthToken, parameter.AuthToken);
                    RequestAddHeader(req, SXAPIRoute,  parameter.APIRoute);

                    if (parameter.Credentials != null)
                        req.Credentials = parameter.Credentials;

                    if (NetworkHelper.IsHttps)
                    {
                        var certData = NetworkHelper.GetCertDataStream(NetworkHelper.ProgramType);
                        NetworkHelper.SetRequestHttps(ref req, certData);
                    }

                    return req;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[MakeRequestOnly Error] {0}", ex);
            }
            return null;
        }

        /// <summary>
        /// Request Header 정보 추가
        /// </summary>
        /// <param name="req"></param>
        /// <param name="key">Header Key</param>
        /// <param name="value">Header Value</param>
        private static void RequestAddHeader(HttpWebRequest req, string key, string value)
        {
            try
            {
                // 토큰값 추가
                if (!string.IsNullOrEmpty(value))
                    req.Headers.Add(key, value);
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[RequestAddToken Error] {0}", ex);
            }
        }

        /// <summary>
        /// 업로드 데이터 작성
        /// </summary>
        /// <param name="request"></param>
        /// <param name="files"></param>
        /// <param name="formFields"></param>
        private static void UploadFilesStream(HttpWebRequest request, string[] files,
            System.Collections.Specialized.NameValueCollection formFields = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.KeepAlive = true;

            using (Stream memStream = new MemoryStream())
            {
                var boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                var endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");

                var formdataTemplate = "\r\n--" + boundary +
                                            "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

                if (formFields != null)
                {
                    foreach (string key in formFields.Keys)
                    {
                        string formitem = string.Format(formdataTemplate, key, formFields[key]);
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        memStream.Write(formitembytes, 0, formitembytes.Length);
                    }
                }

                var headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                    "Content-Type: application/octet-stream\r\n\r\n";

                for (int i = 0; i < files.Length; i++)
                {
                    memStream.Write(boundarybytes, 0, boundarybytes.Length);
                    var header = string.Format(headerTemplate, "file", files[i]);
                    var headerbytes = Encoding.UTF8.GetBytes(header);

                    memStream.Write(headerbytes, 0, headerbytes.Length);

                    using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                    {
                        var buffer = new byte[1024];
                        var bytesRead = 0;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            memStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }
            }
        }



        private static string ExecuteHttpWebRequest(RequestParameter parameter, bool isHttps = IS_HTTPS)
        {
            var req = MakeRequestOnly(parameter, GET, isHttps);
            if (req == null)
                return null;

            return GetResponseStream(parameter, req);
        }

        private static string GetResponseStream(RequestParameter parameter, HttpWebRequest req, string application = "json")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(parameter.PostMessage))
                {
                    req.Method = POST;
                    var byteArray = GetEncodingOption(parameter.EncodingOption).GetBytes(parameter.PostMessage);

                    // Set the ContentType property of the WebRequest.
                    //req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentType = $"application/{application}";
                    req.ContentLength = byteArray.Length;

                    var dataStream = req.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                using (var webResponse = req.GetResponse())
                {
                    using (var stream = webResponse?.GetResponseStream())
                    {
                        if (stream == null)
                        {
                            return string.Empty;
                        }

                        if (String.CompareOrdinal(webResponse.ContentType, "application/zip") == 0)
                        {
                            using (var decompressedFileStream = new MemoryStream())
                            {
                                using (var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedFileStream);
                                    var encodingOption = GetEncodingOption(parameter.EncodingOption);
                                    var resultArray = encodingOption.GetString(decompressedFileStream.ToArray());
                                    return resultArray;
                                }
                            }
                        }
                        else
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[GetResponseStream Error] {0}", ex);
            }
            return string.Empty;
        }


        /// <summary>
        /// POST 전용 비동기 Callback.
        /// </summary>
        /// <param name="asyncResult">The IAsyncResult.</param>
        private static void RequestCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (!(asyncResult.AsyncState is object[] state))
                    return;

                var callback = state[1] as Action<RequestEventArgs>;
                if (!(state[0] is HttpWebRequest req) ||
                    !(state[2] is byte[] content))
                    return;

                using (var request = req.EndGetRequestStream(asyncResult))
                {
                    request.Write(content, 0, content.Length);
                }

                // 비동기로 Response 호출.
                req.BeginGetResponse(ReceiveCallback, new object[] { req, callback });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.WebInterface, $"[Request]", ex);
                Logger.WriteLogAndTrace(LogTypes.Exception, "[RequestCallback Error] {0}", ex);
                //Console.WriteLine(ex);
            }
        }

        private static void ReceiveCallback(IAsyncResult asyncResult)
        {
            if (!(asyncResult.AsyncState is object[] state))
                return;

            if (!(state[0] is HttpWebRequest req) ||
                !(state[1] is Action<RequestEventArgs> callback))
                return;

            string result = string.Empty;
            try
            {
                using (var response = req.EndGetResponse(asyncResult))
                {
                    using (var stream = response.GetResponseStream())
                    {
                        //var res_header = (response as HttpWebResponse)?.GetResponseHeader(TOKENKEY);
                        var encoding = GetEncodingFromContentType(Encoding.UTF8, response);
                        using (var streamReader = new StreamReader(stream, encoding))
                        {
                            //callback?.BeginInvoke(new HttpRequestEndEventArgs(req, true, streamReader.ReadToEnd()), new AsyncCallback(ReceiveCallback), state);
                            //callback?.Invoke(new HttpRequestEndEventArgs(req, true, streamReader.ReadToEnd()));
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, $"[ReceiveCallback Error] {ex.Message}");
                var message = new ResponseParameter() { Result = false, Message = ex.Message };
                result = SerializeHelper.SerializeByJsonCamel(message);
            }
            finally
            {
                Logger.WriteLog(LogTypes.WebInterface, $"[Receive ]\r\n{result}");
                callback?.Invoke(new RequestEventArgs(req, !string.IsNullOrEmpty(result), result));
            }
        }



        #region Convert Encoding

        private static Encoding GetEncodingOption(CharacterSetEncodingOption encodingOption)
        {
            Encoding encoding;

            // Contents에 지정된 EncodingOption을 가져와서 설정한다.
            switch (encodingOption)
            {
                case CharacterSetEncodingOption.ASCII:
                    encoding = Encoding.ASCII;
                    break;
                case CharacterSetEncodingOption.BigEndianUnicode:
                    encoding = Encoding.BigEndianUnicode;
                    break;
                case CharacterSetEncodingOption.Unicode:
                    encoding = Encoding.Unicode;
                    break;
                case CharacterSetEncodingOption.UTF32:
                    encoding = Encoding.UTF32;
                    break;
                case CharacterSetEncodingOption.UTF7:
                    encoding = Encoding.UTF7;
                    break;
                case CharacterSetEncodingOption.UTF8:
                    encoding = Encoding.UTF8;
                    break;
                case CharacterSetEncodingOption.Default:
                    encoding = Encoding.Default;
                    break;
                default:
                    encoding = Encoding.UTF8;
                    break;
            }

            return encoding;
        }

        private static Encoding GetEncodingFromContentType(Encoding defaultDecoderEncodingOption, WebResponse response)
        {
            try
            {
                if (response is HttpWebResponse httpResponse)
                {
                    if (!string.IsNullOrEmpty(httpResponse.CharacterSet))
                        return Encoding.GetEncoding(httpResponse.CharacterSet);
                    return defaultDecoderEncodingOption;
                }

                var contentType = response.ContentType;
                if (contentType == null)
                    return defaultDecoderEncodingOption;
                var ind = contentType.IndexOf("charset=", System.StringComparison.Ordinal);
                if (ind != -1)
                {
                    var charset = contentType.Substring(ind + "charset=".Length);
                    if (charset.StartsWith("\"") && charset.EndsWith("\""))
                    {
                        charset = charset.Substring(1, charset.Length - 2);
                    }
                    defaultDecoderEncodingOption = Encoding.GetEncoding(charset);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[GetEncodingFromContentType Error] {0}", ex);
            }
            return defaultDecoderEncodingOption;
        }

        private static string GetHttpCharset(CharacterSetEncodingOption encodingOption)
        {
            string charset;

            switch (encodingOption)
            {
                case CharacterSetEncodingOption.ASCII:
                    charset = "us-ascii";
                    break;
                case CharacterSetEncodingOption.BigEndianUnicode:
                    charset = "utf-16be";
                    break;
                case CharacterSetEncodingOption.Unicode:
                    charset = "utf-16le";
                    break;
                case CharacterSetEncodingOption.UTF32:
                    charset = "utf-32le";
                    break;
                case CharacterSetEncodingOption.UTF7:
                    charset = "utf-7";
                    break;
                case CharacterSetEncodingOption.UTF8:
                    charset = "utf-8";
                    break;
                case CharacterSetEncodingOption.Default:
                    charset = Encoding.Default.WebName;
                    break;
                default:
                    charset = "utf-8";
                    break;
            }

            return charset;
        }

        #endregion //Convert Encoding

        #endregion //private Methods
    }
}
