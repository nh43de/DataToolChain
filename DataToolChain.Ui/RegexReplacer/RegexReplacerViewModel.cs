using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DataToolChain.Ui.Extensions;

namespace DataToolChain.RegexMaker
{
    public class RegexReplacerViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"CREATE TABLE [dbo].[Generics](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Suffix] [nvarchar](255) NULL,
	[Hierarchy 5] [nvarchar](255) NULL,
	[Account] [nvarchar](255) NULL,
	[Unique ID] [nvarchar](255) NULL,
	[Reinv Type] [nvarchar](255) NULL,
	[Maturity] [float] NULL,
	[Balloon] [float] NULL,
	[Fixed/Adj] [nvarchar](255) NULL,
	[Cpn (%)] [nvarchar](255) NULL,
	[Price] [float] NULL,
	[OAS (bps)] [float] NULL,
	[Book Price] [float] NULL,
	[BetaUp] [float] NULL,
	[BetaDn] [float] NULL,
	[Margin (%)] [float] NULL,
	[TeaserSpread] [float] NULL,
	[Index Mult] [float] NULL,
	[Index] [nvarchar](255) NULL,
	[Pay Freq] [nvarchar](255) NULL,
	[Method] [nvarchar](255) NULL,
	[RateResponseTable] [nvarchar](255) NULL,
	[Day Count] [nvarchar](255) NULL,
	[Lookback] [float] NULL,
	[Rem IO] [float] NULL,
	[Reset Freq] [nvarchar](255) NULL,
	[First Reset] [float] NULL,
	[Life Cap (%)] [nvarchar](255) NULL,
	[Life Floor (%)] [nvarchar](255) NULL,
	[Per Cap (%)] [nvarchar](255) NULL,
	[Per Floor (%)] [nvarchar](255) NULL,
	[Option Type] [nvarchar](255) NULL,
	[Exercise Freq] [nvarchar](255) NULL,
	[LockIn] [float] NULL,
	[ExpireIn] [float] NULL,
	[FundingSpd] [float] NULL,
	[Prem/Disc Method] [nvarchar](255) NULL,
	[PV Curve] [nvarchar](255) NULL,
	[SolveFor] [nvarchar](255) NULL,
	[SpreadRef] [nvarchar](255) NULL,
	[Volatility (%)] [nvarchar](255) NULL,
	[NonIntAdj] [float] NULL,
	[PPConstant] [float] NULL,
	[PPTableName] [nvarchar](255) NULL,
	[PPTableMult] [float] NULL,
	[PPModelName] [nvarchar](255) NULL,
	[PPModelMult] [float] NULL,
	[PPUnit] [nvarchar](255) NULL,
	[YieldHaircut (%)] [float] NULL,
	[IncomeTaxRate (%)] [float] NULL,
	[CapGainTaxRate (%)] [float] NULL,
	[TaxEquivAdj (%)] [float] NULL,
	[Svc Spd (%)] [float] NULL,
	[PPSpeedMin] [int] NULL,
	[PrepayLevel] [nvarchar](255) NULL,
	[PrincipalSwitch] [nvarchar](255) NULL
)";

        private string _regexMatchInputs = @"[nvarchar]
float
NVARCHAR(255)
[smallint]
[datetime]
[float]
(.*?)\t(.*?)\t(.*?)\r\n";

        private string _regexReplaceInputs = @"NVARCHAR
FLOAT
VARCHAR(50)
SMALLINT
DATETIME
FLOAT
$1\t$2\t$3\r\n";
        private bool _useRegex;
        private bool _isCaseSensitive = false;
        private bool _eachLine = false;
        private bool _multiline;
        public event PropertyChangedEventHandler PropertyChanged;

        

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
                            input = Regex.Replace(input, rr.RegexMatchPattern, rr.RegexReplacePattern, regexOptions);
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