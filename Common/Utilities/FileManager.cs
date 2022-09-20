using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32;

namespace Common.Utilities
{
    #region <INI>
    /// <summary>
    /// ".ini" File read / write method
    /// </summary>
    public static class INIHelper
    {
        //읽기
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        //쓰기
        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);


        /// <summary>
        /// INI파일로부터 데이타를 일기
        /// </summary>
        /// <param name="strSection"></param>
        /// <param name="strKey"></param>
        /// <param name="strDefault"></param>
        /// <param name="strINIPath"></param>
        /// <returns>Value</returns>
        public static string ReadIniValue(String strSection, String strKey, String strDefault, String strINIPath)
        {
            StringBuilder dstrResult = new StringBuilder(255);
            int i = 0;

            try
            {
                i = GetPrivateProfileString(strSection, strKey, strDefault, dstrResult, 255, strINIPath);   //"색션", "키", "Default", result, size, iniPath
            }
            catch
            {
            }

            return dstrResult.ToString();
        }

        /// <summary>
        /// INI파일에 데이타 쓰기
        /// </summary>
        /// <param name="strSection"></param>
        /// <param name="strKey"></param>
        /// <param name="strValue"></param>
        /// <param name="strINIPath"></param>
        public static void WriteIniValue(String strSection, String strKey, String strValue, String strINIPath)
        {
            try
            {
                WritePrivateProfileString(strSection, strKey, strValue, strINIPath);                            //"색션", "키", "설정할값", iniPath
            }
            catch
            {
            }
        }
    }
    #endregion //INI

    #region File
    public class ProgressEventArgs : EventArgs
    {
        private float m_progress;

        public ProgressEventArgs(float progress)
        {
            m_progress = progress;
        }

        public float Progress => m_progress;

    }

    public class ProgressStream : Stream
    {
        private Stream m_input = null;
        private long m_length = 0L;
        private long m_position = 0L;
        public event EventHandler<ProgressEventArgs> UpdateProgress;

        public ProgressStream(Stream input)
        {
            m_input = input;
            m_length = input.Length;
        }
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = m_input.Read(buffer, offset, count);
            m_position += n;
            UpdateProgress?.Invoke(this, new ProgressEventArgs((1.0f * m_position) / m_length));
            return n;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => m_length;
        public override long Position
        {
            get { return m_position; }
            set { throw new NotImplementedException(); }
        }
    }

    public static class FileManager
    {
        public static readonly Encoding CurrentEncoding = Encoding.UTF8;


        /// <summary>
        /// 파일 존제 유무(True:파일 있음)
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static Boolean FileExist(String FileName)
        {
            return File.Exists(FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string[] GetFiles(String FileName)
        {
            return Directory.GetFiles(FileName);
        }

        /// <summary>
        /// 파일 생성
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Context"></param>
        /// <returns>(True:생성 성공, False:생성 실패)</returns>
        public static void FileCreate(String FileName, String Context = null)
        {
            try
            {
                var FilePath = Path.GetDirectoryName(FileName);
                //폴더가 없으면 만들고
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);

                using (StreamWriter sw = File.AppendText(FileName))
                {
                    string writeText = (Context != null) ? Context.ToString() : String.Empty;
                    sw.WriteLine(writeText);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 파일 생성
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Context">Stream</param>
        /// <param name="ContextLength">Stream Length</param>
        /// <param name="callback">Action</param>
        /// <returns>(True:생성 성공, False:생성 실패)</returns>
        public static bool FileCreate(String FileName,
            Stream Context = null, long ContextLength = 0, Action<double> callback = null)
        {
            try
            {
                var FilePath = Path.GetDirectoryName(FileName);
                //폴더가 없으면 만들고
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);

                FileDelete(FileName);
                if (Context == null)
                    return false;

                double progress = 0;
                using (FileStream stream = new FileStream(FileName, FileMode.Create))
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int length = 0;
                    do
                    {
                        length = Context.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, length);

                        try
                        {
                            progress = ((double)stream.Length / (double)ContextLength) * 100D;
                        }
                        catch (Exception ex) { }
                        callback?.Invoke(progress);
                    }
                    while (length > 0);

                    stream.Close();
                    Context?.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 파일 오픈
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string FileOpen(String FileName)
        {
            string str = string.Empty;
            using (StreamReader srFile = new StreamReader(FileName, CurrentEncoding))
            {
                str = srFile.ReadToEnd();
            }
            return str;
        }

        public static void DirectoryDelete(String DirectoryPath)
        {
            try
            {
                //파일이 있으면 지운다.
                if (Directory.Exists(DirectoryPath))
                    Directory.Delete(DirectoryPath, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void FileDelete(String FilePath)
        {
            try
            {
                //파일이 있으면 지운다.
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileNames"></param>
        /// <param name="isMulti"></param>
        /// <param name="pRestoreDirectory"></param>
        /// <param name="defaultExt"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string OpenFileDialog(out string[] fileNames, string filter = "All Files (*.*)|*.*", string defaultExt = ".cab",
            bool isMulti = false, bool pRestoreDirectory = true)
        {
            var dlg = new OpenFileDialog
            {
                RestoreDirectory = pRestoreDirectory,
                DefaultExt = defaultExt,
                Multiselect = isMulti,
                Filter = filter
            };

            fileNames = new string[0];
            if (dlg.ShowDialog() == true)
            {
                fileNames = dlg.FileNames;
                return dlg.FileName;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string OpenFolderDialog(string path = "")
        {
            string selectfolder = string.Empty;
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = path
            };
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectfolder = dlg.SelectedPath;
            }
            return selectfolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="defaultExt"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string SaveFileDialog(string fileName, string filter = "All Files (*.*)|*.*", string defaultExt = ".cab")
        {
            var dlg = new SaveFileDialog
            {
                FileName = fileName,
                DefaultExt = defaultExt,
                Filter = filter
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            else
                return string.Empty;
        }

        /// <summary>
        /// 폴더 오픈
        /// </summary>
        /// <param name="exe"></param>
        /// <param name="pArg"></param>
        /// <param name="runas">관리자 권한 실행 여부(true:관리자권한 실행)</param>
        public static string ExecuteExplorer(string exe = "explorer.exe", string pArg = null, bool runas = false)
        {
            var result = string.Empty;
            try
            {
                if (File.Exists(pArg))
                    pArg = "/select, " + pArg;
                var processInfo = new System.Diagnostics.ProcessStartInfo(exe)
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    UseShellExecute = string.IsNullOrEmpty(pArg),
                    Arguments = pArg,
                    CreateNoWindow = true,
                    RedirectStandardInput  = !string.IsNullOrEmpty(pArg),
                    RedirectStandardOutput = !string.IsNullOrEmpty(pArg),
                    Verb = runas ? "runas" : null,
                };
                var process = new System.Diagnostics.Process {
                    StartInfo = processInfo
                };
                process.Start();
                //process.WaitForExit();
                if (!processInfo.UseShellExecute)
                {
                    using (var sOut = process.StandardOutput)
                        result = sOut.ReadToEnd();
                }
                //process.Dispose();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return result;
        }

        /// <summary>
        /// 파일을 암호화 하여 내보내기
        /// </summary>
        /// <param name="ExportFileName"></param>
        public static void FileEncryptExport(string ExportFileName)
        {
            string strFile = string.Empty;
            using (StreamReader srFile = new StreamReader(ExportFileName, CurrentEncoding))
            {
                strFile = srFile.ReadToEnd();
            }

            using (StreamWriter swCryptFile = new StreamWriter(ExportFileName))
            {
                Cryptography crypt = new Cryptography();
                swCryptFile.Write(crypt.Encrypt_DES(strFile));
            }
        }

        /// <summary>
        /// 파일을 암호화 하여 내보내기
        /// </summary>
        /// <param name="FileName">암호화 할 대상 파일</param>
        /// <param name="ExportFileName">암호화 된 새로운 파일</param>
        public static void FileEncryptExport(string FileName, string ExportFileName)
        {
            string strFile = string.Empty;
            using (StreamReader srFile = new StreamReader(FileName, CurrentEncoding))
            {
                strFile = srFile.ReadToEnd();
            }

            using (FileStream fs = new FileStream(ExportFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                Cryptography crypt = new Cryptography();
                byte[] arrBt = crypt.Encrypt_DESGetBytes(strFile);
                fs.Write(arrBt, 0, arrBt.Length);
                fs.Flush();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ExportFileName"></param>
        /// <returns></returns>
        public static string FileDecryptImport(string ExportFileName)
        {
            string strFile = string.Empty;
            string des = string.Empty;
            using (StreamReader srFile = new StreamReader(ExportFileName, CurrentEncoding))
            {
                strFile = srFile.ReadToEnd();
                Cryptography crypt = new Cryptography();
                des = crypt.Decrypt_DES(strFile);
            }
            return des;
        }

        /// <summary>
        /// 암호화된 파일 읽어오기
        /// </summary>
        /// <param name="FileName">암호화 된 대상 파일</param>
        public static string FileDecryptImport(string FileName, string ExportFileName)
        {
            byte[] arrReadData;
            string returnData = string.Empty;
            using (FileStream fs = new FileStream(ExportFileName, FileMode.Open, FileAccess.Read))
            {
                arrReadData = new byte[fs.Length];
                fs.Read(arrReadData, 0, arrReadData.Length);
                Cryptography crypt = new Cryptography();
                returnData = crypt.Decrypt_DESGetBytes(arrReadData);
            }
            return returnData;
        }

        /// <summary>
        /// 정보
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static System.Collections.Generic.Dictionary<string, object> GetProperties<T>(T obj)
        {
            var props = typeof(T).GetProperties();
            var _Attributes = new System.Collections.Generic.Dictionary<string, object>();
            foreach (var prop in props)
            {
                try
                {
                    var tname = typeof(T).Name;
                    var propName = prop.Name;
                    var value = prop.GetValue(obj, null);
                    _Attributes.Add(propName, value);
                    Logger.Trace(string.Format("{0}.{1} = {2}", tname, propName, value));
                }
                catch (Exception ex)
                {
                }
            }
            return _Attributes;
        }
    }
    #endregion //File
}
