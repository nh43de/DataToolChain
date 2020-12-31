using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using DataToolChain.Ui.Extensions;
using ExcelDataReader;
using Microsoft.Win32;

namespace DataToolChain.Ui.ExcelFormulaExtractor
{

    public class FilePathObject
    {
        public string FilePath { get; set; }
    }

    public class ExcelFormulaExtractorViewModel : INotifyPropertyChanged
    {
        private string _outputText;
        private ObservableCollection<FilePathObject> _filePaths = new ObservableCollection<FilePathObject>();
        private string _tableName;

        public string OutputText
        {
            get { return _outputText; }
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilePathObject> FilePaths
        {
            get { return _filePaths; }
            set
            {
                _filePaths = value;
                OnPropertyChanged();
            }
        }

        public static IEnumerable<string> GetFilesFormulas(string[] filePaths)
        {
            yield return "File\tSheet\tColumn\tRow\tFormula";

            foreach (var filePath in filePaths)
            {
                foreach (var x in GetFileFormulas(filePath))
                {
                    yield return x;
                }
            }
        }

        public static IEnumerable<string> GetFileFormulas(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var sheetNumber = 1;

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            do
            {
                var row = 1;
                while (reader.Read())
                {
                    var cols = reader.FieldCount;

                    for (var col = 0; col < cols; col++)
                    {
                        var f = reader.GetFormula(col);

                        if (string.IsNullOrWhiteSpace(f) == false)
                            yield return $"{fileName}\t{sheetNumber}\t{col+1}\t{row}\t{f}";
                    }
                    row++;
                }
                sheetNumber++;
            } while (reader.NextResult()); //all sheets
        }

        public void Go()
        {
            var filePaths = _filePaths.Select(p => p.FilePath).ToArray();

            try
            {
                var t = GetFilesFormulas(filePaths).ToArray();
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
    public partial class ExcelFormulaExtractor : Window
    {
        public ExcelFormulaExtractorViewModel _viewModel { get; set; } = new ExcelFormulaExtractorViewModel();


        public ExcelFormulaExtractor()
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