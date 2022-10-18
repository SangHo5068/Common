using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Common.Utilities
{
    /// <summary>
    /// 해쉬메소드
    /// 지정된 타입으로 해쉬를 만듭니다.(MD5, SHA1, SHA256, SHA512)
    /// </summary>
    public static class HashHelper
    {
        public enum HashType : int
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        //private static string GetHash(byte[] hash)
        private static string GetHash(object text, HashAlgorithm algorithm)
        {
            var hash = new byte[0];
            if (text is string str && !string.IsNullOrEmpty(str))
            {
                var message = Cryptography.CurrentEncoding.GetBytes(str);
                hash = algorithm.ComputeHash(message);
            }
            else
            if (text is FileStream fs && fs.CanRead)
                hash = algorithm.ComputeHash(fs);

            var hex = string.Empty;
            foreach (byte x in hash)
                hex += String.Format("{0:x2}", x);
            return hex;
        }

        private static string GetMD5(object text)
        {
            var algorithm = new MD5CryptoServiceProvider();

            return GetHash(text, algorithm);
        }

        private static string GetSHA1(object text)
        {
            var algorithm = new SHA1Managed();

            return GetHash(text, algorithm);
        }

        private static string GetSHA256(object text)
        {
            var algorithm = new SHA256Managed();

            return GetHash(text, algorithm);
        }

        private static string GetSHA512(object text)
        {
            var algorithm = new SHA512Managed();

            return GetHash(text, algorithm);
        }

        ///// <summary>
        ///// 인코딩이 없는 파일 데이터 해쉬값
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public static string GetSHA256(byte[] message)
        //{
        //    var hashAlgorithm = new SHA256Managed();
        //    var hashValue = hashAlgorithm.ComputeHash(message);
        //    return GetHash(hashValue);
        //}



        public static bool CheckHash(string original, string hashString, HashType hashType)
        {
            string originalHash = GetHash(original, hashType);
            return (originalHash == hashString);
        }

        /// <summary>
        /// 해쉬값을 가져옵니다.
        /// </summary>
        /// <param name="text">원본 문자열</param>
        /// <param name="hashType">해쉬 타입</param>
        /// <returns></returns>
        public static string GetHash(object text, HashType hashType)
        {
            string hash;
            switch (hashType)
            {
                case HashType.MD5:
                    hash = GetMD5(text);
                    break;
                case HashType.SHA1:
                    hash = GetSHA1(text);
                    break;
                case HashType.SHA256:
                    hash = GetSHA256(text);
                    break;
                case HashType.SHA512:
                    hash = GetSHA512(text);
                    break;
                default:
                    hash = "Invalid Hash Type";
                    break;
            }

            return hash;
        }

        /// <summary>
        /// 지정된 경로의 파일 해쉬값을 가져옵니다.
        /// </summary>
        /// <param name="path">파일이름</param>
        /// <returns></returns>
        public static string GetFilesHash(string path, HashType hashType = HashType.SHA256)
        {
            if (!File.Exists(path))
                return string.Empty;

            var hex = string.Empty;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                hex = GetHash(fs, hashType);
            }
            return hex;
        }

        /// <summary>
        /// 사용되는 파일들을 해쉬값을 가져옵니다.(무결성 검사)
        /// </summary>
        /// <returns></returns>
        public static string GetFilesHash(string path, HashType hashType, params object[] paramaters)
        {
            var hashValue = string.Empty;
            if (paramaters != null && paramaters.Length > 0)
            {
                foreach (string target in paramaters.Cast<string>().ToList())
                    hashValue += GetFilesHash(path + target + ".dll", hashType);
            }

            return GetHash(hashValue, hashType);
        }
    }

    /// <summary>
    /// 암호화 / 복호화 클래스
    /// </summary>
    public class Cryptography
    {
        public static readonly Encoding CurrentEncoding = Encoding.UTF8;
        public readonly string Key;
        byte[] _bSkey = new byte[8];

        public const int AES = 1;
        public const int DES = 2;



        public Cryptography(string key = "COMMON_K")
        {
            Key = key;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="nAlgorithm"></param>
        /// <param name="bEncrypt">True:Encrypt,False:Decrypt</param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public string CryptographyInit(int nAlgorithm, bool bEncrypt, string strData)
        {
            var strReturnData = string.Empty;
            try
            {
                switch (nAlgorithm)
                {
                    case AES:
                        if (bEncrypt == true)
                            strReturnData = Encrypt_AES(strData, Key);
                        else
                            strReturnData = Decrypt_AES(strData, Key);
                        break;
                    case DES:
                        if (bEncrypt == true)
                            strReturnData = Encrypt_DES(strData);
                        else
                            strReturnData = Decrypt_DES(strData);
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                strReturnData = strData;
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return strReturnData;
        }

        private static string Encrypt_AES(string InputText, string Password)
        {
            // Rihndael class를 선언하고, 초기화 합니다
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            // 입력받은 문자열을 바이트 배열로 변환
            byte[] PlainText = CurrentEncoding.GetBytes(InputText);

            // 딕셔너리 공격을 대비해서 키를 더 풀기 어렵게 만들기 위해서
            // Salt를 사용합니다.
            byte[] Salt = CurrentEncoding.GetBytes(Password.Length.ToString());

            // PasswordDeriveBytes 클래스를 사용해서 SecretKey를 얻습니다.
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Create a encryptor from the existing SecretKey bytes.
            // encryptor 객체를 SecretKey로부터 만듭니다.
            // Secret Key에는 32바이트
            // (Rijndael의 디폴트인 256bit가 바로 32바이트입니다)를 사용하고,
            // Initialization Vector로 16바이트
            // (역시 디폴트인 128비트가 바로 16바이트입니다)를 사용합니다
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            // 메모리스트림 객체를 선언,초기화 
            MemoryStream memoryStream = new MemoryStream();

            // CryptoStream객체를 암호화된 데이터를 쓰기 위한 용도로 선언
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            // 암호화 프로세스가 진행됩니다.
            cryptoStream.Write(PlainText, 0, PlainText.Length);

            // 암호화 종료
            cryptoStream.FlushFinalBlock();

            // 암호화된 데이터를 바이트 배열로 담습니다.
            byte[] CipherBytes = memoryStream.ToArray();

            // 스트림 해제
            memoryStream.Close();
            cryptoStream.Close();

            // 암호화된 데이터를 Base64 인코딩된 문자열로 변환합니다.
            string EncryptedData = Convert.ToBase64String(CipherBytes);

            // 최종 결과를 리턴
            return EncryptedData;
        }

        private static string Decrypt_AES(string InputText, string Password)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(InputText);
            byte[] Salt = CurrentEncoding.GetBytes(Password.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Decryptor 객체를 만듭니다.
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream(EncryptedData);

            // 데이터 읽기(복호화이므로) 용도로 cryptoStream객체를 선언, 초기화
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            // 복호화된 데이터를 담을 바이트 배열을 선언합니다.
            // 길이는 알 수 없지만, 일단 복호화되기 전의 데이터의 길이보다는
            // 길지 않을 것이기 때문에 그 길이로 선언합니다
            byte[] PlainText = new byte[EncryptedData.Length];

            // 복호화 시작
            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            // 복호화된 데이터를 문자열로 바꿉니다.
            string DecryptedData = CurrentEncoding.GetString(PlainText, 0, DecryptedCount);

            // 최종 결과 리턴
            return DecryptedData;
        }

        public string Encrypt_DES(string p_data)
        {
            _bSkey = CurrentEncoding.GetBytes(Key);
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider
            {
                Key = _bSkey,
                IV = _bSkey
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] data = CurrentEncoding.GetBytes(p_data.ToCharArray());
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt_DES(string p_data)
        {
            _bSkey = CurrentEncoding.GetBytes(Key);
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider
            {
                Key = _bSkey,
                IV = _bSkey
            };
            byte[] buff = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cryStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    byte[] data = Convert.FromBase64String(p_data);
                    cryStream.Write(data, 0, data.Length);
                    cryStream.FlushFinalBlock();

                    buff = ms.GetBuffer();
                }
            }

            return CurrentEncoding.GetString(buff);
        }

        /// <summary>
        /// 암호화한 파일을 Byte[]로 반환
        /// </summary>
        /// <param name="p_data"></param>
        /// <returns></returns>
        public byte[] Encrypt_DESGetBytes(string p_data)
        {
            _bSkey = CurrentEncoding.GetBytes(Key);
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider
            {
                Key = _bSkey,
                IV = _bSkey
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] data = CurrentEncoding.GetBytes(p_data.ToCharArray());
            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return ms.ToArray();
        }

        /// <summary>
        /// 복호화한 파일을 Byte[]로 반환
        /// </summary>
        /// <param name="p_data"></param>
        /// <returns></returns>
        //public byte[] Decrypt_DESGetBytes(string p_data)
        public string Decrypt_DESGetBytes(byte[] p_data)
        {
            _bSkey = CurrentEncoding.GetBytes(Key);
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider
            {
                Key = _bSkey,
                IV = _bSkey
            };
            byte[] buff = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cryStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    //byte[] data = Convert.FromBase64String(p_data);
                    byte[] data = p_data;
                    cryStream.Write(data, 0, data.Length);
                    cryStream.FlushFinalBlock();

                    buff = ms.GetBuffer();
                }
            }

            return CurrentEncoding.GetString(buff);
        }
    }
}
