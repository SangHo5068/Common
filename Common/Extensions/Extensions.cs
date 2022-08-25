using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;

namespace Common.Extensions
{
    public static class EventExtension
    {
        public static void RemoveEvents<T>(this T target, string eventName) where T : Control
        {
            if (target is null) throw new NullReferenceException("Argument \"target\" may not be null.");
            FieldInfo fieldInfo = typeof(Control).GetField(eventName, BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldInfo is null) throw new ArgumentException(
                string.Concat("The control ", typeof(T).Name, " does not have a property with the name \"", eventName, "\""), nameof(eventName));
            object eventInstance = fieldInfo.GetValue(target);
            PropertyInfo propInfo = typeof(T).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)propInfo.GetValue(target, null);
            list.RemoveHandler(eventInstance, list[eventInstance]);
        }

        public static void RemoveControlEvents<T>(this T target, string eventName)
            where T : class
        {
            RemoveObjectEvents<T>(target, eventName);
        }

        private static void RemoveObjectEvents<T>(T target, string eventName) where T : class
        {
            var typeOfT = typeof(T);
            //var fieldInfo = typeOfT.BaseType.GetField(
            //var fieldInfo = typeOfT.GetField(
            //    eventName, BindingFlags.Static | BindingFlags.NonPublic);
            var fieldInfo = GetEventField(typeOfT, eventName);
            if (fieldInfo == null)
                return;
            var provertyValue = fieldInfo.GetValue(target);
            var propertyInfo = typeOfT.GetProperty("Events", BindingFlags.NonPublic);
            if (propertyInfo == null)
                return;
            var eventHandlerList = (EventHandlerList)propertyInfo.GetValue(target, null);
            eventHandlerList.RemoveHandler(provertyValue, eventHandlerList[provertyValue]);
        }

        private static FieldInfo GetEventField(this Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }
            return field;
        }
    }

    public static class FlagExtension
    {
        //public static T Add<T>(this object obj, T toAdd) where T : struct
        //{
        //    T t = EnumExtension.ToEnum<T>(obj);
        //    return t | toAdd;
        //}
        //appends a value
        public static T Add<T>(this Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type | (int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }

        //completely removes the value
        public static T Remove<T>(this Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type & ~(int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                        ), ex);
            }
        }

        //checks if the value contains the provided type
        public static bool Has<T>(this Enum type, T value)
        {
            try
            {
                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            }
            catch
            {
                return false;
            }
        }

        //checks if the value is only the provided type
        public static bool Is<T>(this Enum type, T value)
        {
            try
            {
                return (int)(object)type == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }

        public static Boolean IsEnumFlagPresent<T>(T value, T lookingForFlag) where T : struct
        {
            int intValue = (int)(object)value;
            int intLookingForFlag = (int)(object)lookingForFlag;
            return ((intValue & intLookingForFlag) == intLookingForFlag);
        }
    }

    public static class EnumExtension
    {
        public static T ToEnum<T>(this object obj) where T : struct
        {
            if (Enum.TryParse(obj.ToString(), true, out T converted) == false)
                return default(T);

            return converted;
        }

        public static T GetEnum<T>(this object target) where T : struct
        {
            // TODO : need validtion check.
            var type = typeof(T);
            if (type != typeof(int) || type != typeof(string))
                return default(T);

            return (T)Enum.Parse(typeof(T), target.ToString(), true);
        }

        public static string ToDisplay<T>(this T obj) where T : struct
        {
            var converter = new Converters.EnumToDisplayConverter().Convert(obj, typeof(T), null, null);
            return converter?.ToString();
        }

        /// <summary>
        /// Enum Description to String
        /// Local, Client 인 경우 Port 변환 필요.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToDescription<T>(this T source) where T : struct
        {
            var fi = source.GetType().GetField(source.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return source.ToString();
        }
    }

    /// <summary>
    /// Provides a class that gives us the same features as a [FlagsAttribute] 
    /// enum with methods to query the state of the register but allows us to use
    /// any (appropriately defined) enumeration.
    /// </summary>
    /// <typeparam name="T">An enumerated type.</typeparam>
    public class Register<Enumeration>
    {
        #region Declarations

        // At design time there's no indication that type T allows
        // bitwise operations so we get compile errors so we'll 
        // represent the enumerated type as an integer.
        private int register;

        #endregion // Declarations

        #region Construction

        /// <summary>
        /// Instantiate a fully cleared register.
        /// </summary>
        public Register()
        {
            Clear();
        }

        /// <summary>
        /// Instantiate a register with a given starting state.
        /// </summary>
        /// <param name="flag">Logical OR of one or more enumerated values</param>
        public Register(Enumeration flag)
        {
            Clear();
            Set(flag);
        }

        #endregion // Construction

        #region Public Methods

        /// <summary>
        /// Clear the flag register completely.
        /// </summary>
        public Enumeration Clear()
        {
            register = clearAll;
            return this.State;
        }

        /// <summary>
        /// Clear one or more flags in the register.
        /// </summary>
        /// <param name="value">Logical OR of one or more enumerated values</param>
        public Enumeration Clear(Enumeration value)
        {
            int flag = Convert.ToInt32(value);
            int mask = setAll & ~flag;
            register &= mask;
            return this.State;
        }

        /// <summary>
        /// Set all flags in the register
        /// </summary>
        public Enumeration Set()
        {
            register = setAll;
            return this.State;
        }

        /// <summary>
        /// Set one or more flags in the register.
        /// </summary>
        /// <param name="value">Logical OR of one or more enumerated values</param>
        public Enumeration Set(Enumeration value)
        {
            int flag = Convert.ToInt32(value);
            register = (register | flag);
            return this.State;
        }

        /// <summary>
        /// Query whether one or more flags are set.
        /// </summary>
        /// <param name="value">Logical OR of one or more enumerated values</param>
        /// <returns>True if flag(s) set</returns>
        public bool IsSet(Enumeration value)
        {
            int flag = Convert.ToInt32(value);
            return (register & flag) == flag;
        }

        /// <summary>
        /// Return the current state of the register as the 
        /// enumerated type used to instantiate this object.
        /// </summary>
        public Enumeration State
        {
            get
            {
                return (Enumeration)Enum.ToObject(typeof(Enumeration), register);
            }
        }

        #endregion // Public Methods

        #region Private Methods

        /// <summary>
        /// A convenient way to reset the flag register.
        /// </summary>
        private int clearAll
        {
            get { return 0; }
        }

        /// <summary>
        /// A convenient way to set the flag register in one step.
        /// </summary>
        private int setAll
        {
            get
            {
                int register = clearAll;
                foreach (int flag in Enum.GetValues(typeof(Enumeration)))
                {
                    register |= flag;
                }
                return register;
            }
        }

        #endregion // Private Methods
    }
}
