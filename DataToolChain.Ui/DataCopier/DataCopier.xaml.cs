using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using DataToolChain.Ui.Extensions;
using Microsoft.Data.SqlClient;

namespace DataToolChain
{
    /// <summary>
    ///     Interaction logic for DataUploader.xaml
    /// </summary>
    public partial class DataCopier : Window
    {
        public DataCopier()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public DataCopierViewModel _viewModel { get; set; } = new DataCopierViewModel();

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SourceConfig.Password = SourcePasswordBox.Password;
            _viewModel.DestinationConfig.Password = DestinationPasswordBox.Password;
            _viewModel.Go();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Cancel();
        }
    }

    public class DataCopierConfig : INotifyPropertyChanged
    {
        private string _dbName;
        private string _destinationTable;
        private string _serverName = "my-sql-server01";

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                OnPropertyChanged();
            }
        }

        public string DestinationTable
        {
            get { return _destinationTable; }
            set
            {
                _destinationTable = value;
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

        public string ConnectionString
        {
            get
            {
                var a = new SqlConnectionStringBuilder
                {
                    InitialCatalog = DbName,
                    DataSource = ServerName
                };

                if (string.IsNullOrEmpty(Username))
                {
                    a.IntegratedSecurity = true;
                }
                else
                {
                    //a.IntegratedSecurity = false;
                    a.UserID = Username;
                    a.Password = Password;
                }

                return a.ConnectionString;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class DataCopierViewModel : INotifyPropertyChanged
    {
        private string _statusDisplay;
        private bool _copyAllTables;
        private bool _deleteDestinationData;

        public DataCopierConfig SourceConfig { get; set; } = new DataCopierConfig();
        public DataCopierConfig DestinationConfig { get; set; } = new DataCopierConfig();

        public bool InputTableEnabled => !CopyAllTables;

        public bool CopyAllTables
        {
            get { return _copyAllTables; }
            set
            {
                _copyAllTables = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InputTableEnabled));
            }
        }

        public DataCopierViewModel()
        {
            this._progress = new Progress<string>((str) => WindowStatusDisplay = str);
        }

        public bool KeepNulls { get; set; }
        public bool KeepIdentity { get; set; }
        public bool CheckConstraints { get; set; }

        public bool DeleteDestinationData
        {
            get { return _deleteDestinationData; }
            set
            {
                _deleteDestinationData = value;
                OnPropertyChanged();
            }
        }


        public int BulkCopyRowsPerBatch { get; set; } = 5000;

        public bool UseOrdinals { get; set; } = false;

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
        
        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            this._progress.Report("Operation cancelled");
        }

        readonly IProgress<string> _progress;

        public void Go()
        {
            CancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = CancellationTokenSource.Token;

            CurrentTask = Task.Run(async() => await Go(cancellationToken), cancellationToken);
        }

        private async Task Go(CancellationToken cancellationToken)
        {
            this._progress.Report("Starting process");
            
            try
            {
                using (var src = new SqlConnection(SourceConfig.ConnectionString))
                using (var dest = new SqlConnection(DestinationConfig.ConnectionString))
                {
                    src.Open();
                    dest.Open();

                    if (CopyAllTables == false)
                    {
                        var sourceTableName = SourceConfig.DestinationTable;
                        var destinationTableName = DestinationConfig.DestinationTable;

                        using (var sqlr = new SqlCommand($"SELECT * FROM {sourceTableName}", src).ExecuteReader())
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                                
                            if (DeleteDestinationData)
                            {
                                await Database.DeleteFromTableAsync(destinationTableName, dest, cancellationToken);
                            }

                            await sqlr.BulkInsertSqlServerAsync(DestinationConfig.ConnectionString, DestinationConfig.DestinationTable, new AsyncSqlServerBulkInsertOptions
                            {
                                BatchSize = BulkCopyRowsPerBatch,
                                UseOrdinals = UseOrdinals,
                                RowsCopiedEventHandler = (i) => this._progress.Report(i + " Rows Copied"),
                                CancellationToken = cancellationToken
                            });
                        }
                    }
                    else
                    {
                        if (DeleteDestinationData)
                        {
                            _progress.Report("Deleting destination data.");

                            var tableHierarchyDest = Database.GetTableHierarchy(dest);
                            foreach (var h in tableHierarchyDest.OrderBy(h => h.Level))
                            {
                                var tableName = $"[{h.SchemaName}].[{h.TableName}]";

                                cancellationToken.ThrowIfCancellationRequested();

                                var dropSql = $"DELETE FROM {tableName};";

                                await dest.ExecuteSqlAsync(dropSql, null, cancellationToken);
                            }
                        }

                        var tableHierarchy = Database.GetTableHierarchy(src);
                        var sourceTables = tableHierarchy.OrderByDescending(a => a.Level).ToArray();

                        foreach (var sourceTableInfo in sourceTables)
                        {
                            var sourceTable = $"[{sourceTableInfo.SchemaName}].[{sourceTableInfo.TableName}]";

                            cancellationToken.ThrowIfCancellationRequested();

                            _progress.Report($"Copying data from {sourceTable}");

                            retry:
                            try
                            {
                                using (var cmd = new SqlCommand($"SELECT * FROM {sourceTable}", src) { CommandTimeout = 0 })
                                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                                using (var smartReader = reader.MapToSqlDestination(sourceTable, dest, DataTransformGroups.None))
                                {
                                    var sqlBulkCopyOptions = SqlBulkCopyOptions.Default;

                                    if (CheckConstraints)
                                        sqlBulkCopyOptions |= SqlBulkCopyOptions.CheckConstraints;
                                    
                                    if (KeepNulls)
                                        sqlBulkCopyOptions |= SqlBulkCopyOptions.KeepNulls;

                                    if (KeepIdentity)
                                        sqlBulkCopyOptions |= SqlBulkCopyOptions.KeepIdentity;
                                    
                                    await smartReader.BulkInsertSqlServerAsync(connection: dest, destinationTableName: sourceTable, new AsyncSqlServerBulkInsertOptions
                                    {
                                        BatchSize = BulkCopyRowsPerBatch,
                                        UseOrdinals = UseOrdinals,
                                        RowsCopiedEventHandler = (i) => this._progress.Report(i + $" Rows Copied ( {sourceTable} )"),
                                        CancellationToken = cancellationToken,
                                        SqlBulkCopyOptions = sqlBulkCopyOptions
                                    });

                                    if (cancellationToken.IsCancellationRequested)
                                        cmd.Cancel();
                                }
                            }
                            catch (Exception e)
                            {
                                _progress.Report("Operation aborted: " + e.Message);
                            }
                        }
                    }
                }
                this._progress.Report("Finished");
            }
            catch (Exception ex)
            {
                ex.DisplayInners("Failed to upload");
            }
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}