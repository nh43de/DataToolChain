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
    public partial class StringColumnBuilder : Window
    {
        public StringColumnBuilderViewModel _viewModel { get; set; } = new StringColumnBuilderViewModel();

        public StringColumnBuilder()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }
    }


    public class StringColumnBuilderViewModel : INotifyPropertyChanged
    {
        private string _stringOutput;
        private string _stringInput = @"red
blue
cyan
magenta";
        private string _existingData = @"apple
dog
cat
fruit";
        private string _separator = @"\t";

        private bool _useRegex = true;
        private bool _isCaseSensitive = false;
        private bool _multiline;
        private bool _removeEmpty;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public bool UseRegex
        {
            get { return _useRegex; }
            set
            {
                _useRegex = value;
                UpdateOutput();
            }
        }
        
        public string Separator
        {
            get { return _separator; }
            set
            {
                _separator = value;
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

        public string ExistingData
        {
            get { return _existingData; }
            set
            {
                _existingData = value; 
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

        public StringColumnBuilderViewModel()
        {
            UpdateOutput();
        }

        private static readonly Regex LinesRegex = new Regex("\r\n?", RegexOptions.Compiled);

        public void UpdateOutput()
        {
            var matchesInput = Separator;

            try
            {
                //loop through existing data and append input into output

                var inputLines = LinesRegex.Split(StringInput);
                var dataLines = LinesRegex.Split(ExistingData);

                var sepString = UseRegex ? Regex.Unescape(Separator) : Separator;

                StringOutput = string.Join("\r\n", inputLines.Zip(dataLines, (input, data) => input + sepString + data));
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

