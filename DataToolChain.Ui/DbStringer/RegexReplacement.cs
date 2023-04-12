using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPowerTools.Extensions;
using DataToolChain.Ui.Extensions;

namespace DataToolChain.DbStringer
{
    public class RegexReplacement
    {
        public string Name { get; private set; }

        private RegexReplacement(string name)
        {
            Name = name;
        }
        
        
        public RegexReplacement(string name, string pattern, string replacement, string trimEndString = null) : this(name)
        {
            
            RegexReplacementSteps = new[]
            {
                new RegexReplacementStep
                {
                    TrimEndString = trimEndString,
                    Pattern = pattern,
                    Replacement = replacement
                }
            };
        }

        private IEnumerable<IRegexReplacementStep> RegexReplacementSteps { get; set; }

        private Func<string, string> ReplacementFunc { get; set; } = null;

        public RegexReplacement(string name, IEnumerable<IRegexReplacementStep> replacementSteps) : this(name)
        {
            
            RegexReplacementSteps = replacementSteps;
        }

        public RegexReplacement(string name, Func<string, string> replacementFunc) : this(name)
        {
            ReplacementFunc = replacementFunc;
        }

        public interface IRegexReplacementStep
        {
            string Pattern { get; }
            string Replacement { get; }
            string TrimEndString { get; }
            string DisplayText { get; }

            string Process(string input);
        }

        public class RegexReplacementStepFunc : IRegexReplacementStep
        {
            private readonly Func<string, string> _replacementFunc;
            public string Pattern { get; } = "input";
            public string Replacement { get; set; }

            public string TrimEndString { get; set; }
            public string DisplayText => $"[{Pattern}] -> [{Replacement}]";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="replacementFunc"></param>
            /// <param name="replacementSummary">Description of the replacement func</param>
            /// <param name="trimEndString"></param>
            public RegexReplacementStepFunc(Func<string, string> replacementFunc, string replacementSummary, string trimEndString = null)
            {
                _replacementFunc = replacementFunc;
                Replacement = replacementSummary;
                TrimEndString = trimEndString;
            }

            public RegexReplacementStepFunc()
            {

            }

            public string Process(string input)
            {
                return _replacementFunc(input);
            }
        }

        public class RegexReplacementStep : IRegexReplacementStep
        {
            public string Pattern { get; set; }
            public string Replacement { get; set; }

            public string TrimEndString { get; set; }
            public string DisplayText => $"[{Pattern}] -> [{Replacement}]";

            public RegexReplacementStep(string pattern, string replacement, string trimEndString)
            {
                Pattern = pattern;
                Replacement = replacement;
                TrimEndString = trimEndString;
            }

            public RegexReplacementStep()
            {

            }

            public string Process(string input)
            {
                return RegexReplace(this, input);
            }
        }

        public static string RegexReplace(RegexReplacement r, string text)
        {
            if (text == null)
                return string.Empty;

            if (r.ReplacementFunc != null)
                return r.ReplacementFunc.Invoke(text);

            return r
                .RegexReplacementSteps
                .Aggregate(text, (current, regexReplacementStep) 
                    => regexReplacementStep.Process(current)); //aggregates all the regex replacement steps
        }

        public static string RegexReplace(RegexReplacementStep r, string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var rtn = new Regex(r.Pattern, RegexOptions.Multiline).Replace(text, Regex.Unescape(r.Replacement));
            if (!string.IsNullOrWhiteSpace(r.TrimEndString) && rtn.EndsWith(r.TrimEndString))
            {
                return rtn.Substring(0, rtn.Length - r.TrimEndString.Length);
            }

            return rtn;
        }

        public string DisplayText => Name + ": " + RegexReplacementSteps?.Select((r, i) => r.DisplayText).JoinStr("\r\n");
    }
}