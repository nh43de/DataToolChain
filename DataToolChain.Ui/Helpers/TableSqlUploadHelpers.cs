using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using System.Windows;
using DataPowerTools.Connectivity;

namespace DataToolChain
{
    public static class TableSqlUploadHelpers
    {
        public static string GetTableSql(IEnumerable<string> filePaths, string tableName, char csvDelimiter = ',', int headerOffsetRows = 1)
        {
            var readerFacs = new List<Func<DataReaderInfo>>();
            foreach (var filePath in filePaths)
            {
                var readerFac = new Func<DataReaderInfo>(
                    () => new DataReaderInfo
                    {
                        DataReader = DataReaderFactories.Default(filePath, true, csvDelimiter),
                        FilePath = filePath
                    });
                readerFacs.Add(readerFac);
            }

            return CreateTableSql.FromDataReader_Smart(tableName, readerFacs);
        }

        public static string GetTableSql(string filePath, string destinationTableName)
        {
            var readerFacs = new List<Func<DataReaderInfo>>();

            var readerFac = new Func<DataReaderInfo>(
                () => new DataReaderInfo
                {
                    DataReader = DataReaderFactories.Default(filePath),
                    FilePath = filePath
                });

            readerFacs.Add(readerFac);

            var outputText = CreateTableSql.FromDataReader_Smart(destinationTableName, readerFacs);

            return outputText;
        }
    }
}