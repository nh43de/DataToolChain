using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DataToolChain.Ui.Extensions;
using Microsoft.Win32;

namespace DataToolChain
{
    public class FilePathObject
    {
        public string FilePath { get; set; }
    }

    public class CsvToCreateTableViewModel : INotifyPropertyChanged
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

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                OnPropertyChanged();
            }
        }

        public bool UseTabDelimiter { get; set; } = true;
        public bool UseCommaDelimiter { get; set; }
        public int HeaderOffsetRows { get; set; } = 1;

        public void Go()
        {
            var filePaths = _filePaths.Select(p => p.FilePath).ToArray();
            var tableName = TableName;
            var csvDelimiter = UseTabDelimiter ? '\t' : ',';
            var headerOffsetRows = HeaderOffsetRows;

            try
            {
                OutputText = TableSqlUploadHelpers.GetTableSql(filePaths, tableName, csvDelimiter, headerOffsetRows);
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
    public partial class CsvToCreateTable : Window
    {
        public CsvToCreateTableViewModel _viewModel { get; set; } = new CsvToCreateTableViewModel();


        public 
            CsvToCreateTable()
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
                _viewModel.TableName = Path.GetFileNameWithoutExtension(d.FileName);
            }
        }
    }
}