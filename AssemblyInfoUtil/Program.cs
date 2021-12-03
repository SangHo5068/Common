using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Common.Utilities;

namespace AssemblyInfoUtil
{
    class Program
    {
        private static string fileName = string.Empty;
        private static string setupfileName = string.Empty;

        /// <summary>
        /// AssemblyInfoUtil.exe -setup:"C:\Program Files\MyProject1\Setup1\Setup1.vdproj" -ass:"C:\Program Files\MyProject1\AssemblyInfo.cs"
        /// 
        /// AssemblyInfoUtil.exe -setup:"$(ProjectDir)$(TargetName)_Setup\$(TargetName)_Setup.vdproj" -ass:"$(TargetPath)"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                if (args.Length <= 0)
                {
                    args = new string[] {
                        "-setup:" + @"D:\Works\Commons\Common\Setup\Setup.vdproj",
                        "-ass:" + @"D:\Works\Commons\Common\Output\Common.Demo.exe"
                    };
                }
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-setup:"))
                    {
                        setupfileName = args[i].Substring("-setup:".Length);
                    }
                    else if (args[i].StartsWith("-ass:"))
                    {
                        fileName = args[i].Substring("-ass:".Length);
                    }
                }

                if (File.Exists(setupfileName) && File.Exists(fileName))
                {
                    //var setup = File.ReadAllText(setupfileName);
                    //var json = SerializeHelper.DeserializeByJson(setup);

                    //Jeremy Thompson showing how to detect "ProductVersion" = "8:1.0.0" in vdproj
                    string setupproj = File.ReadAllText(setupfileName);
                    int startPosOfProductVersion = setupproj.IndexOf("\"ProductVersion\" = \"") + 20;
                    int endPosOfProductVersion = setupproj.IndexOf(Environment.NewLine, startPosOfProductVersion) - startPosOfProductVersion;
                    string versionStr = setupproj.Substring(startPosOfProductVersion, endPosOfProductVersion);
                    string oldVersionStr = versionStr.Replace("\"", string.Empty).Replace("8:", string.Empty);

                    var info = new FileInfo(fileName);
                    Assembly _asm = Assembly.LoadFile(fileName);
                    //var version = _asm.CustomAttributes.FirstOrDefault(f => f.ConstructorArguments);
                    var version = _asm.CustomAttributes.FirstOrDefault(f => f.AttributeType == typeof(AssemblyInformationalVersionAttribute));
                    string newVer = version.ConstructorArguments.FirstOrDefault().Value.ToString();

                    var newversionStr = versionStr.Replace(oldVersionStr, newVer);
                    var newSetupProj = setupproj.Replace(versionStr, newversionStr);
                    //File.WriteAllText(setupfileName, setupproj);

                    var bak = string.Format("{0}.{1}.bak", setupfileName, oldVersionStr);
                    if (File.Exists(bak))
                        File.Delete(bak);

                    var arrBt = Encoding.Default.GetBytes(newSetupProj);
                    File.Move(setupfileName, bak);
                    using (var file = new FileStream(setupfileName, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        file.Write(arrBt, 0, arrBt.Length);
                        file.Flush();
                    }
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }
    }
}
