using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataPowerTools.Connectivity.Json;
using DataPowerTools.Extensions;
using DataPowerTools.FileSystem;
using DataPowerTools.PowerTools;
using DataToolChain.Ui.DbStringer;
using DataToolChain.Ui.Extensions;
using MoreLinq.Extensions;
using static DataToolChain.DbStringer.RegexReplacement;

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

    public partial class RegexReplacerCollection : List<SelectableRegexReplacement>
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


        //

        public static List<RegexReplacement> BuiltinReplacementsList => new List<RegexReplacement>
        {
            new RegexReplacement("Vertical list to single quoted, comma-separated list", @"^(.*?)\r?$", "'$1',", ","),
            new RegexReplacement("Vertical list to double quoted, comma-separated list", @"^(.*?)\r?$", "\"$1\",", ","),
            new RegexReplacement("Vertical list to comma-separated list", @"\r\n", ","),
            new RegexReplacement("Vertical list to comma-separated list (one item per line)", @"\r\n", ",\r\n"),
            new RegexReplacement("Vertical list to space-separated list",s => NewLineRegex().Split(s).JoinStr(" ")),
            new RegexReplacement("Comma-separated list to vertical list", @",\W*", "\r\n"),
            new RegexReplacement("Tabbed list to comma-separated list", "\t", ","),
            new RegexReplacement("Tabbed list to rows", "\t", "\r\n"),
            new RegexReplacement("Vertical list to tabbed list", @"\r\n?", "\t", ","),
            new RegexReplacement("Vertical list to sum", s => NewLineRegex().Split(s).JoinStr(" + ")),
            new RegexReplacement("Vertical list to SQL literal string", @"^(.*?)\r?$", "'$1' + CHAR(13) + CHAR(10)", " + CHAR(13) + CHAR(10)"),
            new RegexReplacement("Vertical list to SQL UNIONS", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"'",
                    Replacement = "''"
                },
                new RegexReplacementStep
                {
                    Pattern = @"^(.*?)\r?$",
                    Replacement = "SELECT '$1' \r\nUNION",
                    TrimEndString = "\r\nUNION"
                }
            }),
            new RegexReplacement("Vertical list to SQL UNION ALL", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"'",
                    Replacement = "''"
                },
                new RegexReplacementStep
                {
                    Pattern = @"^(.*?)\r?$",
                    Replacement = "SELECT '$1' \r\nUNION ALL",
                    TrimEndString = "\r\nUNION ALL"
                }
            }),
            new RegexReplacement("List to C# auto-properties", s => NewLineRegex().Split(s).Where(p => string.IsNullOrWhiteSpace(p) == false).Select(p => $"public cs_type {SanitizeColumn(p)} {{ get; set; }}").JoinStr("\r\n")),
            new RegexReplacement("Vertical list to pipe or (|)", s => NewLineRegex().Split(s).JoinStr(" | ")),
           
            new RegexReplacement("Vertical list (or table) to switch arms", s => s.ReadCsvString('\t', false)
                .SelectRows(dr =>
                {
                    if (dr.FieldCount > 1 && string.IsNullOrWhiteSpace(dr[1]?.ToString()) == false)
                    {
                        return $"{dr[0]} => {dr[1]},";
                    }

                    return $"{dr[0]} => ,";
                })
                .JoinStr("\r\n")),
           
            new RegexReplacement("Vertical list to Regex alternative match expression", new IRegexReplacementStep[]
            {
                new RegexReplacementStepFunc(Regex.Escape, "regex escape"),
                new RegexReplacementStep
                {
                    Pattern = @"(.*?)(\\r\\n|\\r|\\n)",
                    Replacement = "$1|"
                },
                new RegexReplacementStep
                {
                    Pattern = @"^",
                    Replacement = "\\("
                },
                new RegexReplacementStep
                {
                    Pattern = @"$",
                    Replacement = "\\)"
                },
            }),
            new RegexReplacement("NULLIF", @"(.*?)\r\n", @"NULLIF($1, 0),\r\n"),
            new RegexReplacement("Smart NULLIF", @"/ *(.*?) *,\r\n", @"/ NULLIF($1, 0),\r\n"),
            new RegexReplacement("Tabs to SQL Columns", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = "^",
                    Replacement = "UNION SELECT '"
                },
                new RegexReplacementStep
                {
                    Pattern = "\t",
                    Replacement = "', '"
                },
                new RegexReplacementStep
                {
                    Pattern = "\r\n",
                    Replacement = "'\r\n"
                }
            }),
            new RegexReplacement("", @"\r", "SELECT '$1' \r\nUNION ALL", "\r\nUNION ALL"),
            new RegexReplacement("Sql Input Params Into Declarations", @"^.*?(@\w+.*?)(,|$).*?(--|/\*)?.*", "DECLARE $1"),
            new RegexReplacement("Escape SQL String", "'", "''"),
            new RegexReplacement("Format JSON", str =>
            {
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
            new RegexReplacement("Escape Regex (no $)", s =>
            {
                var r = Regex.Escape(s).Replace(@"\$", "$");

                return r;
            }),
            new RegexReplacement("Unescape regex", Regex.Unescape),
            new RegexReplacement("Unescape regex $ ", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"\\\$",
                    Replacement = @"\$"
                }
            }),
            new RegexReplacement("To Lower Case", s => s.ToLower()),
            new RegexReplacement("To Upper Case", s => s.ToUpper()),
            new RegexReplacement("To Title Case", ToTitleCaseNoRegex),
            new RegexReplacement("Trim", s => s.Split('\r').Select(x => x.Trim()).JoinStr("\r\n")),
            new RegexReplacement("Trim and Remove Empty Lines", s => s.Split('\r').Select(x => x.Trim()).Where(p => string.IsNullOrEmpty(p) == false).JoinStr("\r\n")),
            new RegexReplacement("Remove Empty Lines", s => NewLineRegex().Split(s).Where(p => string.IsNullOrWhiteSpace(p) == false).JoinStr("\r\n")),
            new RegexReplacement("Remove Line Breaks", s => NewLineRegex().Split(s).JoinStr("")),
            new RegexReplacement("Shuffle Lines", s => NewLineRegex().Split(s).Shuffle().JoinStr("\r\n")),
            //new RegexReplacement("Distinct", s => Regex.Split(s, "\r\n?").Select(x => x.Trim()).Distinct().JoinStr("\r\n")),
            new RegexReplacement("Distinct", s => s.Split('\r').Select(x => x.Trim()).Distinct().OrderBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Except", s =>
            {
                try
                {
                    var lines = NewLineRegex().Split(s);

                    var i = lines.IndexOfFirstEmptyLine();

                    if (i >= lines.Length)
                        return string.Empty;

                    var firstPart = lines[0..(i)];
                    var secondPart = lines[(i + 1)..];

                    return firstPart.Except(secondPart).JoinStr("\r\n");
                }
                catch
                {
                    return string.Empty;
                }
            }),
            new RegexReplacement("Intersect", s =>
            {
                try
                {
                    var lines = NewLineRegex().Split(s);

                    var i = lines.IndexOfFirstEmptyLine();

                    if (i >= lines.Length)
                        return string.Empty;

                    var firstPart = lines[0..(i)];
                    var secondPart = lines[(i + 1)..];

                    return firstPart.Intersect(secondPart).JoinStr("\r\n");
                }
                catch
                {
                    return string.Empty;
                }
            }),

            new RegexReplacement("Count Lines", s => NewLineRegex().Split(s).Count().ToString()),
            new RegexReplacement("Count Non-Empty Lines", s => NewLineRegex().Split(s).Count(p => string.IsNullOrWhiteSpace(p) == false).ToString()),
            new RegexReplacement("Count Distinct Lines", s => NewLineRegex().Split(s).Distinct().Count().ToString()),
            new RegexReplacement("Count Distinct Non-Empty Lines", s => NewLineRegex().Split(s).Where(p => string.IsNullOrWhiteSpace(p) == false).Select(p => p.Trim()).Distinct().Count().ToString()),
            new RegexReplacement("Group and Count", s => Regex.Split(s, "\r\n?")
                .Select(p => p.Trim())
                .GroupBy(p => p)
                .Select(p => new
                {
                    Count = p.Count(),
                    p.Key
                })
                .OrderByDescending(p => p.Count)
                .Select(p => p.Key + '\t' + p.Count)
                .JoinStr("\r\n")),
            new RegexReplacement("List Duplicated Values", s => Regex.Split(s, "\r\n?")
                .Select(p => p.Trim())
                .GroupBy(p => p)
                .Select(p => new
                {
                    Count = p.Count(),
                    p.Key
                })
                .Where(p => p.Count > 1)
                .OrderByDescending(p => p.Count)
                .Select(p => p.Key)
                .JoinStr("\r\n")),
            new RegexReplacement("Sort Alphabetically", s => Regex.Split(s, "\r\n?").OrderBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Sort Alphabetically Desc", s => Regex.Split(s, "\r\n?").OrderByDescending(x => x).JoinStr("\r\n")),
            new RegexReplacement("Sort by Length", s => Regex.Split(s, "\r\n?").OrderBy(x => x.Length).ThenBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Sort by Length Desc", s => Regex.Split(s, "\r\n?").OrderByDescending(x => x.Length).ThenBy(x => x).JoinStr("\r\n")),
            new RegexReplacement("Condense Whitespace", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"[ \t\r\n]+",
                    Replacement = " "
                }
            }),
            new RegexReplacement("Sanitize Column Names", SanitizeColumnReplace),
            new RegexReplacement("Sanitize Quotes", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"['‘’“”]",
                    Replacement = "\""
                }
            }),
            new RegexReplacement("Vertical URL list to HTML image tags", @"^(.*)$", "<img src='$1'/>", null, true),
            new RegexReplacement("SQL Parameters to Tabbed List", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"^\W*?'",
                    Replacement = ""
                },
                new RegexReplacementStep
                {
                    Pattern = @"'\W*?$",
                    Replacement = ""
                },
                new RegexReplacementStep
                {
                    Pattern = @"'\W*?,\W*?'",
                    Replacement = "\t"
                }
            }),
            new RegexReplacement("Tabbed list to SQL Parameters", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = "^",
                    Replacement = "'"
                },
                new RegexReplacementStep
                {
                    Pattern = "\t",
                    Replacement = "', '"
                },
                new RegexReplacementStep
                {
                    Pattern = "\r\n",
                    Replacement = "'\r\n"
                }
            }),
            new RegexReplacement("SQL Columns to DECLARE statement", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"^[ \t]*\[?((?<=\[)[^\[\]]+(?=\])|[a-z0-9]+)\]?[ \t]*\[?([^\[\]]+)\]?[ \t]*(\([0-9]+\))?.*?,\r\n",
                    Replacement = @"DECLARE @$1 $2$3;\r\n"
                }
            }),
            new RegexReplacement("Generalize Regex - use ^^^ to enclose groups, and _________ to indicate a wildcard", new[]
            {
                new RegexReplacementStep
                {
                    Pattern = @"\\",
                    Replacement = @"\\\\"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\*",
                    Replacement = @"\\\*"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\[",
                    Replacement = @"\\["
                },
                new RegexReplacementStep
                {
                    Pattern = @"\]",
                    Replacement = @"\\]"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\.",
                    Replacement = @"\\."
                },
                new RegexReplacementStep
                {
                    Pattern = @"\[[a-z 0-9]+?\]",
                    Replacement = @"{7D12F4D5-9A13-4F2D-BEB7-5FF8C1A199E0}"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\(",
                    Replacement = @"\\("
                },
                new RegexReplacementStep
                {
                    Pattern = @"\)",
                    Replacement = @"\\)"
                },
                new RegexReplacementStep
                {
                    Pattern = @"[ \t]+",
                    Replacement = @"\[ \\t]*"
                },
                new RegexReplacementStep
                {
                    Pattern = @"{7D12F4D5-9A13-4F2D-BEB7-5FF8C1A199E0}",
                    Replacement = @"\[[a-z 0-9]+?\]"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\r",
                    Replacement = @"\\r"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\n",
                    Replacement = @"\\n"
                },
                new RegexReplacementStep
                {
                    Pattern = @"\^\^\^(.*?)\^\^\^",
                    Replacement = @"\($1\)"
                },
                new RegexReplacementStep
                {
                    Pattern = @"___",
                    Replacement = @".+"
                }
            }),
            new RegexReplacement("Parse Hours", ParseHours.Parse),
            new RegexReplacement("Format Json", s =>
            {
                try
                {
                    var o = s.ToObject<object>();

                    var r = o.ToJson(true);

                    return r;
                }
                catch (Exception)
                {
                    return "Error in Json.";
                }
            }),
            new RegexReplacement("UnPivot CSV table (tab)", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.UnPivot();

                    return dd.AsCsv(true, true);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("UnPivot CSV table (comma)", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.UnPivot();

                    return dd.AsCsv(true, false);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            new RegexReplacement("UnPivot2 CSV table (tab)", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.UnPivot(2);

                    return dd.AsCsv(true, true);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("UnPivot2 CSV table (comma)", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.UnPivot(2);

                    return dd.AsCsv(true, false);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            
            new RegexReplacement("UnPivot3 CSV table (tab)", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.UnPivot(3);

                    return dd.AsCsv(true, true);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("UnPivot3 CSV table (comma)", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.UnPivot(3);

                    return dd.AsCsv(true, false);
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            new RegexReplacement("CSV (tab) to Create Table SQL", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.FitToCreateTableSql("MyTable", null);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to Create Table SQL", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.FitToCreateTableSql("MyTable", null);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            new RegexReplacement("CSV (tab) to C# Class", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.FitToCsharpClass("MyClass", null);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to C# Class", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.FitToCsharpClass("MyClass", null);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            
            new RegexReplacement("CSV (tab) to C# Array", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.ReadToCSharpArray();

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (tab) to C# Array (anonymous type)", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.ReadToCSharpArray(true);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to C# Array", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.ReadToCSharpArray();

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            //sql inserts
            new RegexReplacement("CSV (tab) to SQL inserts", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.AsSqlInsertStatements("[MyTable]", DatabaseEngine.SqlServer);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to SQL inserts", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.AsSqlInsertStatements("[MyTable]", DatabaseEngine.SqlServer);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            
            //sql selects
            new RegexReplacement("CSV (tab) to SQL UNION selects", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.AsSqlSelectStatements(DatabaseEngine.SqlServer);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to SQL UNION selects", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.AsSqlSelectStatements(DatabaseEngine.SqlServer);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),

            //
            new RegexReplacement("CSV (tab) to JSON array", s =>
            {
                try
                {
                    var o = s.ReadCsvString('\t', true);

                    var dd = o.ReadToJson(true);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            new RegexReplacement("CSV (comma) to JSON array", s =>
            {
                try
                {
                    var o = s.ReadCsvString(',', true);

                    var dd = o.ReadToJson( true);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error reading csv table";
                }
            }),
            
            new RegexReplacement("JSON array to CSharp Object Initializer", s =>
            {
                try
                {
                    var dd = s.FromJsonToCsharpObjectInit();

                    return dd;
                }
                catch (Exception)
                {
                    return "Error parsing json";
                }
            }),

            new RegexReplacement("JSON array to SQL inserts", s =>
            {
                try
                {
                    var dd = s.FromJsonToSqlInsertStatements("[MyTable]", DatabaseEngine.SqlServer);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error parsing json";
                }
            }),

            new RegexReplacement("JSON array to CSV (comma)", s =>
            {
                try
                {
                    var dd = s.FromJsonToCsv(true, false);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error parsing json";
                }
            }),

            new RegexReplacement("JSON array to CSV (tab)", s =>
            {
                try
                {
                    var dd = s.FromJsonToCsv(true, true);

                    return dd;
                }
                catch (Exception)
                {
                    return "Error parsing json array";
                }
            }),

            new RegexReplacement("C# class to simple map (new)", s =>
            {
                try
                {
                    var options = RegexOptions.IgnoreCase;

                    var matches = RegexMatcherViewModel.Match(@"public\W+.*?\W+(.*?)\W+\{.*[\r\n]+", options, s+"\r\n", true, true, false);

                    var dd = RegexReplace(new RegexReplacementStep(@"(.*?)[\r\n]+", @"$1 = p\.$1,\r\n", null), string.Join("\r\n", matches) + "\r\n");

                    return dd;
                }
                catch (Exception ex)
                {
                    return "Error parsing class " +  ex.Message;
                }
            }),

            new RegexReplacement("C# class to simple map (map class properties from one obj to another)", s =>
            {
                try
                {
                    var options = RegexOptions.IgnoreCase;

                    var matches = RegexMatcherViewModel.Match(@"public\W+.*?\W+(.*?)\W+\{.*[\r\n]+", options, s+"\r\n", true, true, false);

                    var dd = RegexReplace(new RegexReplacementStep(@"(.*?)[\r\n]+", @"a.$1 = b\.$1;\r\n", null), string.Join("\r\n", matches) + "\r\n");

                    return dd;
                }
                catch (Exception ex)
                {
                    return "Error parsing class " +  ex.Message;
                }
            }),

            new RegexReplacement("C# class - list public properties", s =>
            {
                try
                {
                    var options = RegexOptions.IgnoreCase;

                    var matches = RegexMatcherViewModel.Match(@"public\W+.*?\W+(.*?)\W+\{.*[\r\n]+", options, s+"\r\n", true, true, false);

                    return matches.JoinStr("\r\n");
                }
                catch (Exception ex)
                {
                    return "Error parsing class " +  ex.Message;
                }
            }),

            new RegexReplacement("Enumerate files in a directory", s =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s) || Path.Exists(s) == false)
                        return null;

                    var files = Directory.EnumerateFiles(s).Select(p => Path.GetFileName(p)).JoinStr("\r\n");

                    return files;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }),

            new RegexReplacement("Enumerate files in a directory (without extensions)", s =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s) || Path.Exists(s) == false)
                        return null;

                    var files = Directory.EnumerateFiles(s).Select(p => Path.GetFileNameWithoutExtension(p)).JoinStr("\r\n");

                    return files;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }),
            new RegexReplacement("Enumerate files in a directory (recursive)", s =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s) || Path.Exists(s) == false)
                        return null;

                    var strings = new List<string>();

                    DirectoryScanner.ScanRecursive(s, s1 => strings.Add(Path.GetFileName(s1)));

                    var files = strings.JoinStr("\r\n");

                    return files;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }),

            new RegexReplacement("Enumerate files in a directory (recursive) (without extensions) ", s =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s) || Path.Exists(s) == false)
                        return null;

                    var strings = new List<string>();

                    DirectoryScanner.ScanRecursive(s, s1 => strings.Add(Path.GetFileNameWithoutExtension(s1)));

                    var files = strings.JoinStr("\r\n");

                    return files;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }),

            new RegexReplacement("Enumerate files in a directory (recursive) (full path) ", s =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(s) || Path.Exists(s) == false)
                        return null;

                    var strings = new List<string>();

                    DirectoryScanner.ScanRecursive(s, s1 => strings.Add(s1));

                    var files = strings.JoinStr("\r\n");

                    return files;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }),

            new RegexReplacement("GZip Compress Text", s => GZipHelper.CompressString(s)),
            new RegexReplacement("DEFLATE Compress Text", s => DeflateHelper.CompressString(s)),
            new RegexReplacement("GZip Decompress Text", s => GZipHelper.DecompressString(s)),
            new RegexReplacement("DEFLATE Decompress Text", s => DeflateHelper.DecompressString(s)),


        };

        [GeneratedRegex("(?=[A-Z])")]
        private static partial Regex CapitalRegex();

        private static string ToTitleCaseNoRegex(string s)
        {
            var properCase = new CultureInfo("en-US", false).TextInfo;
            var sr = properCase.ToTitleCase(s.ToLower());

            return sr;
        }

        private static string ToTitleCase(string s)
        {
            var properCase = new CultureInfo("en-US", false).TextInfo;

            var sr = CapitalRegex().Split(s).Select(p => properCase.ToTitleCase(p)).JoinStr(" ");

            return sr;
        }

        private static IRegexReplacementStep[] SanitizeColumnReplace { get; } = new IRegexReplacementStep[]
        {
            new ReplacementStep("To Title Case", ToTitleCase),
            new RegexReplacementStep
            {
                Pattern = @"[ \\\)\(\[\]/-]",
                Replacement = ""
            },
            new RegexReplacementStep
            {
                Pattern = @"#",
                Replacement = "Num"
            },
            new RegexReplacementStep
            {
                Pattern = @"\$",
                Replacement = "Dollar"
            },
            new RegexReplacementStep
            {
                Pattern = @"%",
                Replacement = "Pct"
            },
            new StringReplacementStep
            {
                Pattern = "ID",
                Replacement = "Id",
                CaseSensitive = true
            }
        };

        public static string SanitizeColumn(string s)
        {
            var r = RegexReplacement.RegexReplace(SanitizeColumnReplace, s);

            return r;
        }
        
        [GeneratedRegex("\r\n?")]
        public static partial Regex NewLineRegex();
    }
}

