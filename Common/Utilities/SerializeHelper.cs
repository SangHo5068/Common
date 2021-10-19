using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Utilities
{
    /// <summary>
    /// DataTable에 포함되어 있는 컬럼 중 DateTime 타입이 있을 경우 
    /// DateTimeMode를 지정한 Mode로 바꾼 후 해당 DataTable 전체의 복제본을 반환하는 클래스.
    /// </summary>
    public static class DateTimeModeConverter
    {
        /// <summary>
        /// DataSet 에 포함된 DataTable 전체를 검사해 DateTimeMode를 바꾸고
        /// 전체 데이터를 재할당한 복사본 DataSet을 반환한다.
        /// 실제 데이터 변경 여부와 관계 없이 DataSet 전체를 새로 생성하므로 성능에 영향이 있을 수 있음.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static DataSet ConvertTo(DataSet dataSet, DataSetDateTime mode = DataSetDateTime.Utc)
        {
            var newDataSet = new DataSet();
            foreach (DataTable dataTable in dataSet.Tables)
            {
                newDataSet.Tables.Add(GetConvertedDataTable(dataTable, mode));
            }

            return newDataSet;
        }

        /// <summary>
        /// DataTable을 검사해 DateTimeMode를 바꾸고
        /// 전체 데이터를 재할당한 복사본 DataTable을 반환한다.
        /// 실제 데이터 변경 여부와 관계 없이 DataTable 전체를 새로 생성하므로 성능에 영향이 있을 수 있음.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static DataTable ConvertTo(DataTable dataTable, DataSetDateTime mode = DataSetDateTime.Utc)
        {
            var newTable = GetConvertedDataTable(dataTable, mode);

            if (dataTable.DataSet != null)
            {
                using (var tempParentDataSet = new DataSet())
                {
                    tempParentDataSet.DataSetName = dataTable.DataSet.DataSetName;
                    tempParentDataSet.Tables.Add(newTable);
                }
            }

            return newTable;
        }

        /// <summary>
        /// DateTimeMode를 바꾼 DataTable만 복제한다. ParentDataSet은 할당하지 않는다.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private static DataTable GetConvertedDataTable(DataTable dataTable, DataSetDateTime mode = DataSetDateTime.Utc)
        {
            var newTable = new DataTable(dataTable.TableName);
            foreach (DataColumn column in dataTable.Columns)
            {
                using (var newColumn = new DataColumn(column.ColumnName, column.DataType))
                {
                    if (column.DataType == typeof(DateTime))
                    {
                        newColumn.DateTimeMode = mode;
                    }

                    newTable.Columns.Add(newColumn);
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var newRow = newTable.NewRow();
                for (var i = 0; i < row.ItemArray.Length; i++)
                {
                    newRow[i] = row[i];
                }

                newTable.Rows.Add(newRow);
            }

            return newTable;
        }

        /// <summary>
        /// 해당 DataTable의 컬럼 중 타입이 DateTime인 컬럼의 Mode를 Utc로 바꾼다.
        /// DataTable은 Row 할당이 없는 컬럼만 Mode 변경이 가능하다. (한 번이라도 값을 할당 하면 예외 발생)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static void ConvertColumns(DataTable dataTable)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == typeof(DateTime))
                {
                    column.DateTimeMode = DataSetDateTime.Utc;
                }
            }
        }
    }

    /// <summary>
    /// The data type converter.
    /// </summary>
    public static class DataTypeConverter
    {
        /// <summary>
        /// 문자열이 공백으로만 이뤄져있거나 비어있거나 null이면 DBNull로 변환
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object ConvertToDbNullFromString(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return DBNull.Value;
            }

            return data;
        }

        /// <summary>
        /// 문자열이 비어있거나 null이면 DBNull로 변환
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object ConvertToDbNullFromStringAllowWhitespaces(string data)
        {
            return string.IsNullOrEmpty(data) ? (object)DBNull.Value : data;
        }

        /// <summary>
        /// The convert to db null from int.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object ConvertToDbNullFromInt(int? data)
        {
            if (data == null)
            {
                return DBNull.Value;
            }

            return data;
        }

        /// <summary>
        /// NaN과 null인 경우 DBNull로 반환하고, 그 이외의 경우에는 가능한 경우 값을 반환한다.
        /// </summary>
        /// <param name="p">입력하는 double</param>
        /// <exception cref="NotSupportedException">Infinity는 server에서 지원하지 않는다.</exception>
        /// <returns>DB에 들어갈 값을 나타내는 객체</returns>
        public static object ConvertToDbNullFromDouble(double? p)
        {
            if (!p.HasValue || double.IsNaN(p.Value))
            {
                return DBNull.Value;
            }

            if (double.IsInfinity(p.Value))
            {
                throw new NotSupportedException("infinity value is not supported in MSSQL");
            }

            return p.Value;
        }

        public static object ConvertToDbNullFromDateTime(DateTime? data)
        {
            if (data == null)
            {
                return DBNull.Value;
            }

            return (DateTime)data;
        }

        /// <summary>
        /// The convert to db bit from boolean.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static int ConvertToDbBitFromBoolean(bool data)
        {
            return data ? 1 : 0;
        }

        public static double ConvertToDoubleFromDb(object data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
            {
                return double.NaN;
            }

            double d;
            if (double.TryParse(data.ToString(), out d) == false)
            {
                return double.NaN;
            }

            return d;
        }

        /// <summary>
        /// The convert to nullable bit from boolean.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object ConvertToNullableBitFromBoolean(bool? data)
        {
            if (data == null)
            {
                return DBNull.Value;
            }

            if (data == false)
            {
                return 0;
            }

            if (data == true)
            {
                return 1;
            }

            return null;
        }

        /// <summary>
        /// Converter int from object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int? ConvertNullableInt(object data)
        {
            if (data == DBNull.Value)
            {
                return null;
            }

            var toString = data.ToString();
            return toString.ToNullableInt32();
        }

        public static double? ConvertNullableDouble(object data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
            {
                return null;
            }

            return data.ToString().ToNullableDouble();
        }

        public static DateTime? ConvertNullableDateTime(object data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
            {
                return null;
            }

            return data.ToString().ToNullableDatetime();
        }

        /// <summary>
        /// Converter string from object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertNullableString(object data)
        {
            if (data == null || String.IsNullOrEmpty(data.ToString()))
            {
                return null;
            }

            return data.ToString();
        }

        /// <summary>
        /// DataSet,DataTable 시리즈에 들어있는 타입을 있는 그대로 string 타입으로 변환해서 가져온다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertNullableStringStrict(object data)
        {
            if (data == DBNull.Value)
                return null;
            return data.ToString();
        }

        /// <summary>
        /// Converter bool from object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool? ConvertNullablebool(object data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
            {
                return null;
            }

            return data.ToString().ToNullableBool();
        }

        public static Guid? ConvertNullableGuid(object data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
            {
                return null;
            }

            return data.ToString().ToNullableGuid();
        }

        /// <summary>
        /// string을 Nullable int로 형변환한다.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int? ToNullableInt32(this string s)
        {
            int i;
            if (int.TryParse(s, out i))
            {
                return i;
            }

            return null;
        }

        /// <summary>
        /// string을 Nullable int로 형변환한다.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double? ToNullableDouble(this string s)
        {
            double d;
            if (double.TryParse(s, out d))
            {
                return d;
            }

            return null;
        }

        public static bool? ToNullableBool(this string s)
        {
            bool b;
            if (bool.TryParse(s, out b))
            {
                return b;
            }

            return null;
        }

        public static Guid? ToNullableGuid(this string s)
        {
            Guid g;
            if (Guid.TryParse(s, out g))
            {
                return g;
            }

            return null;
        }

        /// <summary>
        /// string을 Nullbale DateTime으로 형변환한다.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime? ToNullableDatetime(this string s)
        {
            DateTime date;
            if (DateTime.TryParse(s, out date))
            {
                return date;
            }

            return null;
        }

        public static string ToDateTimeFormatString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            if (DateTime.TryParse(s, out DateTime date))
            {
                return date.ToString(Defined.DateMinusLFormat);
            }

            return null;
        }

        public static T GetValueOrDefaultFromDB<T>(object data, T defaultValue)
        {
            System.Diagnostics.Debug.Assert(data is DBNull || data is T, "타입이 다름");
            return data is T ? (T)data : defaultValue;
        }

        public static T GetValueFromDb<T>(object data)
        {
            System.Diagnostics.Debug.Assert(data is DBNull || data is T, "Invalid type.");
            return data is T ? (T)data : default(T);
        }
    }

    /// <summary>
	/// The serialize helper.
	/// </summary>
	public class SerializeHelper
    {
        #region Serialize

        /// <summary>
        /// XmlSerializer 를 이용해 현재 타입의 인스턴스를 xml (string) 으로 반환한다. 
        /// </summary>
        /// <param name="obj">대상 인스턴스의 오브젝트.</param>
        /// <returns>
        /// Serialize 결과 XML ( string ).
        /// </returns>
        public static string SerializeByXmlSerializer<T>(T obj)
        {
            try
            {
                // DataTable 내부에 DateTime 타입이 있을 경우 
                // UTC 타입으로 바꿔 옮겨 담은 후 Serialize 한다.
                // TODO : DTO 구조 변경을 통해 DataSet / DataTable 을 새로 옮겨 담는 방식을 제거 해야함.
                // TODO : 대용량 처리 시 심각한 속도 저하 발생함. 반드시 수정 해야함. 
                if (obj is DataSet)
                {
                    var dataSet = obj as DataSet;
                    if (CheckContainDateTimeColumnInDataSet(dataSet))
                    {
                        var convertedDataSet = DateTimeModeConverter.ConvertTo(dataSet);
                        return InnerSerializeByXmlSerializer(convertedDataSet);
                    }
                }

                if (obj is DataTable)
                {
                    var dataTable = obj as DataTable;
                    if (CheckContainDateTimeColumnInDataTable(dataTable))
                    {
                        var convertedDataTable = DateTimeModeConverter.ConvertTo(dataTable);
                        return InnerSerializeByXmlSerializer(convertedDataTable);
                    }
                }

                return InnerSerializeByXmlSerializer(obj);
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[SaveDataToXml<T> Error]", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// XmlSerializer 를 이용해 현재 타입의 인스턴스를 xml (string) 으로 반환한다. 
        /// </summary>
        /// <param name="obj">대상 인스턴스의 오브젝트.</param>
        /// <returns>
        /// Serialize 결과 XML ( string ).
        /// </returns>
        public static string SerializeByXmlSerializerUTF16<T>(T obj)
        {
            try
            {
                // DataTable 내부에 DateTime 타입이 있을 경우 
                // UTC 타입으로 바꿔 옮겨 담은 후 Serialize 한다.
                // TODO : DTO 구조 변경을 통해 DataSet / DataTable 을 새로 옮겨 담는 방식을 제거 해야함.
                // TODO : 대용량 처리 시 심각한 속도 저하 발생함. 반드시 수정 해야함. 
                if (obj is DataSet)
                {
                    var dataSet = obj as DataSet;
                    if (CheckContainDateTimeColumnInDataSet(dataSet))
                    {
                        var convertedDataSet = DateTimeModeConverter.ConvertTo(dataSet);
                        return InnerSerializeByXmlSerializerUTF16(convertedDataSet);
                    }
                }

                if (obj is DataTable)
                {
                    var dataTable = obj as DataTable;
                    if (CheckContainDateTimeColumnInDataTable(dataTable))
                    {
                        var convertedDataTable = DateTimeModeConverter.ConvertTo(dataTable);
                        return InnerSerializeByXmlSerializerUTF16(convertedDataTable);
                    }
                }

                return InnerSerializeByXmlSerializerUTF16(obj);
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[SerializeByXmlSerializerUTF16<T> Error]", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// DataContractSerialize를 이용해 대상 인스턴스를 Serialize 한다.
        /// </summary>
        /// <param name="obj">대상 인스턴스 오브젝트.</param>
        /// <returns>
        /// Serialize 결과 XML ( string ).
        /// </returns>
        public static string SerializeByDataContractSerializer<T>(T obj)
        {
            try
            {
                // DataTable 내부에 DateTime 타입이 있을 경우 
                // UTC 타입으로 바꿔 옮겨 담은 후 Serialize 한다.
                // TODO : DTO 구조 변경을 통해 DataSet / DataTable 을 새로 옮겨 담는 방식을 제거 해야함.
                // TODO : 대용량 처리 시 심각한 속도 저하 발생함. 반드시 수정 해야함. - shlee.
                if (obj is DataSet)
                {
                    var dataSet = obj as DataSet;
                    if (CheckContainDateTimeColumnInDataSet(dataSet))
                    {
                        var convertedDataSet = DateTimeModeConverter.ConvertTo(dataSet);
                        return InnerSerializeByDataContractSerializer(convertedDataSet);
                    }
                }

                if (obj is DataTable)
                {
                    var dataTable = obj as DataTable;
                    if (CheckContainDateTimeColumnInDataTable(dataTable))
                    {
                        var convertedDataTable = DateTimeModeConverter.ConvertTo(dataTable);
                        return InnerSerializeByDataContractSerializer(convertedDataTable);
                    }
                }

                return InnerSerializeByDataContractSerializer(obj);
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[DataContractSerialize<T> Error]", ex);
                return string.Empty;
            }
        }

        /// <summary>
		/// Json Serialize
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string SerializeByJson<T>(T model)
        {
            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// Json Serialize to Camel Casing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string SerializeByJsonCamel<T>(T model)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                Converters = {
                    new JsonStringEnumConverter()
                }
            };
            return System.Text.Json.JsonSerializer.Serialize(model, serializeOptions);
        }

        /// <summary>
        /// Json Serialize to Pascal Casing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string SerializeByJsonPascal<T>(T model)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                IgnoreNullValues = true,
                Converters = {
                    new JsonStringEnumConverter()
                }
            };
            return System.Text.Json.JsonSerializer.Serialize(model, serializeOptions);
        }

        #endregion //Serialize

        #region Deserialize

        /// <summary>
        /// DataContractSerialize를 이용해 Xml 문자열을 Deserialize 한다.
        /// </summary>
        /// <param name="xmlData">
        /// DataContractSerialize를 이용해 Serialize된 xml 문자열.
        /// </param>
        /// <returns>
        /// 결과 오브젝트.
        /// </returns>
        public static T DeserializeByDataContractSerializer<T>(string xmlData) where T : class
        {
            try
            {
                using (var reader = new StringReader(xmlData))
                {
                    var xmlReader = XmlReader.Create(reader);
                    var serializer = new DataContractSerializer(typeof(T));
                    return serializer.ReadObject(xmlReader) as T;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[DataDeserialize<T> Error]", ex);
                return null;
            }
        }

        /// <summary>
        /// Stream을 전달받아 DataContractSerializer로 Deserialize 한다. 
        /// Deserialize 가 끝나면 Stream도 함께 Dispose 된다. Stream을 재사용해야할 경우 이 메서드를 사용하지 말 것!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        public static T DeserializeByDataContractSerializer<T>(Stream dataStream) where T : class
        {
            try
            {
                using (dataStream)
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    return serializer.ReadObject(dataStream) as T;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[DataDeserialize<T> Error]", ex);
                return null;
            }
        }

        /// <summary>
        /// 문자열을 객체로 변환한다. 
        /// </summary>
        /// <param name="xmlData">
        /// xml 문자열 데이터.
        /// </param>
        /// <returns>
        /// 결과 오브젝트.
        /// </returns>
        public static T DeserializeByXmlSerializer<T>(string xmlData) where T : class
        {
            try
            {
                using (var stringReader = new StringReader(xmlData))
                {
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        var serializer = new XmlSerializer(typeof(T));
                        return serializer.Deserialize(xmlReader) as T;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[ReadDataFromXml Error]", ex);
                return null;
            }
        }

        /// <summary>
        /// Stream을 전달받아 XmlSerializer로 Deserialize 한다. 
        /// Deserialize 가 끝나면 Stream도 함께 Dispose 된다. Stream을 재사용해야할 경우 이 메서드를 사용하지 말 것!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T DeserializeByXmlSerializer<T>(Stream stream) where T : class
        {
            try
            {
                using (stream)
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(stream) as T;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[ReadDataFromXml Error]", ex);
                return null;
            }
        }

        /// <summary>
        /// Json Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic DeserializeByJson(string value)
        {
            return JsonConvert.DeserializeObject(value);
        }

        /// <summary>
        /// Json Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeByJson<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Json Deserialize to Camel Casing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeByJsonCamel<T>(string value)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new JsonStringEnumConverter()
                }
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(value, serializeOptions);
        }

        /// <summary>
        /// Json Deserialize to Pascal Casing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeByJsonPascal<T>(string value)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                Converters = {
                    new JsonStringEnumConverter()
                }
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(value, serializeOptions);
        }

        /// <summary>
        /// JObject 에서 특정 Key 의 Value 를 반환
        /// </summary>
        /// <param name="json"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string JsonGetValue(string json, string key = "authToken")
        {
            var jObj = JObject.Parse(json);
            return jObj[key].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public static string JsonParse(string json)
        {
            var jObj = JObject.Parse(json);
            jObj.Capitalize();
            return jObj.ToString();
        }

        #endregion //Deserialize



        #region file
        /*
        // TODO : DataSet Deserialize 시 DataTable의 컬럼 중 DateTime 이 있을 경우 Kind를 UTC 로 바꿔서 옮겨 담도록 되어 있음. -> 현재 속도 저하 발생함. (심각)
        // 새로운 DTO를 설정하거나 DateTimeOffset 등을 사용하는 방식으로 Kind를 변경하지 않는 방향으로 구조 수정 필요. -by shwlee.
        */
        /// <summary>
        /// XmlSerialize 를 이용해 현재 타입의 인스턴스를 xml (file) 로 저장한다. 
        /// </summary>
        /// <param name="fileName">
        /// 저장될 파일명을 지정.
        /// </param>
        /// <returns>
        /// 저장 성공 유무를 반환.
        /// </returns>
        public static string SaveDataToXml<T>(string fileName, T target, bool useDataContractSerialize = false)
        {
            try
            {
                using (TextWriter streamWriter = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    var xmlData = useDataContractSerialize ? SerializeByDataContractSerializer(target) : SerializeByXmlSerializer(target);
                    streamWriter.Write(xmlData);
                    return xmlData;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[SaveDataToXml<T> Error]", ex);
                return ex.ToString();
            }
        }

        /// <summary>
        /// 파일을 객체로 변환한다.
        /// </summary>
        /// <param name="fileName">
        /// 읽어들일 xml 파일.
        /// </param>
        /// <param name="useDataContractSerialize">
        /// DataContractSerialize 사용 여부.
        /// 여기서는 DataContractSerialize 아니면 XmlSerialize 를 사용한다.
        /// </param>
        /// <returns>
        /// 결과 오브젝트.
        /// </returns>
        public static T ReadDataFromXmlFile<T>(string fileName, bool useDataContractSerialize = false) where T : class
        {
            try
            {
                using (var streamReader = new StreamReader(fileName))
                {
                    var xmlData = streamReader.ReadToEnd();

                    var result = useDataContractSerialize ? DeserializeByDataContractSerializer<T>(xmlData) : DeserializeByXmlSerializer<T>(xmlData);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[ReadDataFromXmlFile<T> Error]", ex);
                return null;
            }
        }


        /// <summary>
        /// Serialize DataSet to xml string has no diffgram.
        /// </summary>
        /// <param name="dataSet">The target DataSet.</param>
        /// <returns>The result xml string.</returns>
        public static string DataSetToXmlByNoDiffGram(DataSet dataSet)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        ConformanceLevel = ConformanceLevel.Document,
                        Encoding = new UTF8Encoding(false)
                    };

                    using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                    {
                        dataSet.WriteXml(xmlWriter, XmlWriteMode.WriteSchema);

                        xmlWriter.Flush();
                        if (xmlWriter.Settings == null)
                        {
                            throw new Exception("XmlWriterSettings is not created.");
                        }

                        memoryStream.Position = 0;
                        return GetStringFromWrittenBuffer(xmlWriter.Settings.Encoding, memoryStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogAndTrace(LogTypes.Exception, "[DataSetToXmlByNoDiffGram Error]", ex);
                return null;
            }
        }
        #endregion //file

        #region Byte Converter
        public static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is Stream)
            {
                return StreamToByteArray(obj as Stream);
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }

            if (typeof(T).Equals(typeof(Stream)))
            {
                return (T)ByteArrayToStream(data);
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                var obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
        #endregion //Byte Converter



        #region Private

        private static string InnerSerializeByXmlSerializer<T>(T obj)
        {
            using (var memStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = new string(' ', 4),
                    NewLineOnAttributes = false,
                    Encoding = new UTF8Encoding(false),
                };

                using (var xmlWriter = XmlWriter.Create(memStream, settings))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(xmlWriter, obj);
                }

                return GetStringFromWrittenBuffer(Encoding.UTF8, memStream);
            }
        }

        private static string InnerSerializeByXmlSerializerUTF16<T>(T obj)
        {
            var stringbuilder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = new string(' ', 4),
                NewLineOnAttributes = false,
                OmitXmlDeclaration = true
            };

            using (var xmlWriter = XmlWriter.Create(stringbuilder, settings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(xmlWriter, obj);
            }

            return stringbuilder.ToString();
        }

        private static bool CheckContainDateTimeColumnInDataSet(DataSet dataSet)
        {
            return dataSet.Tables.OfType<DataTable>().Any(CheckContainDateTimeColumnInDataTable);
        }

        private static bool CheckContainDateTimeColumnInDataTable(DataTable table)
        {
            return table.Columns.OfType<DataColumn>().Any(c => c.DataType == typeof(DateTime));
        }

        private static string InnerSerializeByDataContractSerializer<T>(T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    ConformanceLevel = ConformanceLevel.Document,
                    Encoding = new UTF8Encoding(false)
                };

                using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    var serializer = new DataContractSerializer(typeof(T));

                    serializer.WriteObject(xmlWriter, obj);

                    xmlWriter.Flush();

                    if (xmlWriter.Settings == null)
                    {
                        throw new Exception("XmlWriterSettings is not created.");
                    }

                    memoryStream.Position = 0;
                    return GetStringFromWrittenBuffer(xmlWriter.Settings.Encoding, memoryStream);
                }
            }
        }


        private static object ByteArrayToStream(byte[] data)
        {
            return new MemoryStream(data);
        }

        private static byte[] StreamToByteArray(Stream stream)
        {
            try
            {
                stream.Position = 0;
            }
            catch
            {
            }

            var readBuffer = new byte[1024];
            var outputBytes = new List<byte>();

            var offset = 0;

            while (true)
            {
                var bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

                if (bytesRead == 0)
                {
                    break;
                }
                else if (bytesRead == readBuffer.Length)
                {
                    outputBytes.AddRange(readBuffer);
                }
                else
                {
                    var tempBuf = new byte[bytesRead];
                    Array.Copy(readBuffer, tempBuf, bytesRead);
                    outputBytes.AddRange(tempBuf);
                    break;
                }

                offset += bytesRead;
            }

            return outputBytes.ToArray();
        }

        private static string GetStringFromWrittenBuffer(Encoding encoding, MemoryStream memoryStream)
        {
            return encoding.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        #endregion //Private
    }

    public static class JsonExtensions
    {
        public static void Capitalize(this JArray jArr)
        {
            foreach (var x in jArr.Cast<JToken>().ToList())
            {
                if (x is JObject childObj)
                {
                    childObj.Capitalize();
                    continue;
                }
                if (x is JArray childArr)
                {
                    childArr.Capitalize();
                    continue;
                }
            }
        }

        public static void Capitalize(this JObject jObj)
        {
            foreach (var kvp in jObj.Cast<KeyValuePair<string, JToken>>().ToList())
            {
                jObj.Remove(kvp.Key);
                var newKey = kvp.Key.Capitalize();
                if (kvp.Value is JObject childObj)
                {
                    childObj.Capitalize();
                    jObj.Add(newKey, childObj);
                    return;
                }
                if (kvp.Value is JArray childArr)
                {
                    childArr.Capitalize();
                    jObj.Add(newKey, childArr);
                    return;
                }
                jObj.Add(newKey, kvp.Value);
            }
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("empty string");
            }
            char[] arr = str.ToCharArray();
            arr[0] = char.ToUpper(arr[0]);
            return new string(arr);
        }
    }
}
