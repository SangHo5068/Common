using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace Common.Utilities
{
    public static class RegistryHelper
    {
        public const string RegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";



        /// <summary>
        /// 레지스트리 등록
        /// </summary>
        /// <param name="name">실행파일 이름</param>
        /// <param name="path">실행파일 경로</param>
        /// <returns></returns>
        public static bool AddRegistry(string name, string path)
        {
            var runRegKey = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            // 시작프로그램 등록 여부 확인
            var result = runRegKey.GetValue(name) == null;
            if (result)
                runRegKey.SetValue(name, path);
            //runRegKey.SetValue(name, Environment.CurrentDirectory + "\\" + AppDomain.CurrentDomain.FriendlyName);
            return result;
        }

        /// <summary>
        /// 레지스트리 삭제
        /// </summary>
        /// <param name="name">실행파일 이름</param>
        /// <returns></returns>
        public static bool RemoveRegistry(string name)
        {
            var runRegKey = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            // 시작프로그램 등록 여부 확인
            var result = runRegKey.GetValue(name) != null;
            if (result)
                runRegKey.DeleteValue(name, false);
            return result;
        }
    }
}
