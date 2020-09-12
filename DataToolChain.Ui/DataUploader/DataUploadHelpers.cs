using System;
using System.Collections.Generic;
using System.Data;
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

        public static async Task Upload(SqlConnection sqlc, IDataReader reader, DataUploaderTask task, CancellationToken cancellationToken, string destinationTable, bool useOrdinals, int bulkCopyRowsPerBatch = 5000, DataTransformGroup transformGroup = null)
        {
            long lastCopiedRow = -1;

            SmartDataReader<IDataReader> r = null;//.ApplyTransformation(1, row => DataTransforms.TransformExcelDate(row[1]));

            try
            {
                r = reader.MapToSqlDestination(destinationTable, sqlc, transformGroup);
                 
                await r.BulkInsertSqlServerAsync(sqlc, destinationTable, new AsyncSqlServerBulkInsertOptions
                {
                    RowsCopiedEventHandler = i =>
                    {
                        task.StatusMessage = $"{i} Rows Copied";
                        lastCopiedRow = i;
                    },
                    BatchSize = bulkCopyRowsPerBatch,
                    CancellationToken = cancellationToken,
                    UseOrdinals = useOrdinals
                });
                
                task.Success = true;
                task.StatusMessage = $"Finished - {reader.Depth} Rows Copied";
            }
            //csv reader doesn't report depth
            catch (Exception ex)
            {
                if (lastCopiedRow == -1 &&  reader.Depth <= 0)
                {
                    task.StatusMessage = "No rows were copied. ";
                }
                else
                {
                    task.StatusMessage = $"{reader.Depth} total rows were read. ";
                }

                var inners = Try.Get(() => ex.ConcatenateInners(), "");

                var readerDiagnostics = Try.Get(() => r?.PrintDiagnostics(), "");

                task.StatusMessage += "Exception occurred: " + inners + ". " + readerDiagnostics;
            }
        }

    }


}