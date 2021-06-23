using System.Text.RegularExpressions;

namespace Common.Utilities
{
    public class DataValidation
    {
        #region 문자 입력 제한
        /// <summary>
        /// 한글 입력 제한
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsValidHangul(char c)
        {
            // 한글 입력 제한
            if ((0xAC00 <= c && c <= 0xD7A3) || (0x3131 <= c && c <= 0x318E))
                return true;
            return false;
        }

        /// <summary>
        /// 특수문자 입력 제한
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsValidSpecialLetters(char c)
        {
            // 특수문자 인경우
            if (char.IsSymbol(c) || char.IsPunctuation(c))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 정규식 조건에 맞는 문자열인지 체크합니다.
        /// </summary>
        /// <param name="type">1 : 숫자, 2 : 영문자, 3 : 한글, 4 : 한자, 5 : 숫자+영문자, 
        ///                    6 : 숫자+영문자+한자, 7 : 숫자+영문자+한자+한글</param>
        /// <param name="plainText">문자열</param>
        /// <returns></returns>
        public static bool IsValidRegexMatch(int type, string plainText)
        {
            Regex rx;
            switch (type)
            {
                case 1:
                    rx = new Regex(@"^[0-9]*$", RegexOptions.None);
                    break;
                case 2:
                    rx = new Regex(@"^[a-zA-Z]*$", RegexOptions.None);
                    break;
                case 3:
                    rx = new Regex(@"^[가-힣]*$", RegexOptions.None);
                    break;
                case 4:
                    rx = new Regex(@"^[一-龥]*$", RegexOptions.None);
                    break;
                case 5:
                    rx = new Regex(@"^[a-zA-Z0-9]*$", RegexOptions.None);
                    break;
                case 6:
                    rx = new Regex(@"^[a-zA-Z0-9一-龥]*$", RegexOptions.None);
                    break;
                case 7:
                    rx = new Regex(@"^[a-zA-Z0-9가-힣一-龥]*$", RegexOptions.None);
                    break;
                case 8:
                    rx = new Regex(@"^[가-힣@,!,@,#,$,%,^,&,*,(,),-,=,+,/,<,>,`,;,:,',{,},\,|,?,_,~]*$");
                    break;
                default:
                    return false;

            }
            return !string.IsNullOrEmpty(plainText) && rx.IsMatch(plainText);
        }
        #endregion //문자 입력 제한
    }
}
