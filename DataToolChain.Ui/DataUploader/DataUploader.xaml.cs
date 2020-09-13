using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DataPowerTools.Connectivity;
using DataPowerTools.Connectivity.Json;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace DataToolChain
{
    /// <summary>
    ///     Interaction logic for DataUploader.xaml
    /// </summary>
    public partial class DataUploader : Window
    {
        public DataUploader()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public DataUploaderViewModel _viewModel { get; set; } = new DataUploaderViewModel();


        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var a = new OpenFileDialog();
            a.Multiselect = true;

            if (a.ShowDialog() == true)
            {
                _viewModel.AddFiles(a.FileNames);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Cancel();
        }

        private void DataGrid_OnCurrentCellChanged(object sender, EventArgs e)
        {
            _viewModel.SaveConfig();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = this.PasswordBox.Password;

            Task.Run(async () =>
            {
                try
                {
                    await _viewModel.Go();
                }
                catch (Exception ex)
                {
                    _viewModel.WindowStatusDisplay = "Finished with errors: " + ex.Message;
                }
            });
        }
    }

    public class DataUploaderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DataUploaderTask> _dataUploaderTasks = new ObservableCollection<DataUploaderTask>();
        private string _dbName;
        private string _destinationTable = "";
        private string _serverName = "localhost";
        private string _statusDisplay;
        private string _jsonConfiguration;
        private bool _applyDefaultTransformGroup = true;
        private bool _createTable = false;
        private bool _truncateTable = false;


        public DataUploaderViewModel()
        {
            _dataUploaderTasks.CollectionChanged += DataUploaderTasksOnCollectionChanged;
        }

        private void DataUploaderTasksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveConfig();
        }

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                OnPropertyChanged();
            }
        }

        public bool CreateTable
        {
            get { return _createTable; }
            set
            {
                _createTable = value; 
                OnPropertyChanged();
            }
        }

        public bool TruncateTableFirst
        {
            get { return _truncateTable; }
            set
            {
                _truncateTable = value;
                OnPropertyChanged();
            }
        }

        public int BulkCopyRowsPerBatch { get; set; } = 5000;

        public bool UseOrdinals { get; set; } = false;

        public string DestinationTable
        {
            get { return _destinationTable; }
            set
            {
                _destinationTable = value;
                OnPropertyChanged();
            }
        }

        public string JsonConfiguration
        {
            get { return _jsonConfiguration; }
            set
            {
                _jsonConfiguration = value;
                LoadConfig();
                OnPropertyChanged();
            }
        }

        private class JsonConfig
        {
            public string ServerName { get; set; }
            public string DbName { get; set; }
            public string DestinationTable { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public bool TruncateTables { get; set; } 

            public DataUploaderTask[] Tasks { get; set; }
        }

        public void LoadConfig()
        {
            try
            {
                var a = JsonConfiguration.ToObject<JsonConfig>();
                ServerName = a.ServerName;
                DbName = a.DbName;
                DestinationTable = a.DestinationTable;
                Username = a.Username;
                Password = a.Password;
                TruncateTableFirst = a.TruncateTables;

                DataUploaderTasks = new ObservableCollection<DataUploaderTask>(a.Tasks);
            }
            catch (Exception e)
            {
                //nah
            }
        }

        public void SaveConfig()
        {
            var a = new JsonConfig
            {
                ServerName = ServerName,
                DbName = DbName,
                DestinationTable = DestinationTable,
                Username = Username,
                Password = Password,
                Tasks = DataUploaderTasks.ToArray(),
                TruncateTables = TruncateTableFirst
            };
            var b = a.ToJson(true);
            if (JsonConfiguration != b)
            {
                JsonConfiguration = b;
            }
        }

        public bool ApplyDefaultTransformGroup
        {
            get { return _applyDefaultTransformGroup; }
            set
            {
                _applyDefaultTransformGroup = value;
                OnPropertyChanged();
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string DbName
        {
            get { return _dbName; }
            set
            {
                _dbName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DataUploaderTask> DataUploaderTasks
        {
            get { return _dataUploaderTasks; }
            set
            {
                _dataUploaderTasks = value;
                OnPropertyChanged();
            }
        }

        public string WindowStatusDisplay
        {
            get { return _statusDisplay; }
            set
            {
                _statusDisplay = value;
                OnPropertyChanged();
            }
        }

        public Task CurrentTask { get; private set; }

        private CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                AddFile(file);
            }
            SaveConfig();
        }

        public void AddFile(string file)
        {
            DataUploaderTasks.Add(new DataUploaderTask {FilePath = file});
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            WindowStatusDisplay = "Operation cancelled";

            //TODO: this should be async and wait for the current task to complete (fail) before changing display
        }


        public async Task Go()
        {
            if (CurrentTask != null && !CurrentTask.IsCompleted)
            {
                MessageBox.Show("A process is currently running.");
                return;
            }

            DataUploaderTasks.ToList().ForEach(t =>
            {
                t.Success = false;
                t.StatusMessage = "";s
            });

            var cancellationToken = CancellationTokenSource.Token;

            CurrentTask = null;

            WindowStatusDisplay = "Starting upload.";

            var a = new SqlConnectionStringBuilder
            {
                InitialCatalog = DbName,
                DataSource = ServerName
            };

            if (string.IsNullOrEmpty(this.Username))
            {
                a.IntegratedSecurity = true;
            }
            else
            {
                //a.IntegratedSecurity = false;
                a.UserID = Username;
                a.Password = Password;
            }

            var connstring = a.ConnectionString;

            //long lastCopiedRow = -1;
            
            if (this.CreateTable) //TODO: group by all tables and create
            {
                WindowStatusDisplay = "Creating table.";

                using (var sqlc = new SqlConnection(connstring))
                {
                    sqlc.Open();
                    DataUploadHelpers.CreateTable(new Progress<string>(s => WindowStatusDisplay = s), sqlc, DestinationTable, DataUploaderTasks.Select(u => u.FilePath));
                }
            } else if (this.TruncateTableFirst)
            {
                WindowStatusDisplay = "Truncating table(s).";

                var tablesToTruncate = DataUploaderTasks
                    .Select(GetDestinationTable)
                    .Where(t => string.IsNullOrWhiteSpace(t) == false)
                    .Distinct()
                    .ToArray();

                using (var sqlc = new SqlConnection(connstring))
                {
                    sqlc.Open();

                    foreach (var table in tablesToTruncate)
                    {
                        sqlc.ExecuteSql("TRUNCATE TABLE " + table + ";");
                    }
                }          
            }

            CurrentTask = Task.Run(async () =>
            {
                foreach (var task in DataUploaderTasks)
                {
                    if (string.IsNullOrWhiteSpace(task.FilePath))
                    {
                        task.StatusMessage = "No file specified";
                    }

                    var reader = DataReaderFactories.Default(task.FilePath);

                    if(!UseOrdinals)
                        reader = reader.AddColumn("FilePath", v => task.FilePath);

                    using (reader)
                    using(var sqlc = new SqlConnection(connstring))
                    {
                        sqlc.Open();

                        var destinationTable = GetDestinationTable(task);

                        await DataUploadHelpers.Upload(sqlc, reader, task, cancellationToken, destinationTable, UseOrdinals, BulkCopyRowsPerBatch, ApplyDefaultTransformGroup ? (DataTransformGroup)DataTransformGroups.Default : (DataTransformGroup)DataTransformGroups.None);
                    }
                }
            }, cancellationToken);

            CurrentTask?.GetAwaiter().OnCompleted(() =>
            {
                CurrentTask = null;
                WindowStatusDisplay = "Finished.";
            });
        }
        
        private string GetDestinationTable(DataUploaderTask task)
        {
            var destinationTable = string.IsNullOrWhiteSpace(task.DestinationTable) ? DestinationTable : task.DestinationTable;

            if (string.IsNullOrWhiteSpace(destinationTable))
            {
                //last check get from file name without extension
                destinationTable = System.IO.Path.GetFileNameWithoutExtension(task.FilePath);
            }

            return destinationTable;
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            SaveConfig();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
