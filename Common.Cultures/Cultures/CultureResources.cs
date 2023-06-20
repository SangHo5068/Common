using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;

using Common.Cultures.Cultures;
using Common.Utilities;

namespace Common.Cultures
{
    /// <summary>
    /// Wraps up XAML access to instance of Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources
    {
        #region Lazy Instance
        //private static readonly Lazy<Resources> instance = new Lazy<Resources>(() => new Resources());

        ///// <summary>
        ///// 싱글턴 객체
        ///// </summary>
        //public static Resources Instance { get { return instance.Value; } }
        #endregion //Lazy Instance

        //only fetch installed cultures once
        private static readonly bool bFoundInstalledCultures = false;
        private static readonly List<CultureInfo> pSupportedCultures = new List<CultureInfo>();

        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the Properties.Resources class
        /// </summary>
        /// <returns></returns>
        public Resources GetResourceInstance()
        {
            return new Resources();
        }

        private static ObjectDataProvider _provider;
        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (_provider == null)
                    _provider = (ObjectDataProvider)System.Windows.Application.Current.FindResource("CultureResources");
                return _provider;
            }
        }

        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public static List<CultureInfo> SupportedCultures
        {
            get { return pSupportedCultures; }
        }

        static CultureResources()
        {
            if (!bFoundInstalledCultures)
            {
                const string dll = ".resources.dll";
                //determine which cultures are available to this application
                Logger.WriteLogAndTrace(LogTypes.Info, "Get Installed cultures:");

                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var path = Environment.CurrentDirectory;
                foreach (string dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        //see if this directory corresponds to a valid culture name
                        var dirinfo = new DirectoryInfo(dir);

                        //determine if a resources dll exists in this directory that matches the executable name
                        //if (dirinfo.GetFiles(Path.GetFileNameWithoutExtension(path) + ".resources.dll").Length > 0)
                        if (dirinfo.GetFiles(name + dll).Length > 0)
                        {
                            var tCulture = CultureInfo.GetCultureInfo(dirinfo.Name);
                            pSupportedCultures.Add(tCulture);
                            Logger.WriteLogAndTrace(LogTypes.Info, string.Format("Found Culture: {0} [{1}]", tCulture.DisplayName, tCulture.Name));
                        }
                    }
                    catch (ArgumentException ex) //ignore exceptions generated for any unrelated directories in the bin folder
                    {
                        Logger.WriteLog(LogTypes.Exception, "", ex);
                    }
                }
                bFoundInstalledCultures = true;
            }
        }

        /// <summary>
        /// Change the current culture used in the application.
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        /// <param name="culture">Culture to change to</param>
        public static void ChangeCulture(CultureInfo culture)
        {
            //remain on the current culture if the desired culture cannot be found
            // - otherwise it would revert to the default resources set, which may or may not be desired.
            if (pSupportedCultures.Contains(culture))
            {
                Resources.Culture = culture;
                ResourceProvider.Refresh();
            }
            else
                Logger.WriteLogAndTrace(LogTypes.Info, string.Format("Culture [{0}] not available", culture));
        }
    }
}
