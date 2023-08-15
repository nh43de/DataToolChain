using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DataPowerTools.Extensions;
using DataToolChain.Ui.Extensions;

namespace DataToolChain.RegexMaker
{
    public class RegexReplacerViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;

        //        private string _stringInput = "Apple\t10\tRed\r\nPear\t5\tGreen\r\nOrange\t15\tOrange";

        private string _stringInput = @"CREATE TABLE [dbo].[Generics](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Suffix] [nvarchar](255) NULL
	[Reinv Type] [nvarchar](255) NULL,
	[Price] [float] NULL
)";

        private string _regexMatchInputs = @"nvarchar
float
NVARCHAR(255)
smallint
datetime
float
(.*?)\t(.*?)\t(.*?)\r\n";

        private string _regexReplaceInputs = @"NVARCHAR
FLOAT
VARCHAR(50)
SMALLINT
DATETIME
FLOAT
$1\t$2\t$3\r\n";

        private bool _useRegex = false;
        private bool _isCaseSensitive = false;
        private bool _eachLine = false;
        private bool _magicMode = false;
        private bool _multiline;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool MagicMode
        {
            get { return _magicMode; }
            set
            {
                _magicMode = value;
                UseRegex = true;
                UpdateOutput();
            }
        }

        public bool EachLine
        {
            get { return _eachLine; }
            set
            {
                _eachLine = value;
                UpdateOutput();
            }
        }

        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set
            {
                _isCaseSensitive = value;
                UpdateOutput();
            }
        }

        public bool UseRegex
        {
            get { return _useRegex; }
            set
            {
                _useRegex = value;
                OnPropertyChanged();
                UpdateOutput();
            }
        }

        public bool Multiline
        {
            get { return _multiline; }
            set
            {
                _multiline = value;
                UpdateOutput();
            }
        }


        public RegexOptions GetRegexOptions()
        {
            var regexOptions = RegexOptions.None;

            if (Multiline == false)
            {
                regexOptions |= RegexOptions.Multiline;
            }
            else
            {
                regexOptions |= RegexOptions.Singleline;
            }

            if (IsCaseSensitive == false)
                regexOptions |= RegexOptions.IgnoreCase;

            return regexOptions;
        }

        public string RegexReplaceInputs
        {
            get { return _regexReplaceInputs; }
            set
            {
                _regexReplaceInputs = value;
                UpdateOutput();
            }
        }

        public string RegexMatchInputs
        {
            get { return _regexMatchInputs; }
            set
            {
                _regexMatchInputs = value;
                UpdateOutput();
            }
        }

        public string StringInput
        {
            get { return _stringInput; }
            set
            {
                _stringInput = value;
                UpdateOutput();
            }
        }

        public string StringOutput
        {
            get { return _stringOutput; }
            set
            {
                _stringOutput = value;
                OnPropertyChanged();
            }
        }

        public RegexReplacerViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            var matchesInput = RegexMatchInputs.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            var replacementInput = RegexReplaceInputs.Split(new[] { "\r\n" }, StringSplitOptions.None);

            var zippedStrings = matchesInput
                .Zip(replacementInput, (match, replacement) => new
            {
                RegexMatchPattern = UseRegex ? match : Regex.Escape(match),
                RegexReplacePattern = replacement == null ? ""
                                    : UseRegex ?  Regex.Unescape(replacement) : replacement,
            }).Where(p => string.IsNullOrEmpty(p.RegexMatchPattern) == false);

            try
            {
                var regexOptions = GetRegexOptions();

                string ProcessReplacements(string input)
                {
                    foreach (var rr in zippedStrings)
                    {
                        if (UseRegex)
                        {
                            var replacePattern = MagicMode ? (Regex.Unescape(Regex.Escape(RegexReplaceInputs).Replace(@"\$", "$"))) : rr.RegexReplacePattern;

                            input = Regex.Replace(input, rr.RegexMatchPattern, replacePattern, regexOptions);
                        }
                        else
                        {
                            MatchEvaluator me = match => rr.RegexReplacePattern;
                            input = Regex.Replace(input, rr.RegexMatchPattern, me, regexOptions);
                        }
                    }

                    return input;
                }

                if (EachLine)
                {
                    var lines = Regex.Split(StringInput, "\r\n?");

                    var outputString = lines.Select(ProcessReplacements).JoinStr("\r\n");

                    StringOutput = outputString;
                }
                else
                {
                    var outputString = ProcessReplacements(StringInput);

                    StringOutput = outputString;
                }
            }
            catch (Exception)
            {
                StringOutput = "Error in regex.";
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }




}