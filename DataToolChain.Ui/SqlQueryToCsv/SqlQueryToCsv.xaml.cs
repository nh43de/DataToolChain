using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataToolChain.Ui.Extensions;
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

                using var r = sqlConn.ExecuteReader(Sql);

                var rows = 0;

                var rc = r.NotifyOn(new Progress<int>(i =>
                {
                    StatusMessage = $"{i} rows output.";
                    rows = i;
                }));

                rc.WriteCsv(FilePath);

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
