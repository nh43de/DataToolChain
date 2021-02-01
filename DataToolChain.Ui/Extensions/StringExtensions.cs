using System.Collections.Generic;

namespace DataToolChain.Ui.Extensions
{
    public static class StringExtensions
    {
        //
        // Summary:
        //     Shorthand for string.Join(separator, enumerable)
        //
        // Parameters:
        //   enumerable:
        //
        //   separator:
        //
        // Type parameters:
        //   T:
        public static string JoinStr<T>(this IEnumerable<T> enumerable, string separator = ",")
        {
            return string.Join(separator, enumerable);
        }
    }
}