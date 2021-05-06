using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using log4net;
using log4net.Config;

namespace Common.Utilities
{
    /// <summary>
    /// ALL
    /// DEBUG
    /// INFO
    /// WARN
    /// ERROR,FATAL
    /// </summary>
    public enum LogTypes
    {
        Debug,
        Info,
        /// <summary>
        /// H/W Interface
        /// </summary>
        Interface,
        /// <summary>
        /// Rest Message
        /// </summary>
        WebInterface,
        Error,
        Warning,
        Exception,
    }

    public static class LogFactory
    {
        public const string ConfigFileName = "log4net.config";
        public static string LogPath { get; private set; }

        public static void Configure()
        {
            Type type = typeof(LogFactory);
            //FileInfo assemblyDirectory = AssemblyInfo.GetCodeBaseDirectory(type);
            //string path = Path.Combine(assemblyDirectory.FullName, ConfigFileName);
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDirectory, ConfigFileName);

            FileInfo configFile = new FileInfo(path);
            XmlConfigurator.ConfigureAndWatch(configFile);
            log4net.ILog log = LogManager.GetLogger(type);
            log.ToString();

            LogPath = baseDirectory + @"Logs\";
        }
    }

    public static class SecurityExtensions
    {
        static readonly log4net.Core.Level InterfaceLevel = new log4net.Core.Level(300000, "Interface");
        static readonly log4net.Core.Level WebInterfaceLevel = new log4net.Core.Level(310000, "WebInterface");

        public static void Interface(this ILog log, string message, Exception ex)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                InterfaceLevel, message, ex);
        }

        public static void InterfaceFormat(this ILog log, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                InterfaceLevel, formattedMessage, null);
        }

        public static void WebInterface(this ILog log, string message, Exception ex)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                WebInterfaceLevel, message, ex);
        }

        public static void WebInterfaceFormat(this ILog log, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                WebInterfaceLevel, formattedMessage, null);
        }
    }

    /// <summary>
    /// xcopy /Y /R "$(ProjectDir)..\Common\log4net.config" "$(ProjectDir)"
    /// 
    /// LEVEL [ALL DEBUG INFO WARN ERROR FATAL OFF]
    /// </summary>
    public static class Logger
    {
        private static readonly object locker = new object();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Logger()
        {
            //XmlConfigurator.Configure();
            LogFactory.Configure();
        }

        public static void WriteLog(LogTypes logTypes, string logMessage, Exception ex = null)
        {
            lock (locker)
            {
                switch (logTypes)
                {
                    case LogTypes.Debug:
                        Log.Debug(logMessage, ex);
                        break;
                    case LogTypes.Info:
                        Log.Info(logMessage, ex);
                        break;
                    case LogTypes.Interface:
#if DEBUG
                        Log.Interface(logMessage, ex);
#endif
                        break;
                    case LogTypes.WebInterface:
#if DEBUG
                        Log.WebInterface(logMessage, ex);
#endif
                        break;
                    case LogTypes.Warning:
                        Log.Warn(logMessage, ex);
                        break;
                    case LogTypes.Error:
                        Log.Error(logMessage, ex);
                        break;
                    default:
                        WriteException(logMessage, ex);
                        break;
                }

                DeleteLogFile();
            }
        }

        /// <summary>
        /// The write line.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void WriteLogAndTrace(LogTypes logTypes, string logMessage, Exception ex = null)
        {
            WriteLog(logTypes, logMessage, ex);
            string debug = logTypes >= LogTypes.Error ? "[{0} Error] {1} \r\n{2}" : string.Empty;
            if (ex != null)
                Debug.WriteLine(debug, GetMethodInfoStrings.GetMethodName(2), logMessage, ex);
            else
            {
                //Debug.WriteLine(logMessage);
                Debug.WriteLine(
                    "[" + MethodBase.GetCurrentMethod().ReflectedType.FullName + "] " +
                    "[" + MethodBase.GetCurrentMethod().Name + "] " +
                    logMessage);
            }
        }

        private static void WriteException(string logMessage, Exception ex)
        {
            var dataString = string.Empty;
            if (ex != null)
            {
                dataString += logMessage;
                dataString += $"\nError Message : {ex.Message}\n";
                if (null != ex.InnerException)
                {
                    dataString += $"\nInnerException Message : {ex.InnerException.Message}\n";
                }
            }
            else
            {
                dataString += logMessage;
            }

            Log.Fatal(dataString, ex);
        }


        #region Debug Write

        /// <summary>
        /// TRACE method.
        /// </summary>
        /// <param name="originClass">
        /// The origin class of the message.
        /// </param>
        /// <param name="priority">
        /// The priority of the message.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(object originClass, int priority, string message, params object[] obj)
        {
#if DEBUG
            DoTrace(obj, message, originClass, priority);
#endif
        }

        /// <summary>
        /// The TRACE method.
        /// </summary>
        /// <param name="priority">
        /// Priority of the message.
        /// </param>
        /// <param name="message">
        /// The Message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(int priority, string message, params object[] obj)
        {
            Trace((object)null, priority, message, obj);
        }

        /// <summary>
        /// TRACE method.
        /// </summary>
        /// <param name="message">
        /// The Message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(string message, params object[] obj)
        {
            Trace(-1, message, obj);
        }

        /// <summary>
        /// The do trace.
        /// 디버깅용 트레이스 함수.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="originClass">
        /// The origin class.
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        private static void DoTrace(object[] obj, string message, object originClass, int priority)
        {
            var count = 0;
            foreach (var strObject in obj)
            {
                string strSubFormat = "{" + count + "}";

                message = strObject == null ? message.Replace(strSubFormat, "null") : message.Replace(strSubFormat, strObject.ToString());

                count++;
            }

            string className = string.Empty;

            if (originClass != null)
            {
                className = originClass.GetType().Name;
            }

            var overallMessage = new StringBuilder();

            if (originClass != null)
            {
                overallMessage.Append("[" + className + "] ");
            }

            overallMessage.Append(message);
            if (priority != -1)
            {
                overallMessage.Append("(Priority : " + priority + ")");
            }

            // System.Diagnostics.Trace.WriteLine(overallMessage.ToString());
            Debug.WriteLine(overallMessage.ToString());
        }

        #endregion //Debug Write

        private static void DeleteLogFile()
        {
            try
            {
                string folderPath = LogFactory.LogPath;
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                if (dirInfo != null)
                {
                    foreach (var dir in dirInfo.EnumerateDirectories())
                    {
                        if (dir.CreationTime < DateTime.Now.AddDays(-30))
                            dir.Delete(true);
                    }
                }
                dirInfo = null;
            }
            catch (Exception ex)
            {
                WriteException("Log File Delete Error", ex);
            }
        }
    }
}
