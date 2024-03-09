using System.Linq;
using DataPowerTools.Extensions;
using DataToolChain.DbStringer;

namespace DataToolChain.Ui.Extensions;

public static class EnumerableExtensions
{
    public static int IndexOfFirstEmptyLine(this string[] lines)
    {
        var i = 0;

        foreach (var str in lines)
        {
            if (string.IsNullOrWhiteSpace(str))
                break;

            i++;
        }

        return i;
    }


    /// <summary>
    /// Splits a bunch of lines into two parts: before first empty line and after (for doing except operations, etc.)
    /// </summary>
    public static (string[] firstPart, string[] secondPart)? SplitLines(this string src)
    {
        try
        {
            var lines = RegexReplacerCollection.NewLineRegex().Split(src);

            var i = lines.IndexOfFirstEmptyLine();

            if (i >= lines.Length)
                return null;

            var firstPart = lines[0..(i)];
            var secondPart = lines[(i + 1)..];

            return (firstPart, secondPart);
        }
        catch
        {
            return null;
        }
    }


}
