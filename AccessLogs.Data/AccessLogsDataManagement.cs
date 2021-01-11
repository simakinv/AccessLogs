using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AccessLogs.Model;

namespace AccessLogs.Data
{
    public class AccessLogsDataManagement
    {
        private readonly string _connectionString;

        public AccessLogsDataManagement(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public void SaveLogs(IEnumerable<Log> logs)
        {
            using var connection = new SqlConnection(_connectionString);
            
            SqlBulkCopy bulk = new SqlBulkCopy(connection)
            {
                DestinationTableName = "Logs",
                BatchSize = 250000
            };

            using var data = new DataTable();
            var properties = typeof(Log).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in properties)
            {
                data.Columns.Add(propertyInfo.Name, propertyInfo.PropertyType);
                bulk.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
            }

            foreach (var entity in logs)
            {
                var dataRow = data.NewRow();

                foreach (var propertyInfo in properties)
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(entity, null);
                }

                data.Rows.Add(dataRow);
            }

            connection.Open();
            bulk.WriteToServer(data);
        }

        public async Task<IEnumerable<string>> GetTopClients(int topN, 
            DateTimeOffset from, 
            DateTimeOffset to, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "GetTopClients";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(new []
            {
                new SqlParameter("topN", topN), 
                new SqlParameter("from", from), 
                new SqlParameter("to", to),  
            });

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var clients = new List<string>();
            
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                clients.Add((string)reader[0]);
            }

            return clients;
        }
    }
}
