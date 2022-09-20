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
    public class HashHelper
    {
        public HashHelper() { }

        public enum HashType : int
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        /// <summary>
        /// 해쉬값을 가져옵니다.
        /// </summary>
        /// <param name="text">원본 문자열</param>
        /// <param name="hashType">해쉬 타입</param>
        /// <returns></returns>
        public static string GetHash(string text, HashType hashType)
        {
            string hashString;
            switch (hashType)
            {
                case HashType.MD5:
                    hashString = GetMD5(text);
                    break;
                case HashType.SHA1:
                    hashString = GetSHA1(text);
                    break;
                case HashType.SHA256:
                    hashString = GetSHA256(text);
                    break;
                case HashType.SHA512:
                    hashString = GetSHA512(text);
                    break;
                default:
                    hashString = "Invalid Hash Type";
                    break;
            }

            return hashString;
        }

        public static bool CheckHash(string original, string hashString, HashType hashType)
        {
            string originalHash = GetHash(original, hashType);
            return (originalHash == hashString);
        }

        private static string GetMD5(string text)
        {
            MD5 hashString = new MD5CryptoServiceProvider();
            string hex = "";

            byte[] message = Cryptography.CurrentEncoding.GetBytes(text);
            byte[] hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }

        private static string GetSHA1(string text)
        {
            byte[] hashValue;
            byte[] message = Cryptography.CurrentEncoding.GetBytes(text);

            SHA1Managed hashString = new SHA1Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }

        private static string GetSHA256(string text)
        {
            var message = Cryptography.CurrentEncoding.GetBytes(text);
            return GetSHA256(message);
            //var hashValue = hashString.ComputeHash(message);
            //foreach (byte x in hashValue)
            //    hex += String.Format("{0:x2}", x);
            //return hex;
        }

        public static string GetSHA256(byte[] buff)
        {
            var hex = string.Empty;
            var hashString = new SHA256Managed();
            var hashValue = hashString.ComputeHash(buff);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }

        private static string GetSHA512(string text)
        {
            byte[] hashValue;
            byte[] message = Cryptography.CurrentEncoding.GetBytes(text);

            SHA512Managed hashString = new SHA512Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);
            return hex;
        }



        /// <summary>
        /// 지정된 경로의 파일 해쉬값을 가져옵니다.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string GetFileHashSHA256(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string hex = string.Empty;
            //byte[] hashValue;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var hashString = new SHA256Managed();
                var hashValue  = hashString.ComputeHash(fs);
                foreach (byte x in hashValue)
                    hex += String.Format("{0:x2}", x);
            }

            return hex;
        }

        /// <summary>
        /// 사용되는 파일들을 해쉬값을 가져옵니다.(무결성 검사)
        /// </summary>
        /// <returns></returns>
        public static string GetFilesHash(string path, params object[] paramaters)
        {
            string hashValue = string.Empty;
            foreach (string target in paramaters.Cast<string>().ToList())
                hashValue += GetFileHashSHA256(path + target + ".dll");

            return GetHash(hashValue, HashType.SHA256);
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
