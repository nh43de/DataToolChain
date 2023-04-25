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
            var matchesInput = RegexMatchInputs.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var patternStringDictionary = new StringLengthDictionary();

            try
            {
                var regexOptions = GetRegexOptions();

                var dd = matchesInput.SelectMany(
                    pattern =>
                        Regex.Matches(StringInput, UseRegex ? pattern : Regex.Escape(pattern), regexOptions)
                            .OfType<Match>()
                            .Where(m => m.Success));

                foreach (var m in dd)
                {
                    patternStringDictionary.Add(m.Index, m.Value.Trim());
                }

                if (Unique)
                {
                    StringOutput = string.Join("\r\n", patternStringDictionary.GetValues().Distinct());
                }
                else
                {
                    StringOutput = string.Join("\r\n", patternStringDictionary.GetValues());
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

        private class StringLengthDictionary
        {
            private Dictionary<int, string> PatternStringDictionary { get; set; } = new Dictionary<int, string>();
            
            public void Add(int pos, string value)
            {
                if (PatternStringDictionary.ContainsKey(pos) == false)
                {
                    PatternStringDictionary.Add(pos, value);
                }
            }

            public IEnumerable<string> GetValues()
            {
                return PatternStringDictionary.Select(d => d.Value);
            }


        }

    }
}

