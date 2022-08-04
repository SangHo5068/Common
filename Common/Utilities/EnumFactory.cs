using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace Common.Utilities
{
    public static class EnumFactory
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class DisplayAttribute : Attribute
        {
            public DisplayAttribute(string displayName)
            {
                Description = displayName;
            }

            public string Description { get; set; }
        }


        public class DisplayAttributeBasedObjectDataProvider : ObjectDataProvider
        {
            public object GetEnumValues(Enum enumObj)
            {
                var attribute = enumObj.GetType().GetRuntimeField(enumObj.ToString()).
                    GetCustomAttributes(typeof(DisplayAttribute), false).
                    SingleOrDefault() as DisplayAttribute;
                return attribute == null ? enumObj.ToString() : attribute.Description;
            }

            public List<object> GetShortListOfApplicationGestures(Type type)
            {
                var shortListOfApplicationGestures = Enum.GetValues(type).OfType<Enum>().Select(GetEnumValues).ToList();
                return
                    shortListOfApplicationGestures;
            }
        }

        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                //해당 text 값을 배열로 추출해 옵니다.
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DisplayAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        /// <summary>
        /// 열거형 데이터 Source
        /// </summary>
        /// <param name="type"></param>
        /// <param name="all">All 추가 여부(Default : false)</param>
        /// <returns></returns>
        public static ObservableCollection<Object> GetEnumeration(Type type, bool all = false)
        {
            ObservableCollection<Object> enumDictionary = new ObservableCollection<Object>();
            try
            {
                var enumList = Enum.GetValues(type).OfType<Enum>();
                if (all)
                    enumDictionary.Add("All");
                foreach (var item in enumList)
                    enumDictionary.Add(item);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return enumDictionary;
        }

        /// <summary>
        /// 열거형 데이터에서 param을 제외한 컬렉션을 추가한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="all">All 추가 여부(Default : false)</param>
        /// <param name="param">제외 할 열거형 데이터</param>
        /// <returns></returns>
        public static ObservableCollection<Object> GetEnumeratorExcept(Type type, bool all = false, params Enum[] param)
        {
            ObservableCollection<Object> enumDictionary = new ObservableCollection<Object>();

            try
            {
                var enumList = Enum.GetValues(type).OfType<Enum>();
                if (all)
                    enumDictionary.Add("All");
                foreach (var item in enumList)
                {
                    if (param.Any(a => a.Equals(item)))
                        continue;
                    enumDictionary.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return enumDictionary;
        }

        /// <summary>
        /// 열거형 데이터에서 param을 제외한 컬렉션을 추가한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="all">All 추가 여부(Default : false)</param>
        /// <param name="param">제외 할 열거형 데이터</param>
        /// <returns></returns>
        public static List<Object> GetEnumeratorExceptEnum(Type type, bool all = false, params Enum[] param)
        {
            var enumDictionary = new List<Object>();
            try
            {
                var enumList = Enum.GetValues(type);
                if (all)
                    enumDictionary.Add("All");
                foreach (var item in enumList)
                {
                    if (param.Any(a => a.Equals(item)))
                        continue;
                    enumDictionary.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return enumDictionary;
        }

        /// <summary>
        /// 열거형 데이터에서 param에 있는 항목만 추가한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="all">All 추가 여부(Default : false)</param>
        /// <param name="param">추가 할 열거형 데이터</param>
        /// <returns></returns>
        public static ObservableCollection<Object> GetEnumeratorImport(Type type, bool all = false, params Enum[] param)
        {
            ObservableCollection<Object> enumDictionary = new ObservableCollection<Object>();
            try
            {
                var enumList = Enum.GetValues(type).OfType<Enum>();
                if (all)
                    enumDictionary.Add("All");
                foreach (var item in enumList)
                {
                    if (!param.Any(a => a.Equals(item)))
                        continue;
                    enumDictionary.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return enumDictionary;
        }

        public static bool IsNotNullOrEmptyFromKeyValuePairInObject(object selectedItem)
        {
            if (selectedItem == null) return false;
            return !IsNullOrEmptyWithKeyValuePair(GetKeyToString((KeyValuePair<object, string>)selectedItem));
        }

        public static bool IsNullOrEmptyWithKeyValuePair(string selectedItem)
        {
            return string.IsNullOrEmpty(selectedItem);
        }

        public static string GetKeyToString(KeyValuePair<object, string> selectedItem)
        {
            return selectedItem.Key.ToString();
        }

        public static object GetKey(KeyValuePair<object, string> selectedItem)
        {
            return selectedItem.Key;
        }
    }
}
