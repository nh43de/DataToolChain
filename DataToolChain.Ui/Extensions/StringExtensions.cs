using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DataToolChain.Ui.Extensions
{
    public static class StringExtensions
    {
        public static readonly Regex IndentRegex = new Regex("^", RegexOptions.Compiled | RegexOptions.Multiline);

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


        /// <summary>
        /// Indent each line using tabs.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tabs"></param>
        /// <returns></returns>
        public static string Indent(this string str, int tabs = 1)
        {
            return IndentRegex.Replace(str, "".PadLeft(tabs, '\t'));
        }

    }
}