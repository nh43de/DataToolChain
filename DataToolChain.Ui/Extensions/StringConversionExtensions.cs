using System;

namespace DataToolChain.Ui.Extensions
{
    public static class StringConversionExtensions
    {
        public static long? ToNullableShort(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (short.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static TimeSpan? ToNullableTimespan(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (TimeSpan.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static int? ToNullableInt(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (int.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static decimal? ToNullableDecimal(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (decimal.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static double? ToNullableDouble(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (double.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static long? ToNullableLong(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (long.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static Guid? ToNullableGuid(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (Guid.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static float? ToNullableFloat(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (float.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

    }
}
