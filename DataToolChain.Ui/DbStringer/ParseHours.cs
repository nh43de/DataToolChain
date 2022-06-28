using System;
using System.Linq;
using System.Text.RegularExpressions;
using DataToolChain.Ui.Extensions;

namespace DataToolChain.Ui.DbStringer
{
    public class ParseHours
    {
        //
        //private static readonly Regex HoursRegex = new Regex(@"(<hour>[0-9]{1,2}):?(?<minute>[0-9]{2})?\W*(?<tod>am|pm)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HoursRegex = new Regex(@"(?<preText>.*?)(?<hour1>[0-9]{1,2}):?(?<minute1>[0-9]{2})?\s*(?<tod1>am|pm)?\s*-\s*(?<hour2>[0-9]{1,2}?):?(?<minute2>[0-9]{2})?\s*(?<tod2>am|pm)?($|\W)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static string Parse(string timeStrings)
        {
            var tStr = timeStrings.Split("\r\n");

            var tStr2 = tStr.Select(p =>
            {
                if (string.IsNullOrWhiteSpace(p))
                    return "";

                var match1 = HoursRegex.Matches(p);

                if (match1.Any() == false)
                    return "No match";

                var hour = match1[0]?.Groups["hour1"]?.Value?.ToNullableInt() % 12;
                var minute = match1[0].Groups["minute1"]?.Value?.ToNullableInt() ?? 0;
                var tod = match1[0].Groups["tod1"]?.Value?.ToLower();
                var tdf = tod == "am" ? 0 : 12;
                
                var hour2 = match1[0]?.Groups["hour2"]?.Value?.ToNullableInt() % 12;
                var minute2 = match1[0].Groups["minute2"]?.Value?.ToNullableInt() ?? 0;
                var tod2 = match1[0].Groups["tod2"]?.Value?.ToLower();
                var tdf2 = tod2 == "am" ? 0 : 12;

                if (hour == null)
                    return "No start hour";

                if (hour2 == null)
                    return "No end hour";

                var tsStart = new TimeSpan(0, hour.Value + tdf, minute, 0);

                var tsEnd = new TimeSpan(0, hour2.Value + tdf2, minute2, 0);

                if (tsStart > tsEnd)
                {
                    tsStart = tsStart.Add(TimeSpan.FromHours(-12));
                }

                var tsDiff = tsEnd - tsStart;

                var preText = match1[0]?.Groups["preText"]?.Value;

                var r = $"{preText}{tsStart.Hours}:{tsStart.Minutes:D2}-{tsEnd.Hours}:{tsEnd.Minutes:D2} \t{Math.Round(tsDiff.TotalHours, 2)}\thours";

                return r;
            });

            return tStr2.JoinStr("\r\n");
        }
    }
}
