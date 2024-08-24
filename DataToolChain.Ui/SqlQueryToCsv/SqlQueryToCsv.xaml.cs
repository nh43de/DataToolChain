using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DataPowerTools.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace DataToolChain
{
    /// <summary>
    /// Interaction logic for SqlQueryToCsv.xaml
    /// </summary>
    public partial class SqlQueryToCsv : Window
    {
        private SqlQueryToCsvViewModel _viewModel = new SqlQueryToCsvViewModel();

        private bool isRunning = false;

        public SqlQueryToCsv()
        {
            InitializeComponent();

            this.DataContext = _viewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if(isRunning)
                return;

            _viewModel.Password = this.PasswordBox.Password;

            isRunning = true;
            
            Task.Run(async () =>
            {
                await _viewModel.Go();
                _viewModel.Password = "";
                isRunning = false;
            });
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var d = new SaveFileDialog();
            
            if (d.ShowDialog() == true)
            {
                _viewModel.FilePath = d.FileName;
            }
        }

    }

    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            return null;
        }
    }

    public class SqlQueryToCsvViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "Ready.";
        private string _filePath = "output.csv";
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string ServerName { get; set; } = "";
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private int? _rowsPerBatch;

        public int? RowsPerBatch
        {
            get => _rowsPerBatch;
            set
            {
                if (_rowsPerBatch != value)
                {
                    _rowsPerBatch = value;
                    OnPropertyChanged(nameof(RowsPerBatch));
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string Sql { get; set; }


        public async Task Go()
        {
            try
            {
                var sqlcstr = new SqlConnectionStringBuilder
                {
                    InitialCatalog = Database,
                    DataSource = ServerName,
                    TrustServerCertificate = true
                };

                if (string.IsNullOrEmpty(this.Username))
                {
                    sqlcstr.IntegratedSecurity = true;
                }
                else
                {
                    //a.IntegratedSecurity = false;
                    sqlcstr.UserID = Username;
                    sqlcstr.Password = Password;
                }
                
                using var sqlConn = sqlcstr.ConnectionString.CreateSqlConnection();

                //sqlConn.Open();
                StatusMessage = $"Executing reader ...";
                
                using var r = sqlConn.ExecuteReader(Sql);

                var rows = 0;
                StatusMessage = "Starting copy ...";

                if (RowsPerBatch.HasValue)
                {
                    var rr = r.Batch(RowsPerBatch.Value);

                    var fileNum = 0;

                    foreach (var dataReader in rr)
                    {
                        fileNum++;

                        var rc = dataReader.NotifyOn(new Progress<int>(i =>
                        {
                            StatusMessage = $"File {fileNum} - {i} rows output.";
                            rows = i;
                        }));

                        var filePath = $"{Path.GetDirectoryName(FilePath)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(FilePath)}_{fileNum}{Path.GetExtension(FilePath)}";
                        
                        rc.WriteCsv(filePath);
                    }
                }
                else
                {
                    var rc = r.NotifyOn(new Progress<int>(i =>
                    {
                        StatusMessage = $"{i} rows output.";
                        rows = i;
                    }));

                    rc.WriteCsv(FilePath);
                }


                StatusMessage = $"Finished: {rows} rows output.";
            }
            catch (Exception e)
            {
                StatusMessage = e.ConcatenateInners();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
