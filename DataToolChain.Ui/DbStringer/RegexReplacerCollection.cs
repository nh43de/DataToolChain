using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPowerTools.Connectivity.Json;
using DataPowerTools.Extensions;
using DataToolChain.Ui.Extensions;

namespace DataToolChain.DbStringer
{
    public class SelectableRegexReplacement
    {
        public bool IsChecked { get; set; } = false;


        public RegexReplacement RegexReplacement { get; set; }

        public string DisplayText => RegexReplacement?.DisplayText ?? "";

        public SelectableRegexReplacement(RegexReplacement replacement)
        {
            RegexReplacement = replacement;
        }
    }

    public class RegexReplacerCollection : List<SelectableRegexReplacement>
    {
        public string FailString { get; set; }

        public static RegexReplacerCollection DefaultReplacerCollection
        {
            get
            {
                var a = new RegexReplacerCollection();
                a.AddRange(BuiltinReplacementsList.Select(p => new SelectableRegexReplacement(p)));

                return a;
            }
        }





        public static List<RegexReplacement> BuiltinReplacementsList => new List<RegexReplacement>
        {
            new RegexReplacement("Vertical list to single quoted, comma-separated list", @"^(.*?)\r?$", "'$1',", ","),
            new RegexReplacement("Vertical list to double quoted, comma-separated list", @"^(.*?)\r?$", "\"$1\",", ","),
            new RegexReplacement("Vertical list to comma-separated list", @"\r\n", ","),
            new RegexReplacement("Comma-separated list to vertical list", @",\W+", "\r\n"),
            new RegexReplacement("Tabbed list to comma-separated list", "\t",","),
            new RegexReplacement("NULLIF", @"(.*?)\r\n", @"NULLIF($1, 0),\r\n"),
            new RegexReplacement("Smart NULLIF", @"/ *(.*?) *,\r\n", @"/ NULLIF($1, 0),\r\n"),
            new RegexReplacement("Vertical list to SQL literal string", @"^(.*?)\r?$", "'$1' + CHAR(13) + CHAR(10)", " + CHAR(13) + CHAR(10)"),
            new RegexReplacement("Vertical list to SQL UNIONS", @"^(.*?)\r?$", "SELECT '$1' \r\nUNION", "\r\nUNION"),
            new RegexReplacement("Vertical list to SQL UNION ALL", @"^(.*?)\r?$", "SELECT '$1' \r\nUNION ALL", "\r\nUNION ALL"),
            new RegexReplacement("", @"\r", "SELECT '$1' \r\nUNION ALL", "\r\nUNION ALL"),
            new RegexReplacement("Sql Input Params Into Declarations", @"^.*?(@\w+.*?)(,|$).*?(--|/\*)?.*", "DECLARE $1"),
            new RegexReplacement("Escape SQL String", "'", "''"),
            new RegexReplacement("Format JSON", str => {
                try
                {
                    return str.ToObject<object>().ToJson(true);
                }
                catch (Exception ex)
                {
                    return "Error in Json: " + ex.Message;
                }
            }),
            new RegexReplacement("Escape Regex", Regex.Escape),
            new RegexReplacement("Unescape regex", Regex.Unescape),
            new RegexReplacement("Trim", s => s.Split('\r').Select(x => x.Trim()).JoinStr("\r\n")),
            new RegexReplacement("Sort Alphabetically", s => Regex.Split(s, "\r\n?").OrderBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Sort by Length", s => Regex.Split(s, "\r\n?").OrderBy(x => x.Length).ThenBy(x => x).JoinStr("\r\n")),
            //new RegexReplacement("Distinct", s => Regex.Split(s, "\r\n?").Select(x => x.Trim()).Distinct().JoinStr("\r\n")),
            new RegexReplacement("Distinct", s => s.Split('\r').Select(x => x.Trim()).Distinct().OrderBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Params to Tabs", new []
            {
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"^\W*?'",
                    Replacement = ""
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"'\W*?$",
                    Replacement = ""
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"'\W*?,\W*?'",
                    Replacement = "\t"
                }
            }),
            new RegexReplacement("Tabs to rows", "\t", "\r\n"),
            new RegexReplacement("Tabs to params", new []
            {
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "^",
                    Replacement = "'"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "\t",
                    Replacement = "', '"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "\r\n",
                    Replacement = "'\r\n"
                }
            }),
            new RegexReplacement("SQL Columns to DECLARE statement", new []
            {
                new RegexReplacement.RegexReplacementStep
                {
                    Pattern = @"^[ \t]*\[?((?<=\[)[^\[\]]+(?=\])|[a-z0-9]+)\]?[ \t]*\[?([^\[\]]+)\]?[ \t]*(\([0-9]+\))?.*?,\r\n",
                    Replacement = @"DECLARE @$1 $2$3;\r\n"
                }
            }),
            new RegexReplacement("Tabs to SQL Columns", new []
            {
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "^",
                    Replacement = "UNION SELECT '"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "\t",
                    Replacement = "', '"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = "\r\n",
                    Replacement = "'\r\n"
                }
            }),
            new RegexReplacement("Generalize Regex - use ^^^ to enclose groups, and _________ to indicate a wildcard", new []
            {
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\\",
                    Replacement = @"\\\\"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\*",
                    Replacement = @"\\\*"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\[",
                    Replacement = @"\\["
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\]",
                    Replacement = @"\\]"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\.",
                    Replacement = @"\\."
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\[[a-z 0-9]+?\]",
                    Replacement = @"{7D12F4D5-9A13-4F2D-BEB7-5FF8C1A199E0}"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\(",
                    Replacement = @"\\("
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\)",
                    Replacement = @"\\)"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"[ \t]+",
                    Replacement = @"\[ \\t]*"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"{7D12F4D5-9A13-4F2D-BEB7-5FF8C1A199E0}",
                    Replacement = @"\[[a-z 0-9]+?\]"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\r",
                    Replacement = @"\\r"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\n",
                    Replacement = @"\\n"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"\^\^\^(.*?)\^\^\^",
                    Replacement = @"\($1\)"
                },
                new RegexReplacement.RegexReplacementStep {
                    Pattern = @"___",
                    Replacement = @".+"
                }
            })

        };
    }
}
