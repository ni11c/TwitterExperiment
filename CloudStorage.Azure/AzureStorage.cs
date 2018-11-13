using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Nde.TwitterExperiment.CloudStorage.Azure
{
    public class AzureStorage : ICloudStorage
    {
        #region Properties

        private CloudStorageAccount Account { get; }

        #endregion

        #region Initialization

        public AzureStorage(string connectionString)
        {
            Console.WriteLine("Connecting to azure storage account...");
            if (CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                Account = storageAccount;
                Console.WriteLine("Success !");
            }
            else
            {
                Console.WriteLine("Connection failed :-(");
                throw new ApplicationException("Unable to connect to azure storage account with the connection string provided");
            }
        }

        #endregion

        #region Services

        public async Task<bool> InsertToTable<T>(T entity, string tableName)
        {
            var tableEntity = entity as TableEntity;
            if (tableEntity == null)
            {
                throw new ArgumentException($"Argument {entity} is not assignable to type {nameof(TableEntity)}. Cannot proceed with azure.");
            }

            var table = await CreateTableIfNotExistsAsync(tableName);
            var insertOperation = TableOperation.Insert(tableEntity);
            var result = await table.ExecuteAsync(insertOperation);
            return result.HttpStatusCode == 200;
        }

        #endregion

        #region Helpers

        private async Task<CloudTable> CreateTableIfNotExistsAsync(string tableName)
        {
            var tableClient = Account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        #endregion
    }
}