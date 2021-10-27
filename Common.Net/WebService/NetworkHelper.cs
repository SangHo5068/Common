using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

using Common.Utilities;

namespace ServiceBase
{
    /// <summary>
    ///     Each Program Type enum
    /// </summary>
    public enum ProgramType
    {
        Server,
        Client,
        DataServer,
        LogServer,
        EventServer,
    }

    public static class CertificationHelper
    {
        public const string certPassword = "1";

        public const string certServerFileName = "server.pfx";

        public const string certClientFileName = "client.cer";
    }

    /// <summary>
    /// The network helper.
    /// </summary>
    public static class NetworkHelper
    {
        private const string UrlPrefix = "{0}/api/";

        private static bool _IsCluster;
        private static bool _IsHttps;

        // Service 가 Cluster Mode 인지 확인하는 값
        public static bool IsCluster => _IsCluster;
        public static bool IsHttps  => _IsHttps;
        public static ProgramType ProgramType { get; private set; }



        static NetworkHelper()
        {
            // AppSettings 에 IsCluster 값이 정상적이지 않을 경우 false 반환
            bool.TryParse(ConfigurationManager.AppSettings["IsCluster"], out _IsCluster);
            bool.TryParse(ConfigurationManager.AppSettings["IsHttps"], out _IsHttps);
        }



        /// <summary>
        ///     set default values and program type
        /// </summary>
        /// <param name="type"></param>
        public static void Init(ProgramType type)
        {
            ProgramType = type;
        }

        /// <summary>
        /// The xelement.
        /// 현재 컴퓨터 주소들을 XML 형식으로 반환 함.
        /// </summary>
        /// <returns>
        /// The <see cref="XElement"/>.
        /// </returns>
        public static XElement GetLocalAddresses()
        {
            var root = new XElement("Addresses");

            IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (var item in address)
            {
                root.Add(new XElement("Address", item.ToString()));
            }

            return root;
        }

        /// <summary>
        /// The check url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The url string.
        /// </returns>
        public static string CheckUrl(string url)
        {
            bool isHttps = IsHttps;
            var val = url;
            var httpStr = GetHttpString(isHttps);
            val = isHttps ? val.Replace("http://", "https://") : val.Replace("https://", "http://");

            if (url.EndsWith("/") == false)
            {
                val += "/";
            }

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == false &&
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == false)
            {
                val = httpStr + val;
            }

            return val;
        }

        /// <summary>
        ///     Https 통신 시 HttpWebRequest의 자격증명을 설정해 줍니다.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="certData"></param>
        public static void SetRequestHttps(ref HttpWebRequest req, byte[] certData)
        {
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
            req.PreAuthenticate = true;
            req.AllowAutoRedirect = true;
            req.ClientCertificates.Add(new X509Certificate2(certData));
        }

