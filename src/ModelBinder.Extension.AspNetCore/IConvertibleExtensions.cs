using System;
using System.Globalization;

namespace ModelBinder.Extension.AspNetCore
{
    public static class IConvertibleExtensions
    {
        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>

        public static T ConvertTo<T>(this IConvertible value) where T : IConvertible
        {
            return (T)ConvertTo(value, typeof(T));
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns></returns>
        public static T TryConvertTo<T>(this IConvertible value, T defaultValue = default) where T : IConvertible
        {
            try
            {
                return (T)ConvertTo(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="result">转换失败的默认值</param>
        /// <returns></returns>
        public static bool TryConvertTo<T>(this IConvertible value, out T result) where T : IConvertible
        {
            try
            {
                result = (T)ConvertTo(value, typeof(T));
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type">目标类型</param>
        /// <param name="result">转换失败的默认值</param>
        /// <returns></returns>
        public static bool TryConvertTo(this IConvertible value, Type type, out object result)
        {
            try
            {
                result = ConvertTo(value, type);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static object ConvertTo(this IConvertible value, Type type)
        {
            if (null == value)
            {
                return default;
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType);
            }

            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 类型直转 
        ///     https://stackoverflow.com/questions/18015425/invalid-cast-from-system-int32-to-system-nullable1system-int32-mscorlib
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static object ChangeType(this object value, Type type)
        {
            var t = type;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            if (t.IsEnum)
            {
                string s = Convert.ToString(value);
                return Enum.Parse(t, s, true);
            }
            if (t == typeof(Guid))
            {
                string s = Convert.ToString(value);
                return Guid.Parse(s);
            }
            return Convert.ChangeType(value, t);
        }
    }
}
