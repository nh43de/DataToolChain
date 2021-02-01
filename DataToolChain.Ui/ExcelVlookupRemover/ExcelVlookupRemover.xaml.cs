using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using DataToolChain.Ui.ExcelFormulaExtractor;
using DataToolChain.Ui.Extensions;
using ExcelDataReader;
using Microsoft.Win32;

namespace DataToolChain.Ui.ExcelVlookupRemover
{

    public class ExcelVlookupRemoverViewModel : INotifyPropertyChanged
    {
        private string _outputText;
        private ObservableCollection<FilePathObject> _filePaths = new ObservableCollection<FilePathObject>();
        private string _destinationFileSuffix = "_clean";

        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilePathObject> FilePaths
        {
            get => _filePaths;
            set
            {
                _filePaths = value;
                OnPropertyChanged();
            }
        }

        public string DestinationFileSuffix
        {
            get => _destinationFileSuffix;
            set
            {
                _destinationFileSuffix = value;
                OnPropertyChanged();
            }
        }

        public static IEnumerable<string> ProcessFiles(string[] filePaths, string suffix)
        {
            foreach (var filePath in filePaths)
            {
                yield return filePath + ": " + ProcessFile(filePath, suffix);
            }
        }

        public static string ProcessFile(string filePath, string suffix)
        {
            try
            {
                var d = new ExcelVlookupRemoverHelpers();

                //append suffix
                var fp = Path.Combine(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(filePath));
                var fpx = Path.GetExtension(filePath);

                var n = fp + suffix + fpx;

                d.SanitizeVlookupFormulas(filePath, n);

                //not ideal but whatever
                return "Success";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public void Go()
        {
            var filePaths = _filePaths.Select(p => p.FilePath).ToArray();

            try
            {
                if (string.IsNullOrWhiteSpace(DestinationFileSuffix))
                    throw new Exception("Destination file suffix cannot be empty");

                var t = ProcessFiles(filePaths, DestinationFileSuffix).ToArray();
                
                OutputText = string.Join("\r\n", t);
            }
            catch (Exception ex)
            {
                OutputText = ex.ConcatenateInners();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExcelVlookupRemover : Window
    {
        public ExcelVlookupRemoverViewModel _viewModel { get; set; } = new ExcelVlookupRemoverViewModel();


        public ExcelVlookupRemover()
        {
            InitializeComponent();

            DataContext = _viewModel;
        }

        private void ButtonGoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.Go();
            }
            catch (Exception ex)
            {
                ex.DisplayInners();
            }
        }
        
        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            d.Multiselect = true;

            if (d.ShowDialog() == true)
            {
                _viewModel.FilePaths.AddRange(d.FileNames.Select(p => new FilePathObject(){FilePath = p}));
            }
        }

    }
}