using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows;

using ImageConverter = Common.Converters.ImageConverter;

namespace Common.Utilities
{
    public static class FindResources
    {
        #region Resources

        public static object LoadProjectResource(Assembly assembly, string strResName)
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                return null;

            string strBaseName = assembly.GetName().Name + "." + "Properties.Resources";

            ResourceManager rm = new ResourceManager(strBaseName, assembly);

            return rm.GetObject(strResName);    // load resource from item name
        }

        public static T GetAssemblyResource<T>(Assembly assembly, string strResName)
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();

            string strBaseName = assembly.GetName().Name + "." + "Properties.Resources";

            ResourceManager rm = new ResourceManager(strBaseName, assembly);

            return (T)rm.GetObject(strResName);    // load resource from item name
        }

        public static T GetAssemblyResource<T>(string strResName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string strBaseName = assembly.GetName().Name + "." + "Properties.Resources";

            ResourceManager rm = new ResourceManager(strBaseName, assembly);

            return (T)rm.GetObject(strResName);    // load resource from item name
        }

        public static System.Windows.Media.Imaging.BitmapImage GetResourceImage(string assemblyname, string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                var assembly = Assembly.Load(assemblyname);

                var icon = LoadProjectResource(assembly, name);
                if (icon != null)
                    return ImageConverter.BitMapToBitmapImage((Bitmap)icon, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return null;
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapImage LoadBitmapFromResource(string pathInApplication, string assemblyName = "")
        {
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.RelativeOrAbsolute));
        }

        public static System.Windows.Media.Imaging.BitmapImage UrlFromResource(string path)
        {
            return new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public static T GetResource<T>(string name, string path = @"")
        {
            var resource = new ResourceDictionary
            {
                Source = new Uri(path, UriKind.RelativeOrAbsolute)
            };
            return (T)resource[name];
        }

        #endregion //Resources
    }
}