        /// <summary>
        ///     어셈블리 내에 포함된 자격증명 데이터 파일의 데이터를 가져옵니다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] GetCertDataStream(ProgramType type)
        {
            var name = Assembly.GetEntryAssembly().GetName().Name;
            if (type == ProgramType.Client)
            {
                name = "ControlManager";
            }

            var certFileName = (type == ProgramType.Client || type == ProgramType.Server) ?
                                    CertificationHelper.certClientFileName : CertificationHelper.certServerFileName;

            var resourcePath = string.Format("{0}.{1}", name, certFileName);

            using (var sr = Assembly.GetEntryAssembly().GetManifestResourceStream(resourcePath))
            {
                if (sr == null)
                {
                    return null;
                }

                var ba = new byte[sr.Length];
                sr.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        ///     HTTP 인지 HTTPS 인지 판단해 URL 을 변경해 줍니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ChangeHttpsUrl(string url, bool isHttps = true)
        {
            return isHttps ? url.Replace("http://", "https://") : url.Replace("https://", "http://");
        }

        /// <summary>
        /// IP 주소가 *이라면 스스로가 사용하기 위해 사용할 IP로 변환해주는 함수
        /// </summary>
        /// <param name="inputAddress"></param>
        /// <returns></returns>
        public static string SubstituteGeneralAddress(string inputAddress, string requestUri = "127.0.0.1")
        {
            if (string.CompareOrdinal(inputAddress, "*") == 0)
            {
                return requestUri;
            }
            return inputAddress;
        }

        /// <summary>
        ///     return http or https string
        /// </summary>
        /// <param name="isHttps"></param>
        /// <returns></returns>
        public static string GetHttpString(bool isHttps)
        {
            return isHttps ? "https://" : "http://";
        }

        /// <summary>
        ///     ULR 에서 포트번호를 가져옴.
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns></returns>
        public static int GetPort(string urlString)
        {
            var tmpStr = urlString.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (tmpStr.Length < 3)
            {
                return 0;
            }

            var res = tmpStr[2].Split(new char[] { ':', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)[0].ToString();
            return Convert.ToInt32(res);
        }

        public static bool IsExistCertificate(string certName)
        {
            var res = false;
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            foreach (X509Certificate2 mCert in store.Certificates)
            {
                var name = mCert.Subject;
                if (certName == name)
                {
                    res = true;
                    break;
                }
            }
            store.Close();
            return res;
        }

        /// <summary>
        ///     Cert 포함 리소스 파일을 읽어옵니다.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static byte[] GetCertResourceStream(string resourceName)
        {
            using (var sr = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
            {
                if (sr == null)
                {
                    return null;
                }

                byte[] ba = new byte[sr.Length];
                sr.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        /// Service url format을 반환하는 메서드.
        /// </summary>
        /// <example>"http://{0}:{1}/rest/data/{2}"</example>
        /// <param name="serviceType"></param>
        /// <param name="contract"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string GetServiceUrlFormat(ServiceUrls serviceType, string contract, params object[] param)
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
            var format = converter.Convert(serviceType, typeof(ServiceUrls), null, null).ToString();
            var service = string.Format(format, serviceType);
            var servicePrefix = string.Format("{0}/", service);

            return string.Concat(UrlPrefix, servicePrefix, returnContract);
            //return string.Concat(UrlPrefix, returnContract);
        }

        ///// <summary>
        ///// Service url format을 반환하는 메서드.
        ///// </summary>
        ///// <example>"http://{0}:{1}/rest/data/{2}"</example>
        ///// <param name="serviceType"></param>
        ///// <param name="contract"></param>
        ///// <param name="startIndex"></param>
        ///// <returns></returns>
        //public static string GetServiceUrlFormat(ServiceTypes serviceType, string contract, int startIndex = 1)
        //{
        //    var returnContract = contract;
        //    var queryStrings = contract.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
        //    if (queryStrings.Length > 1) // has querystring.
        //    {
        //        var pairs = queryStrings[1].Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var pair in pairs)
        //        {
        //            var keyValue = pair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
        //            if (keyValue.Length != 2)
        //            {
        //                throw new ArgumentOutOfRangeException();
        //            }

        //            returnContract = returnContract.Replace(keyValue[1], "{" + startIndex + "}");
        //            startIndex++;
        //        }
        //    }

        //    var servicePrefix = string.Format("{0}/", serviceType);

        //    return string.Concat(UrlPrefix, servicePrefix, returnContract);
        //    //return string.Concat(UrlPrefix, returnContract);
        //}

        #region private static functions

        /// <summary>
        ///     포트 바인딩 된 Cert 항목을 제거합니다.
        /// </summary>
        /// <param name="port">binding port</param>
        /// <returns></returns>
        private static bool DeletePortBinding(int port)
        {
            var command = string.Format("http delete sslcert ipport=0.0.0.0:{0}", port);
            var process = new Process();
            process.StartInfo.FileName = "netsh.exe";
            process.StartInfo.Arguments = command;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            var errMsg = process.StandardError.ReadToEnd();
            var outMsg = process.StandardOutput.ReadToEnd();
            Logger.WriteLog(LogTypes.Info, string.Format("err msg : {0}", errMsg));
            Logger.WriteLog(LogTypes.Info, string.Format("out msg : {0}", outMsg));
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        /// <summary>
        ///     지정한 포트에 Cert 바인딩을 걸어줍니다.
        /// </summary>
        /// <param name="certData">cert data</param>
        /// <param name="password">cert password</param>
        /// <param name="port">binding port</param>
        private static bool BindCertToPort(byte[] certData, string password, int port)
        {
            var certificate = new X509Certificate2(certData, password);
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "netsh.exe");
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = string.Format("http add sslcert ipport=0.0.0.0:{0} certhash={1} appid={{{2}}}", port, certificate.Thumbprint, Guid.NewGuid());
            process.Start();
            var errMsg = process.StandardError.ReadToEnd();
            var outMsg = process.StandardOutput.ReadToEnd();
            Logger.WriteLog(LogTypes.Info, string.Format("err msg : {0}", errMsg));
            Logger.WriteLog(LogTypes.Info, string.Format("out msg : {0}", outMsg));
            if (errMsg.Length > 0 || outMsg.Contains("1312") || outMsg.Contains("오류") || outMsg.Contains("error") || outMsg.Contains("fail"))
            {
                process.WaitForExit();
                return false;
            }
            process.WaitForExit();
            return true;
        }

        /// <summary>
        ///     지정한 Cert 파일을 로컬 컴퓨터에서 Import(Install) 시켜줍니다.
        /// </summary>
        /// <param name="certPath"></param>
        /// <param name="password"></param>
        private static void ImportCert(string certPath, string password = "")
        {
            string cmd = string.IsNullOrEmpty(password) ?
            string.Format("-importPFX \"{0}\"", certPath) :
            string.Format("-f -p \"{0}\" -importPFX \"{1}\"", password, certPath);
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "certutil.exe");
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = cmd;
            process.Start();
            var errMsg = process.StandardError.ReadToEnd();
            var outMsg = process.StandardOutput.ReadToEnd();
            Logger.WriteLog(LogTypes.Info, string.Format("import cert err msg : {0}", errMsg));
            Logger.WriteLog(LogTypes.Info, string.Format("import cert out msg : {0}", outMsg));
            process.WaitForExit();

            var storeProcess = new Process();
            storeProcess.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "certutil.exe");
            storeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            storeProcess.StartInfo.CreateNoWindow = true;
            storeProcess.StartInfo.UseShellExecute = false;
            storeProcess.StartInfo.RedirectStandardError = true;
            storeProcess.StartInfo.Arguments = string.Format("-store my");
            storeProcess.Start();
            string storeErrMsg = process.StandardError.ReadToEnd();

            var errStoreMsg = process.StandardError.ReadToEnd();
            var outStoreMsg = process.StandardOutput.ReadToEnd();
            Logger.WriteLog(LogTypes.Info, string.Format("import cert store err msg : {0}", errStoreMsg));
            Logger.WriteLog(LogTypes.Info, string.Format("import cert store out msg : {0}", outStoreMsg));
            storeProcess.WaitForExit();
        }

        private static void ImportCert(X509Certificate2 cert)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
        }

        /// <summary>
        ///     지정한 Cert 파일을 로컬 컴퓨터에서 Delete(UnInstall) 시켜줍니다.
        /// </summary>
        /// <param name="certName"></param>
        private static void DelStoreCert(string certName)
        {
            string cName = certName.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Last();
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "certutil.exe");
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.Arguments = string.Format("-delstore my \"{0}\"", cName);
            process.Start();
            var errMsg = process.StandardError.ReadToEnd();
            var outMsg = process.StandardOutput.ReadToEnd();
            Logger.WriteLog(LogTypes.Info, string.Format("delete store cert err msg : {0}", errMsg));
            Logger.WriteLog(LogTypes.Info, string.Format("delete store cert out msg : {0}", outMsg));

            process.WaitForExit();
        }

        private static void DeleteCert(X509Certificate2 cert)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(cert);
        }
        #endregion
    }
}
