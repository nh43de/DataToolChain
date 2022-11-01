using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace DataToolChain
{
    public static class DataUploadHelpers
    {
        public static void CreateTable(IProgress<string> progress, SqlConnection sqlc, string destinationTable, IEnumerable<string> filePaths)
        {
            //check table exists
            progress.Report("Checking if table exists.");

            if (Database.TableExists(sqlc, destinationTable))
            {
                var a = MessageBox.Show("Table with this name already exists. Replace?", "Warning", MessageBoxButton.YesNoCancel);

                if (a != MessageBoxResult.Yes)
                {
                    return;
                }

                progress.Report("Dropping Table");
                Database.DropTable(sqlc, destinationTable);
            }

            //generate create table sql
            progress.Report("Generating table SQL from file.");
            var tableSql = TableSqlUploadHelpers.GetTableSql(filePaths, destinationTable);

            //execute create table sql
            progress.Report("Creating destination table.");
            sqlc.ExecuteSql(tableSql);
        }

        public static async Task Upload(SqlConnection sqlc, IDataReader sourceReader, DataUploaderTask task, CancellationToken cancellationToken, string destinationTable, bool useOrdinals, int bulkCopyRowsPerBatch = 5000, DataTransformGroup transformGroup = null, string filterNullColumns = null)
        {
            long lastCopiedRow = -1;

            SmartDataReader<IDataReader> smartReader = null;//.ApplyTransformation(1, row => DataTransforms.TransformExcelDate(row[1]));
            
            sourceReader = sourceReader.CountRows();

            try
            {

                var reader = sourceReader;
                
                if (string.IsNullOrWhiteSpace(filterNullColumns) == false)
                {
                    var cols = filterNullColumns.Split(",");

                    reader = reader.Where(dataReader =>
                    {
                        return cols.All(col => !string.IsNullOrWhiteSpace(dataReader[col]?.ToString()));
                    });
                }
                
                smartReader = reader.MapToSqlDestination(destinationTable, sqlc, transformGroup);

                var progress = new Progress<int>(i =>
                {
                    task.StatusMessage = $"{i} Rows Copied";
                });

                var drr = smartReader
                    .NotifyOn(progress, bulkCopyRowsPerBatch)
                    .CountRows();
                
                await drr.BulkInsertSqlServerAsync(sqlc, destinationTable, new AsyncSqlServerBulkInsertOptions
                    {
                        BatchSize = bulkCopyRowsPerBatch,
                        CancellationToken = cancellationToken,
                        UseOrdinals = useOrdinals
                    });
                
                task.Success = true;

                task.StatusMessage = $"Finished - {sourceReader.Depth} Rows Read / {drr.Depth} Rows Copied";
            }
            //csv reader doesn't report depth
            catch (Exception ex)
            {
                if (lastCopiedRow == -1 && sourceReader.Depth <= 0)
                {
                    task.StatusMessage = "No rows were copied. ";
                }
                else
                {
                    task.StatusMessage = $"{sourceReader.Depth} total rows were read. ";
                }

                var inners = Try.Get(() => ex.ConcatenateInners(), "");

                var readerDiagnostics = Try.Get(() => smartReader?.PrintDiagnostics(), "");

                task.StatusMessage += "Exception occurred: " + inners + ". " + readerDiagnostics;
            }
        }

    }


}