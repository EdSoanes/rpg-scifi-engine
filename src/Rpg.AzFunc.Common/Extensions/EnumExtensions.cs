using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigill.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T, TS>(this TS value, T defaultValue)
            => ToEnum(value!.ToString()!, defaultValue);

        public static T ToEnum<T>(this string value, T defaultValue)
        {
            return Enum.TryParse(typeof(T), value, true, out var result)
                ? (T)result
                : defaultValue;
        }
    }
}
