using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataToolChain.Ui.Extensions;

public static class ChunkifyExtensions
{
    private static Regex _whitespaceRegex = new Regex(@"([\s''?""’‘\\/\.]|-|_|\W|[^a-z0-9])+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string[] ChunkifyString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<string>();

        var cleanName = _whitespaceRegex.Replace(input, " ").Trim(); //could also just use regex.split

        var chunks = cleanName
            .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        return chunks;
    }
}