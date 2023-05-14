using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace DataToolChain
{
    /// <summary>
    /// Interaction logic for RegexMatcher.xaml
    /// </summary>
    public partial class RegexMatcher : Window
    {
        public RegexMatcherViewModel _viewModel { get; set; } = new RegexMatcherViewModel();

        public RegexMatcher()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    public class RegexMatcherViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"The {quick} [[brown]] +fox jumped|| over^ the >> lazy <<dog";
        private string _regexMatchInputs = @"the
quick
brown
fox";

        private bool _useRegex;
        private bool _isCaseSensitive = false;
        private bool _multiline;
        private bool _printGroups;
        private bool _unique; 
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set
            {
                _isCaseSensitive = value;
                UpdateOutput();
            }
        }

        /// <summary>
        /// A little confusing, but when this is true it sets regex Multiline option = false.
        /// </summary>
        public bool Multiline
        {
            get { return _multiline; }
            set
            {
                _multiline = value;
                UpdateOutput();
            }
        }

        public bool Unique
        {
            get { return _unique; }
            set
            {
                _unique = value;
                UpdateOutput();
            }
        }

        public bool PrintGroups
        {
            get { return _printGroups; }
            set
            {
                _printGroups = value;
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

        public RegexMatcherViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            try
            {
                var regexOptions = GetRegexOptions();

                var r = Match(RegexMatchInputs, regexOptions, StringInput, UseRegex, _printGroups, Unique);

                if (Unique)
                {
                    StringOutput = string.Join("\r\n", r);
                }
                else
                {
                    StringOutput = string.Join("\r\n", r);
                }
            }
            catch (Exception)
            {
                StringOutput = "Error in regex.";
                return;
            }
        }

        public static string[] Match(string regexMatchInputs, RegexOptions regexOptions, string stringInput, bool useRegex, bool printGroups, bool unique)
        {
            var matchesInput = regexMatchInputs.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var patternStringDictionary = new StringStartingPosDictionary();

            try
            {
                var dd = matchesInput.SelectMany(
                    pattern =>
                        Regex.Matches(stringInput, useRegex ? pattern : Regex.Escape(pattern), regexOptions)
                            .OfType<Match>()
                            .Where(m => m.Success));

                foreach (var m in dd)
                {
                    patternStringDictionary.Add(m.Index, printGroups ? m.Groups[1].Value : m.Value);
                }

                if (unique)
                {
                    var r = patternStringDictionary.GetValues().Distinct().ToArray();

                    return r;
                }
                else
                {
                    var r = patternStringDictionary.GetValues().ToArray();

                    return r;
                }
            }
            catch (Exception)
            {
                return new []{ "Error in regex." };
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Stores strings keyed by their starting position
        /// </summary>
        private class StringStartingPosDictionary
        {
            private Dictionary<int, string> StartingPosDictionary { get; set; } = new Dictionary<int, string>();
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pos">The starting position of the match</param>
            /// <param name="value"></param>
            public void Add(int pos, string value)
            {
                if (StartingPosDictionary.ContainsKey(pos) == false)
                {
                    StartingPosDictionary.Add(pos, value);
                }
            }

            public IEnumerable<string> GetValues()
            {
                return StartingPosDictionary.Select(d => d.Value);
            }
        }

    }
}

