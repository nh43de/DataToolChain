using System;
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
    public partial class RegexSplitter : Window
    {
        public RegexSplitterViewModel _viewModel { get; set; } = new RegexSplitterViewModel();

        public RegexSplitter()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    public class RegexSplitterViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"The quick brown fox jumped over the lazy dog";
        private string _regexMatchInputs = @"\W+";

        private bool _useRegex = true;
        private bool _isCaseSensitive = false;
        private bool _multiline;
        private bool _removeEmpty;
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
        public bool RemoveEmpty
        {
            get { return _removeEmpty; }
            set
            {
                _removeEmpty = value;
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

        public RegexSplitterViewModel()
        {
            UpdateOutput();
        }

        public void UpdateOutput()
        {
            var matchesInput = RegexMatchInputs;

            try
            {
                var regexOptions = GetRegexOptions();

                var splitResults = Regex.Split(StringInput, UseRegex ? matchesInput : Regex.Escape(matchesInput), regexOptions, TimeSpan.FromMinutes(1));

                StringOutput = string.Join("\r\n", RemoveEmpty ? splitResults.Where(txt => string.IsNullOrWhiteSpace(txt) == false) : splitResults);
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

