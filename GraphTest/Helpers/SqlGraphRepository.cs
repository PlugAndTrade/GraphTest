using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dapper;
using Tester.Interfaces;
using Tester.Model;

namespace Tester.Helpers
{
    public class SqlGraphRepository : IGraphRepository
    {
        private readonly string _connectionString;

        public SqlGraphRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Initialize(string sqlScript)
        {
            //return true;

            if (await IsInitialized())
                return;

            var parts = sqlScript.Split(new[] { $"go{Environment.NewLine}" }, StringSplitOptions.None);
            await using var connection = new SqlConnection(_connectionString);
            foreach (var part in parts)
            {
                await connection.ExecuteAsync(part);
            }
        }

        private async Task<bool> IsInitialized()
        {
            await using var connection = new SqlConnection(_connectionString);
            var exists = await connection.ExecuteScalarAsync<int>("select case when exists(SELECT * FROM sys.schemas WHERE name = 'graphtester') then 1 else 0 end as [exists]");
            return exists == 1;
        }

        public async IAsyncEnumerable<Graph> GetAll()
        {
            var nodes = new List<Node>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("graphtester.GraphGetAll", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            var doContinue = await reader.ReadAsync();
            while (doContinue)
            {
                var graphId = reader.GetInt32("GraphId");
                var name = reader.GetString("Name");

                nodes.Clear();
                while (reader.GetInt32("GraphId") == graphId)
                {
                    var nodeId = reader.GetString("NodeId");
                    var parentId = await reader.IsDBNullAsync("ParentNodeId") 
                        ? null
                        : reader.GetString("ParentNodeId");

                    nodes.Add(new Node
                    {
                        Id = nodeId,
                        ParentId = parentId,
                    });

                    if (await reader.ReadAsync())
                        continue;

                    doContinue = false;
                    break;

                }

                yield return new Graph
                {
                    Name = name,
                    Nodes = nodes.ToArray(),
                };
            }
        }
    }
}